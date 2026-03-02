using Newtonsoft.Json;
using Project_1.GameObjects.Unit.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Items.SubTypes
{
    public class EquipmentSetBonus
    {
        [JsonConstructor]
        public EquipmentSetBonus()
        {
        }
    }

    internal class EquipmentSet
    {
        int[] itemIdsInSet;
        //(int piecesRequired, EquipmentSetBonus bonus)[] setBonuses;

        [JsonConstructor]
        EquipmentSet(int[] itemIdsInSet)
        {
            this.itemIdsInSet = itemIdsInSet;
            //This is all autocompleted debug assertions, make sure to verify their correctness.
            //Debug.Assert(itemIdsInSet.Length > 0, "Equipment set must contain at least one item.");
            //Debug.Assert(itemIdsInSet.Distinct().Count() == itemIdsInSet.Length, "Equipment set contains duplicate item IDs.");
            //Debug.Assert(itemIdsInSet.All(id => ItemFactory.GetItemDataById(id) != null), "Equipment set contains invalid item IDs.");
            for (int i = 0; i < itemIdsInSet.Length; i++)
            {
                //Debug.Assert(ItemFactory.GetItemDataById(itemIdsInSet[i]).SetId == this.Id, $"Item ID {itemIdsInSet[i]} does not belong to set ID {this.Id}.");
            }
        }


    }
}
