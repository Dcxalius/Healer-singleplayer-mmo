using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Unit.Stats
{
    internal class EquipmentStats : PrimaryStats
    {
        public Armor TotalArmor => armor;
        Armor armor;

        public EquipmentStats(int[] aStats, int aArmor) : base(aStats)
        {
            armor = new Armor(aArmor);
        }

        public void RemoveStats(EquipmentStats aStats)
        {
            for (int i = 0; i < stats.Length; i++)
            {
                stats[i].Decrease(aStats.Stats[i]);
            }
            armor.Decrease(aStats.armor);
        }

        public void AddStats(EquipmentStats aStats)
        {
            for (int i = 0; i < stats.Length; i++)
            {
                stats[i].Increase(aStats.Stats[i]);
            }
            armor.Increase(aStats.armor);
        }
    }
}
