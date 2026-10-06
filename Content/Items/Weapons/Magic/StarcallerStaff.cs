using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Brigade.Content.Items.Weapons.Magic {
    internal class StarcallerStaff : ModItem {
        public override void SetDefaults() {
            Item.Size = new Vector2(42, 40);
            Item.DamageType = DamageClass.Magic;
            Item.damage = 5;
            Item.knockBack = 1f;
            Item.mana = 5;

            Item.UseSound = SoundID.Item1;
            Item.useStyle = ItemUseStyleID.Shoot;

            Item.useAnimation = 30;
            Item.useTime = 30;

            Item.shoot = ModContent.ProjectileType<StarcallerStaffHoldoutProjectile>();
            Item.shootSpeed = 2f;

            Item.rare = ItemRarityID.Yellow;

            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true; // Hold weapon
        }

        public override void AddRecipes() {
            CreateRecipe()
                .AddIngredient(ItemID.Wood, 15)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    public class StarcallerStaffHoldoutProjectile : ModProjectile {
        public bool Dying;
        //public int RecoilTimer;
        // This will increment every frame the weapon is held.
        public int Timer {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        // Equivalent to Item.UseTime
        public int UseTime {
            get => (int)Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        // Used for the shoot animation. Decreases by 1 when greater than zero in AI() function.
        public int AnimationTime {
            get => (int)Projectile.ai[2];
            set => Projectile.ai[2] = value;
        }

        // Check if the player is holding the correct item and other booleans.
        public bool CanHold => Owner.HeldItem.ModItem is StarcallerStaff && Owner.channel && !Owner.CCed && !Owner.noItems;
        public Vector2 ArmPosition => Owner.RotatedRelativePoint(Owner.MountedCenter, true) + new Vector2(20f, 0f).RotatedBy(Projectile.rotation);
        public Player Owner => Main.player[Projectile.owner];
        public override string Texture => "Brigade/Content/Items/Weapons/Magic/StarcallerStaff";

        public override bool? CanDamage() {
            return false;
        }

        public override void SetDefaults() {
            Projectile.width = 42;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI() {
            if (!CanHold && !Dying) {
                Dying = true;
                Projectile.timeLeft = 10;
            }

            if (AnimationTime > 0) {
                AnimationTime--;
            }

            if (Timer == 0f) {
                if (Main.myPlayer == Projectile.owner) {
                    Projectile.velocity = Owner.DirectionTo(Main.MouseWorld);
                }

                Projectile.rotation = Projectile.velocity.ToRotation();
                Projectile.netUpdate = true;
                UseTime = CombinedHooks.TotalUseTime(Owner.itemTime, Owner, Owner.HeldItem);
            }

            if (!Dying) {
                if (Timer % UseTime == 0) {
                    AnimationTime = 30;
                }

                UpdateHeldProjectile();
                Timer++;

                const int ticks = 5;
                if (AnimationTime > 0 && AnimationTime % ticks == 0) {
                    SpawnProjectile();
                    Projectile.velocity = Projectile.velocity.RotatedByRandom(0.5f);
                }
            } else {
                UpdateHeldProjectile(false, false);
            }
        }

        public void SpawnProjectile() {

            if (Owner.statMana < 5) {
                Dying = true;
                Projectile.timeLeft = 10;
                return;
            } else {
                Owner.statMana -= 5;
            }

            Vector2 vel = new Vector2(0.1f, Projectile.velocity.Y);

            SoundEngine.PlaySound(SoundID.Item43 with { PitchVariance = 0.4f, Volume = 0.75f, Pitch = -0.3f }, Projectile.Center);

            if (Timer >= 10) {
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center + new Vector2(0, -500), vel,
                 ProjectileID.AmberBolt, (int) (Projectile.damage * 1.25f), Projectile.knockBack, Projectile.owner);
            } else {
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center + new Vector2(0, -500), vel,
                    ProjectileID.RubyBolt, Projectile.damage, Projectile.knockBack, Projectile.owner);
            }
        }

        public void UpdateHeldProjectile(bool updateTimeLeft = true, bool updateVelocity = true) {
            Owner.ChangeDir(Projectile.direction);
            Owner.heldProj = Projectile.whoAmI;

            Owner.itemTime = 2;
            Owner.itemAnimation = 2;

            if (updateTimeLeft) {
                Projectile.timeLeft = 2;
            }

            Projectile.rotation = Projectile.velocity.ToRotation();
            Owner.itemRotation = Utils.ToRotation(Projectile.velocity * Projectile.direction);

            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(90f));
            Projectile.position = ArmPosition - Projectile.Size * 0.5f;

            if (Main.myPlayer == Projectile.owner && updateVelocity) {
                Vector2 oldVel = Projectile.velocity;

                // the Lerp allows the weapont to have a sort of 'following' effect on the cursor which is nice.
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, Owner.DirectionTo(Main.MouseWorld), 0.05f);
                //Projectile.velocity = Owner.DirectionTo(Main.MouseWorld);

                if (Projectile.velocity != oldVel) {
                    Projectile.netSpam = 0;
                    Projectile.netUpdate = true;
                }
            }

            Projectile.spriteDirection = Projectile.direction;
        }

        public override bool PreDraw(ref Color lightColor) {
            var tex = ModContent.Request<Texture2D>(Texture).Value;

            SpriteEffects spriteEffects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Vector2 pos = (ArmPosition - Main.screenPosition);

            float rotation = Projectile.rotation + (spriteEffects == SpriteEffects.FlipHorizontally ? MathHelper.Pi : 0f) + MathHelper.PiOver4 * Projectile.spriteDirection;

            float fadeIn = 1f;
            if (Timer < 10f) {
                fadeIn = Timer / 10f;
            } else if (Dying) {
                fadeIn = Projectile.timeLeft / 10f;
            }

            Main.spriteBatch.Draw(tex, pos, null, lightColor * fadeIn, rotation, tex.Size() / 2f, Projectile.scale, spriteEffects, 0f);

            return false;
        }
    }
}
