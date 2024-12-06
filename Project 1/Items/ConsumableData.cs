using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Items
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
            Drink
        }

        public ConsumableType Consumable { get => type; }
        ConsumableType type;
        public float Value { get => value; }
        float value;


        [JsonConstructor]
        public ConsumableData(int id, string gfxName, string name, string description, int maxStack, ConsumableType type, float value = -1) : base(id, gfxName, name, description, maxStack, ItemType.Consumable)
        {

            this.type = type;
            this.value = value;
            Assert();
        }

        void Assert()
        {
            Debug.Assert(type != ConsumableType.NONE, "Type not set.");
            Debug.Assert(value != -1, "Value not set.");

        }
    }
}
