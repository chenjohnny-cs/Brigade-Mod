using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Brigade.Content.Items.Weapons.Melee {
    internal class GarmBlade : ModItem {
        public override void SetDefaults() {
            Item.Size = new Vector2(32, 32);

            Item.DamageType = DamageClass.Melee;
            Item.damage = 11;
            Item.crit = 3;
            Item.knockBack = 2.4f;

            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item1;

            Item.useAnimation = 35;
            Item.useTime = 35;

            Item.scale = 1f;
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone) {
            SoundEngine.PlaySound(SoundID.Item43 with { PitchVariance = 0.4f, Volume = 0.75f, Pitch = -0.3f }, player.Center);

            int maxProjs = 10;

            int activeProjs = player.ownedProjectileCounts[ModContent.ProjectileType<GarmBladeProjectile>()];

            // Disallow more than 10 projectiles spawned at a time since player can hit multiple enemies, which would spawn dozens of orbs.
            if (activeProjs >= maxProjs) {
                return;
            }

            for (int i = 0; i < 9; i++) {
                Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(45f, 45f), DustID.Smoke,
                     Main.rand.NextVector2CircularEdge(2f, 2f), Main.rand.Next(50, 100), default, 2f).noGravity = true;

                Dust dust = Dust.NewDustPerfect(target.Center, DustID.Torch);
                dust.velocity = new Vector2(1f, 4f);
            }

            int projectileCount = Main.rand.Next(3, 6);
            float projectileSpeed = 8f;
            float knockback = 0f;
            int dmg = 4;

            for (int i = 0; i < projectileCount; i++) {
                float angle = MathHelper.TwoPi * i / projectileCount;
                Vector2 dir = angle.ToRotationVector2();

                Projectile.NewProjectile(player.GetSource_OnHit(target), target.Center, dir * projectileSpeed, ModContent.ProjectileType<GarmBladeProjectile>(), dmg, knockback, player.whoAmI, target.whoAmI);
            }
        }
    }

    public class GarmBladeProjectile : ModProjectile {

        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.RubyBolt}";

        private ref float TargetID => ref Projectile.ai[0];
        private ref float Timer => ref Projectile.ai[1];

        private const int ExpandDuration = 20;
        private const float CollapseSpeed = 10f;


        public override void SetDefaults() {
            Projectile.DamageType = DamageClass.Melee;
            Projectile.Size = new Vector2(10, 10);

            Projectile.friendly = true;
            Projectile.hostile = false;

            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;

            Projectile.tileCollide = false;
        }

        public override bool? CanDamage() {
            return Timer >= ExpandDuration;
        }

        public override void AI() {
            Timer++;

            NPC target = Main.npc[(int)TargetID];

            if (!target.active) {
                Projectile.Kill();
                return;
            }

            // Give projectiles some time to chase and then destroy at 50 secs.
            if (Timer >= ExpandDuration * 2.5) {
                Projectile.Kill();
                return;
            }

            // Projectile just spawned, velocity is constantly decreased while it exapnds outward.
            if (Timer < ExpandDuration) {
                Projectile.velocity *= 0.96f;
            }

            // Collapse back towards the enemy after expanding.
            else {
                Vector2 dir = target.Center - Projectile.Center;

                // If the projectile is too close, we destroy it.

                dir.Normalize();
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, dir * CollapseSpeed, 0.15f);

                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.CorruptionThorns);

                dust.noGravity = true;
                dust.velocity = Vector2.Zero;
            }
        }
    }
}
