using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Brigade.Content.Items.Armor {

    [AutoloadEquip(EquipType.Body)]
    internal class OathswornBreastplate : ModItem {
        public override void SetDefaults() {
            Item.width = 18;
            Item.height = 18;

            Item.rare = ItemRarityID.Green;
            Item.value = Item.sellPrice(gold: 5);

            Item.defense = 7;
        }
    }
}
