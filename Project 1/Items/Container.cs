using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Items
{
    internal class Container : Item
    {
        public int SlotCount { get => (itemData as ContainerData).SlotCount; }

        public Container(ContainerData aData) : base(aData, 1)
        {
        }

        public Container(LootData aLoot) : base(aLoot)
        {
        
        }
    }
}
