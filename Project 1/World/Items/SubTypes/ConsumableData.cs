using Newtonsoft.Json;
using System.Diagnostics;

namespace Project_1.Items.SubTypes
{
    internal class ConsumableData : ItemData
    {
        public enum ConsumableType
        {
            NONE,
            Heal,
            Mana,
            Energy,
            Food,
            Drink,
            EnchantScroll
        }

        [JsonIgnore]
        public ConsumableType Consumable { get => type; }
        ConsumableType type;
        [JsonIgnore]
        public float Value { get => value; }
        float value;
        [JsonIgnore]
        public int EnchantmentId => enchantmentId;
        int enchantmentId;
        [JsonIgnore]
        public bool RequiresItemTarget => type == ConsumableType.EnchantScroll;


        [JsonConstructor]
        public ConsumableData(int id, string gfxName, string name, string description, int maxStack, ConsumableType type, Item.Quality quality, int cost, float value = -1, int enchantmentId = -1, int itemLevel = 1) : base(id, gfxName, name, description, maxStack, ItemType.Consumable, quality, cost, itemLevel)
        {

            this.type = type;
            this.value = value;
            this.enchantmentId = enchantmentId;
            Assert();
        }

        void Assert()
        {
            Debug.Assert(type != ConsumableType.NONE, "Type not set.");
            if (type == ConsumableType.EnchantScroll)
            {
                Debug.Assert(enchantmentId >= 0, "EnchantmentId not set.");
                return;
            }

            Debug.Assert(value != -1, "Value not set.");
        }
    }
}
