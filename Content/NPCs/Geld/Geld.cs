using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.GameContent.Bestiary;
using Terraria;
using Terraria.Localization;
using Terraria.ID;
using Terraria.GameContent.ItemDropRules;
using System.Collections.Generic;
using Brigade.Content.Items.Weapons.Magic;
using Brigade.Core.System;
using Terraria.Graphics;
using Terraria.Audio;
using Brigade.Core.ModPlayers;
using Terraria.Graphics.CameraModifiers;


namespace Brigade.Content.NPCs.Geld {
    [AutoloadBossHead]
    internal class Geld : ModNPC {
        // Boss will have more than two stages, so we will use integers to denote the stage.

        public static int secondStageHeadSlot = -1;

        public int BossStage {
            get => (int)NPC.ai[0];
            set => NPC.ai[0] = value;
        }

        public Vector2 FirstStageDestination {
            get => new Vector2(NPC.ai[1], NPC.ai[2]);
            set {
                NPC.ai[1] = value.X;
                NPC.ai[2] = value.Y;
            }
        }

        public Vector2 LastFirstStageDestination { get; set; } = Vector2.Zero;

        public int FirstStageTimerMax = 90;

        // Reference propertly, allows us to write NPC.localAI[1] as FirstStageTimer, which makes code easier to read.
        public ref float FirstStageTimer => ref NPC.localAI[1];


        // This results in the death message being  
        public override LocalizedText DeathMessage => Language.GetText("Announcement.HasBeenDefeated_Single").WithFormatArgs(this.GetLocalization("BossFightName"));

        public override void Load() {
            string tex = BossHeadTexture + "_SecondStage";
            secondStageHeadSlot = Mod.AddBossHeadTexture(tex, -1);
        }

        public override void BossHeadSlot(ref int index) {
            int slot = secondStageHeadSlot;

            // If the boss is already in phase 2, then we switch to that icon immediately.
            if (BossStage == 2 && slot != -1) {
                index = slot;
            }
        }

        public override void SetStaticDefaults() {
            Main.npcFrameCount[Type] = 6;

            // If we want a summoning item for this boss, we add this here.
            // Probably will change but for now we will use a summoning item.
            NPCID.Sets.MPAllowedEnemies[Type] = true;

            // Automatically group this boss with other boss-type enemies in Bestiary
            NPCID.Sets.BossBestiaryPriority.Add(Type);

            // Specify buffs they are immune to
            // Most in-game NPCs are already immune to Confused and Poisoned
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;

            NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers() {
                CustomTexturePath = "Brigade/Assets/Textures/Bestiary/Geld_Preview",

                // When modifying portrait, it refers to the full-size image when clicking on the boss in the Bestiary
                PortraitScale = 0.6f,
                PortraitPositionYOverride = 0
            };

            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
        }
        
        public override void SetDefaults() {
            NPC.width = 110;
            NPC.height = 110;

            NPC.damage = 12;
            NPC.defense = 10;
            NPC.lifeMax = 2700;

            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;

            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;

            NPC.value = Item.buyPrice(gold: 5);
            NPC.SpawnWithHigherTime(30);
            NPC.boss = true;
            NPC.npcSlots = 10f; // Takes up open spawn slots, preventing random NPCs from spawning during the fight.

            // Default buff immunities should be set in SetStaticDefaults.
            // In order to dynamically adjust immunities of an active NPC, NPC.buffImmune[] can be changed in AI.
            // This will not preserve the buff immunities though, to preserve them, we need to use NPC.BecomeImmuneTo and NPC.ClearImmuneBuffs.
            // Check the second stage transition for more details.

            // Setting it to -1 makes it to the NPC has custom AI.
            // A value of 0 sets it to "Bound Town NPC" which will slow down the NPC itself and change the sprite orientation towards the target.
            NPC.aiStyle = -1;

            // Custom Boss Bar
            // none in place yet... :(
            //NPC.BossBar = ModContent.GetInstance<GeldBossBar>();

            // Following code will assign a custom music track specifically for this boss.
            if (!Main.dedServ) {
                //Music = MusicLoader.GetMusicSlot(Mod, "Asset/Music/Ropocalypse2");
                Music = MusicID.OtherworldlyBoss1;

                if (!Main.swapMusic == Main.drunkWorld && !Main.remixWorld) {
                    Music = MusicID.OtherworldlyBoss1;
                }
            }

        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
            // Sets the description of this NPC that is listed in the Bestiary.
            bestiaryEntry.Info.AddRange(new List<IBestiaryInfoElement> {
                new MoonLordPortraitBackgroundProviderBestiaryInfoElement(), // Plain black background
                new FlavorTextBestiaryInfoElement("Mods.Brigade.Bestiary.Geld")
            });
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot) {
            // Do not misuse ModifyNPCLoot and OnKill hooks.
            // This is only used for registering drops.

            // The order in which you add loot will appear as such in the bestiary. The vanilla convention is:
            // 1. Trophy
            // 2. Classic Mode ("stuff thats not expert")
            // 3. Expert Mode (usually just the treasure bag)
            // 4. Master Mode (relic first, pet last, everything else inbetween)

            // Trophies are spawned with a 1/10 chance.
            // i dont have one made yet
            //npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Items.Placeable.Furniture.GeldBossTrophy>(), 10));

            // All the classic mode drops here are based on "not expert"
            // Thus, we need to use .OnSuccess() to add them into the rule.
            LeadingConditionRule notExpertRule = new LeadingConditionRule(new Conditions.NotExpert());

            // We only use .OnSuccess() and not npcLoot.Add() so it only applies in normal mode.
            notExpertRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<StarcallerStaff>(), 7));

