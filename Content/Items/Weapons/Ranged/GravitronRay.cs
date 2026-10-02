using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using ReLogic.Content;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using Terraria;
using Terraria.DataStructures;
using System;
using Terraria.Audio;

namespace Brigade.Content.Items.Weapons.Ranged {

    // "Held Projectile Weapons" are weapons that serve as projectiles as well, enabling you to have more control over custom animations than normal items.
    // This weapon will have fire rate ramp up / acceleration and a random chance to fire a second projectile while held.
    // This weapon also manually picks ammo and consumes ammo through Player.PickAmmo

    internal class GravitronRay : ModItem {

        public const int HoldoutDistance = 20;

        private static Asset<Texture2D> glowTex;

        public override void Load() {
            glowTex = ModContent.Request<Texture2D>(Texture + "_Glow");
        }

        public override void SetDefaults() {
            Item.Size = new Vector2(56, 26);
            Item.DamageType = DamageClass.Ranged;
            Item.damage = 60;
            Item.knockBack = 2f;

            // By convention, the shoot speed of held projectile weapons corresponds to how far out the projectile is held.
            // This would affect the velocity of the ammo projectiles this weapon spawns as well. So we wont be going by convention.
            Item.shootSpeed = 6f;

            Item.useStyle = ItemUseStyleID.Shoot;

            Item.useAnimation = 20;
            Item.useTime = 20;

            Item.shoot = ModContent.ProjectileType<GravitronRayProjectile>();
            Item.useAmmo = AmmoID.Arrow;
            Item.rare = ItemRarityID.Yellow;
            

            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true; // allows us to hold the weapon
        }

        public override void AddRecipes() {
            CreateRecipe()
                .AddIngredient(ItemID.Wood, 15)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
            type = ModContent.ProjectileType<GravitronRayProjectile>();

            // as said before, since by convention the velocity wil lbe affected by out shoot speed, we need to change it so that it is correct using HoldoutDistance.
            // The velocity for the projectiles of this weapon are actually the holdout offset.
            velocity = Vector2.Normalize(velocity) * HoldoutDistance;

            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, Main.myPlayer);

            return false;
        }

