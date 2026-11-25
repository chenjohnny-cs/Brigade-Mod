using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Brigade.Content.Items.Armor {

    [AutoloadEquip(EquipType.Head)]
    internal class OathswornCowl : ModItem {
        public static readonly int MagicDamageIncrease = 8;
        public static readonly int ManaIncrease = 60;

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MagicDamageIncrease);

        public static LocalizedText SetBonusText { get; private set; }

        public override void SetStaticDefaults() {
            SetBonusText = this.GetLocalization("SetBonus").WithFormatArgs("{0}", ManaIncrease);
        }

        public override void SetDefaults() {
            Item.width = 18;
            Item.height = 18;

            Item.rare = ItemRarityID.Green;
            Item.value = Item.sellPrice(gold: 5);

            Item.defense = 4;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs) {
            return body.type == ModContent.ItemType<OathswornBreastplate>() && legs.type == ModContent.ItemType<OathswornGrieves>();
        }

        public override void UpdateArmorSet(Player player) {
            player.statManaMax2 += ManaIncrease;
        }

        public override void UpdateEquip(Player player) {
            player.GetDamage(DamageClass.Magic) += MagicDamageIncrease / 100f;
        }

    }
}
