using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Brigade.Core.ModPlayers;

namespace Brigade.Content.Items.Armor {

    [AutoloadEquip(EquipType.Head)]
    internal class OathswornMask : ModItem {
        public static readonly int CriticalStrikeChanceIncrease = 8;
        public static readonly int CriticalStrikeDamageIncrease = 10;
        public static readonly int RangedDamageIncrease = 6;

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(RangedDamageIncrease);

        public static LocalizedText SetBonusText { get; private set; }

        public override void SetStaticDefaults() {
            SetBonusText = this.GetLocalization("SetBonus").WithFormatArgs(CriticalStrikeChanceIncrease, CriticalStrikeDamageIncrease);
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
            player.setBonus = SetBonusText.Value;

            player.GetCritChance(DamageClass.Ranged) += CriticalStrikeChanceIncrease;
            player.GetModPlayer<BrigadePlayer>().RangeCritDamageIncrease += CriticalStrikeDamageIncrease / 100f;
        }

        public override void UpdateEquip(Player player) {
            player.GetDamage(DamageClass.Ranged) += RangedDamageIncrease / 100f;
        }

    }
}
