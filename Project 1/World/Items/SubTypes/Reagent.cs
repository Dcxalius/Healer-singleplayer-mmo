using Project_1.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.World.Items.SubTypes
{
    internal class Reagent : Item
    {
        public Reagent(LootData aLoot) : base(aLoot)
        {
        }

        public Reagent(int id, int count) : this(ItemFactory.GetItemData<ReagentData>(id), count)
        {
        }

        public Reagent(ItemData aData, int aCount) : base(aData, aCount)
        {
        }
    }
}
