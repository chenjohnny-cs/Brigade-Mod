using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Brigade.Content.Items.Weapons.Melee {
    internal class LividBroadsword : ModItem {

        public override void SetDefaults() {
            Item.Size = new Vector2(32, 32);

            Item.DamageType = DamageClass.Melee;
            Item.damage = 10;
            Item.knockBack = 2.4f;

            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item1;

            Item.useAnimation = 15;
            Item.useTime = 15;

            Item.scale = 1.25f;
        }

        public override void AddRecipes() {
            CreateRecipe()
                .AddIngredient(ItemID.WoodenSword, 1)
                .AddIngredient(ItemID.Mushroom, 5)
                .AddIngredient(ItemID.Daybloom, 1)
                .AddTile(TileID.WorkBenches)
                .Register();
        }

        // Two different ways of creating dust particle effects, NewDust and NewDustPerfect. NewDust returns an integer that is the id of new dust particle
        // that was created in the global dust array. You can then index it with Main.dust[int]. NewDustPerfect skips that and just returns the dust effect.
        public override void MeleeEffects(Player player, Rectangle hitbox) {
            if (Main.rand.NextBool(3)) {
                int d = Dust.NewDust(hitbox.TopLeft(), hitbox.Width, hitbox.Height, DustID.Grass);
                Dust dust = Main.dust[d];

                dust.noGravity = true;
            }
        }

        // GetSource_OnHit is pretty much saying, why is this item being created? We want to create it because this player hit this target.
        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone) {
            if (hit.Crit) {
                for (int i=0; i<5; i++) {
                    Dust onHitDust = Dust.NewDustPerfect(target.Center, DustID.MushroomTorch, Main.rand.NextVector2Circular(5f, 5f));
                    onHitDust.noGravity = false;
                }

                SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact, target.Center);
                Item.NewItem(player.GetSource_OnHit(target), target.getRect(), ModContent.ItemType<HealingMushroom>());
            }
        }
    }

    public class HealingMushroom : ModItem {
        public override string Texture => $"Terraria/Images/Item_{ItemID.Mushroom}";
        public override void SetDefaults() {
            Item.Size = new Vector2(12, 12);

            Item.scale = 0.6f;
        }

        public override bool OnPickup(Player player) {
            player.Heal(5);

            return false;
        }
    }
}
