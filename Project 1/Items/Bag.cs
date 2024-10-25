using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Items
{
    internal class Bag : Item
    {
        public int SlotCount { get => slotCount; }
        int slotCount;

        public Bag(ItemData aData) : base(aData, 1)
        {
        }
    }
}
