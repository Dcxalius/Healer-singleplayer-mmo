using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Unit.Stats
{
    internal class TotalPrimaryStats : PrimaryStats
    {
        static void AssertSimThread() => ThreadAffinity.AssertSimThread();
        BasePrimaryStats basePrimaryStats;
        EquipmentStats equipmentStats;

        public TotalPrimaryStats(BasePrimaryStats aBaseStats, EquipmentStats aEquipmentStats) : base(
            new int[] {
                aBaseStats.Strength + aEquipmentStats.Strength,
                aBaseStats.Agility + aEquipmentStats.Agility,
                aBaseStats.Intellect + aEquipmentStats.Intellect,
                aBaseStats.Spirit + aEquipmentStats.Spirit,
                aBaseStats.Stamina + aEquipmentStats.Stamina
            })
        { 
            basePrimaryStats = aBaseStats;
            equipmentStats = aEquipmentStats;
        }

        public void UpdateBaseStats(BasePrimaryStats aBaseStats)
        {
            AssertSimThread();
            SetStats(aBaseStats, equipmentStats);
        }

        public void UpdateEquipmentStats(EquipmentStats aEquipmentStats)
        {
            AssertSimThread();
            SetStats(basePrimaryStats, aEquipmentStats);
        }

        void SetStats(BasePrimaryStats aBaseStats, EquipmentStats aEquipmentStats)
        {
            AssertSimThread();
            int[] stats = new int[] {
                aBaseStats.Strength + aEquipmentStats.Strength,
                aBaseStats.Agility + aEquipmentStats.Agility,
                aBaseStats.Intellect + aEquipmentStats.Intellect,
                aBaseStats.Spirit + aEquipmentStats.Spirit,
                aBaseStats.Stamina + aEquipmentStats.Stamina
            };
            SetStats(stats);
        }
    }
}
