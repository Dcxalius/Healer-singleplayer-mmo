using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Unit.Stats
{
    internal class TotalPrimaryStats : PrimaryStats
    {
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
            SetStats(aBaseStats, equipmentStats);
        }

        public void UpdateEquipmentStats(EquipmentStats aEquipmentStats)
        {
            SetStats(basePrimaryStats, aEquipmentStats);
        }

        void SetStats(BasePrimaryStats aBaseStats, EquipmentStats aEquipmentStats)
        {
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