        public override bool CanConsumeAmmo(Item ammo, Player player) {
            // Prevents the player from consuming ammo when initially used.
            // The projectile will instead "spin up" and then consume ammo.

            if (player.ItemTimeIsZero) {
                return false;
            }

            return true;
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) {
            // This will allow us to draw the glow texture into the world.

            Texture2D tex = glowTex.Value;
            spriteBatch.Draw(tex, new Vector2(Item.position.X - Main.screenPosition.X + Item.width * 0.5f, Item.position.Y - Main.screenPosition.Y + Item.height - tex.Height * 0.5f),
                new Rectangle(0, 0, tex.Width, tex.Height), Color.White, rotation, tex.Size() * 0.5f, scale, SpriteEffects.None, 0f);
        }

    }

    public class GravitronRayProjectile : ModProjectile {

        public ref float HoldTimer => ref Projectile.ai[0];
        public ref float ShootTimer => ref Projectile.ai[1];

        public int shootCount = 0;

        public override void SetStaticDefaults() {
            Main.projFrames[Type] = 6;

            // Prevents jittering when stepping up and down blocks and half blocks.
            ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
        }

        public override void SetDefaults() {
            Projectile.Size = new Vector2(22, 22);

            Projectile.DamageType = DamageClass.Ranged;

            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.hide = true;
            Projectile.ignoreWater = true;

            DrawOffsetX = -17;
            DrawOriginOffsetY = -4;
        }

        // Held Projectiles should set this to false, if not it will deal contact damage.
        public override bool? CanDamage() {
            return false;
        }

        public override void AI() {
            Player player = Main.player[Projectile.owner];
            Vector2 playerCenter = player.RotatedRelativePoint(player.MountedCenter);

            // Hold Timer will count how long the weapon has been held.
            // This will help us control how fast the weapon can shoot and how fast the animation can play.
            HoldTimer += 1f;
            int animSpeed = Math.Min((int)HoldTimer / 40, 3);
            int initialShootDelay = 24;
            int shootDelayAdjustmentRate = 6;

            ShootTimer += 1f;
            bool shouldShootArrow = false;
            if (ShootTimer >= initialShootDelay - shootDelayAdjustmentRate * animSpeed) {
                ShootTimer = 0f;
                shouldShootArrow = true;
            }

            // Code allows us to continue to cycle the animation when the weapon is held, which gets faster as anim speed increases.
            Projectile.frameCounter += 1 + animSpeed;
            if (Projectile.frameCounter >= 4) {
                Projectile.frameCounter = 0;
                Projectile.frame = ++Projectile.frame % Main.projFrames[Type];
            }

            // Sound and dust are seperate from the code of the actual firing of the projectile as they need to be played
            // on every client to ensure consistency.
            if (Projectile.soundDelay <= 0) {
                Projectile.soundDelay = initialShootDelay - shootDelayAdjustmentRate * animSpeed;

                // prevents a shoot sound from being played when the projectile initially spawns.
                if (HoldTimer != 1f) {
                    SoundEngine.PlaySound(SoundID.Item5, Projectile.position);
                }
            }

            // Again, we want to make sure this code is only running on the client
            // of the person who fired the projectile, since some client sided arrays hold
            // multiple clients worth of information, so we need to distinguish it.
            if (shouldShootArrow && Main.myPlayer == Projectile.owner) {
                Item heldItem = player.HeldItem;
                if (player.channel && player.HasAmmo(heldItem) && !player.noItems && !player.CCed) {
                    float holdoutDistance = GravitronRay.HoldoutDistance * Projectile.scale;
                    Vector2 holdoutOffset = holdoutDistance * Vector2.Normalize(Main.MouseWorld - playerCenter);

                    if (holdoutOffset.X != Projectile.velocity.X || holdoutOffset.Y != Projectile.velocity.Y) {
                        // replicates it to the server
                        Projectile.netUpdate = true;
                    }

                    Projectile.velocity = holdoutOffset;

                    int projCount = 1;
                    if (shootCount == 3) {
                        // Allow it to fire a stronger projectile every 4 shots.
                        projCount = 1;
                    }

                    for (int i = 0; i < projCount; i++) {
                        var spawnLoc = playerCenter + holdoutOffset + Main.rand.NextVector2Circular(6, 6);
                        bool ammoConsumed = player.PickAmmo(heldItem, out int projToShoot, out float speed, out int damage, out float knockBack, out int usedAmmoItemId);

                        if (ammoConsumed) {
                            if (shootCount == 3) {
                                projToShoot = ProjectileID.BeeArrow;
                            }

                            var source = player.GetSource_ItemUse_WithPotentialAmmo(heldItem, usedAmmoItemId);
                            Projectile.NewProjectile(source, spawnLoc, Vector2.Normalize(Projectile.velocity) * speed, projToShoot, damage, knockBack, Projectile.owner);
                        }
                    }

                    shootCount = (shootCount + 1) % 4;
                } else {
                    Projectile.Kill();
                }
            }

            Projectile.direction = Projectile.velocity.X < 0 ? -1 : 1;
            Projectile.spriteDirection = Projectile.direction;
            player.ChangeDir(Projectile.direction);
            player.heldProj = Projectile.whoAmI;
            player.SetDummyItemTime(2);
            Projectile.Center = playerCenter;

            float rotationOffset = Projectile.spriteDirection == -1 ? MathHelper.Pi : 0;
            Projectile.rotation = Projectile.velocity.ToRotation() + rotationOffset;
            player.itemRotation = (Projectile.velocity * Projectile.direction).ToRotation();
            Projectile.timeLeft = 2;
        }
    }
}
