using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Unit.Stats
{
    internal class EquipmentStats : PrimaryStats
    {
        public EquipmentStats(int[] aStats) : base(aStats)
        {

        }

        public void RemoveStats(EquipmentStats aStats)
        {
            for (int i = 0; i < stats.Length; i++)
            {
                stats[i].Decrease(aStats.Stats[i]);
            }
        }

        public void AddStats(EquipmentStats aStats)
        {
            for (int i = 0; i < stats.Length; i++)
            {
                stats[i].Increase(aStats.Stats[i]);
            }
        }
    }
}
