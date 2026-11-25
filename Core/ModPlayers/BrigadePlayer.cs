using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Brigade.Core.ModPlayers {
    internal class BrigadePlayer : ModPlayer {

        public int shakeTimer;

        public float RangeCritDamageIncrease;

        public override void ResetEffects() {
            RangeCritDamageIncrease = 0;
        }

        public override void ModifyScreenPosition() {
            if (shakeTimer > 0) {
                shakeTimer--;
                Vector2 shake = new Vector2(Main.rand.NextFloat(shakeTimer), Main.rand.NextFloat(shakeTimer));
                Main.screenPosition += shake;
            }
        }

        public void AddShake(int amount, bool clamped = true) {
            if (clamped) {
                if (shakeTimer < amount) {
                    shakeTimer = amount;
                }
            } else {
                shakeTimer += amount;
            }
        }

        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers) {
            if (proj.CountsAsClass(DamageClass.Ranged)) {
                modifiers.CritDamage += RangeCritDamageIncrease;
            }
        }
    }
}