            // Add the leading rule to npcLoot
            npcLoot.Add(notExpertRule);

            // Add the treasure bag using ItemDropRule.BossBag, will automatically check for expert mode
            // need to make boss bag too
            //npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<GeldBossBag>()));

            // ItemDropRule.MasterModeCommonDrop for the relic
            //npcLoot.Add(ItemDropRule.MasterModeCommonDrop(ModContent.ItemType<Items.Placeable.Furniture.GeldBossRelic>()));

            // ItemDropRule.MasterModeDropOnAllPlayers for the pet
            //npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<GeldBossPetItem>(), 4));
        }

        public override void OnKill() {
            // If we want this boss to spawn ore into the world when its first killed, we do this:
            // This code is above SetEventFlagCleared because that will set downedGeldBoss to true.
            if (!DownedBossSystem.downedGeldBoss) {
                //ModContent.GetInstance<BrigadeOreSystem>().BlessWorldWithOre();
            }

            // Sets our downedGeldBoss flag to true, if it was false before, it starts a lantern night.
            NPC.SetEventFlagCleared(ref DownedBossSystem.downedGeldBoss, -1);

            // This hook is only run on singleplayer and serverside, thus we would need to sync it manually.
            // Vanilla sends the MessageID.WorldData packet if a BOSS was killed automatically, shortly after this hook is ran.

            // If the NPC is not a Boss and you need to sync the world (includes ModSystem, check DownedBossSystem with NetSend/NetReceive), use this code:
            /*
            if (Main.netMode == NetmodeID.Server) {
                NetMessage.SendData(MessageID.WorldData);
            }
            */
        }

        public override void BossLoot(ref int potionType) {
            // Here, we'd change the potion type that the boss drops usually.
            // If we wanted to change it, simply write:
            // potionType = ItemID.LesserHealingPotion; or any other potion type.
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot) {
            cooldownSlot = ImmunityCooldownID.Bosses; // Use the bosses immunity cooldown counter to prevent ignoring boss attacks by taking damage from other sources.
            return true;
        }

        public override void FindFrame(int frameHeight) {
            // This is used to animate the NPC.
            // This boss uses a simple "go from start to final frame and loop back to the start" rule for its animation.
            // In this case, First Stage: 0 1 2, 0 1 2, Second Stage: 3 4 5, 3 4 5 being "total frame count - 1"
            int startFrame = 0;
            int endFrame = 2;

            if (BossStage == 2) {
                startFrame = 3;
                endFrame = Main.npcFrameCount[Type] - 1;

                if (NPC.frame.Y < startFrame * frameHeight) {
                    // If we are animating the first stage frames and then switch to the second stage, immediately change to the start of the frame of the second stage.

                    NPC.frame.Y = startFrame * frameHeight;
                }
            }

            int frameSpeed = 5;
            NPC.frameCounter += 0.5f;
            NPC.frameCounter += NPC.velocity.Length() / 10f; // Make the counter go faster with higher movement speed
            if (NPC.frameCounter > frameSpeed) {

                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;

                if (NPC.frame.Y > endFrame * frameHeight) {
                    NPC.frame.Y = startFrame * frameHeight;
                }


            }

        }

        public override void HitEffect(NPC.HitInfo hit) {
            // If NPC dies, spawn gore and play a SFX
            if (Main.netMode == NetmodeID.Server) {
                // Do not run Mod.Find<ModGore> on servers because gores aren't loaded on servers and game will go boom!
                return;
            }

            if (NPC.life <= 0) {
                // These gores will work by simply existing as a texture in any folder path which contains "Gores/"
                // Gores need to be made
                //int backGoreType = Mod.Find<ModGore>("GeldBossBody_Back").Type;
                //int frontGoreType = Mod.Find<ModGore>("GeldBossBody_Front").Type;

                //var entitySource = NPC.GetSource_Death();

                //for (int i=0; i<2; i++) {
                //    Gore.NewGore(entitySource, NPC.position, new Vector2(Main.rand.Next(-6, 7), Main.rand.Next(6, -7)), backGoreType);
                //    Gore.NewGore(entitySource, NPC.position, new Vector2(Main.rand.Next(-6, 7), Main.rand.Next(6, -7)), frontGoreType);
                //}

                SoundEngine.PlaySound(SoundID.Roar, NPC.Center);

                // Screenshake effect different from the one in BrigadePlayers class
                PunchCameraModifier modifier = new PunchCameraModifier(NPC.Center, (Main.rand.NextFloat() * (float)MathHelper.Pi * 2f).ToRotationVector2(), 20f, 6f, 20, 1000f, FullName);
                Main.instance.CameraModifiers.Add(modifier);
            }
        }

        public override void AI() {
            
        }

        // Various helper functions if needed for AI()

    }
}
