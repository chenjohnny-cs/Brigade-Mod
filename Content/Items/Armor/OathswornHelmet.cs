using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Brigade.Content.Items.Armor {

    [AutoloadEquip(EquipType.Head)]
    internal class OathswornHelmet : ModItem {
        public static readonly int MeleeDamageIncrease = 8;
        public static readonly int UseSpeedIncrease = 10;

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MeleeDamageIncrease);

        public static LocalizedText SetBonusText { get; private set; }

        public override void SetStaticDefaults() {
            SetBonusText = this.GetLocalization("SetBonus").WithFormatArgs("{0}", UseSpeedIncrease);
        }

        public override void SetDefaults() {
            Item.width = 18;
            Item.height = 18;

            Item.rare = ItemRarityID.Green;
            Item.value = Item.sellPrice(gold: 5);

            Item.defense = 6;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs) {
            return body.type == ModContent.ItemType<OathswornBreastplate>() && legs.type == ModContent.ItemType<OathswornGrieves>();
        }

        public override void UpdateArmorSet(Player player) {
            player.setBonus = SetBonusText.Value;
            player.GetAttackSpeed(DamageClass.Melee) += UseSpeedIncrease / 100f;
        }

        public override void UpdateEquip(Player player) {
            player.GetDamage(DamageClass.Melee) += MeleeDamageIncrease / 100f;
        }

    }
}
