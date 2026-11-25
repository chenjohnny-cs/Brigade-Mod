using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Brigade.Content.Items.Weapons.Ranged {
    internal class RoyalBow : ModItem {

        public override string Texture => $"Terraria/Images/Item_{ItemID.CopperBow}";

        public override void SetDefaults() {
            Item.Size = new Vector2(26, 26);
            Item.DamageType = DamageClass.Ranged;
            Item.damage = 6;
            Item.knockBack = 2f;

            Item.useStyle = ItemUseStyleID.Shoot;
            Item.UseSound = SoundID.Item5;

            Item.useTime = 5;
            Item.useAnimation = 20;
            Item.useLimitPerAnimation = 4;
            Item.reuseDelay = 25;

            Item.shoot = ProjectileID.WoodenArrowFriendly;
            Item.shootSpeed = 8f;
            Item.useAmmo = AmmoID.Arrow;

            Item.consumeAmmoOnLastShotOnly = true;

            Item.noMelee = true;
            Item.autoReuse = true;
        }

    }
}
