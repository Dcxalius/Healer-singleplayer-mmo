using Newtonsoft.Json;
using Project_1.Items;

namespace Project_1.World.Items.SubTypes
{
    internal class ReagentData : ItemData
    {
        [JsonConstructor]
        public ReagentData(int id, string gfxName, string name, string description, int maxStack, Item.Quality quality, int cost, int itemLevel = 1, string[] tags = null) : base(id, gfxName, name, description, maxStack, ItemType.Reagent, quality, cost, itemLevel, tags)
        {
        }
    }
}
