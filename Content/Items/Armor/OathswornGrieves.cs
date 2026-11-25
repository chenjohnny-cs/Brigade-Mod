using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
    
namespace Brigade.Content.Items.Armor {

    [AutoloadEquip(EquipType.Legs)]
    internal class OathswornGrieves : ModItem {

        public override void SetDefaults() {
            Item.width = 18;
            Item.height = 18;

            Item.rare = ItemRarityID.Green;
            Item.value = Item.sellPrice(gold: 5);

            Item.defense = 7;
        }
    }
}
