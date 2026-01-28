using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Shepherd.Content.Items
{
    public class HelloWorldItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = 99;
            Item.value = Item.buyPrice(0, 0, 1, 0);
            Item.rare = ItemRarityID.Blue;
        }
    }
}