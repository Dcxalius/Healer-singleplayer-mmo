using Project_1.Managers;
using Project_1.GameObjects.Unit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.World.GameObjects.Unit.Stats.Primary
{
    internal class TotalPrimaryStats : PrimaryStats
    {
        static void AssertSimThread() => ThreadAffinity.AssertSimThread();
        BasePrimaryStats basePrimaryStats;
        EquipmentStats equipmentStats;
        UnitData unitData;

        public TotalPrimaryStats(BasePrimaryStats aBaseStats, EquipmentStats aEquipmentStats, UnitData aUnitData) : base(
            BuildStats(aBaseStats, aEquipmentStats, aUnitData))
        { 
            basePrimaryStats = aBaseStats;
            equipmentStats = aEquipmentStats;
            unitData = aUnitData;
        }

        public void UpdateBaseStats(BasePrimaryStats aBaseStats)
        {
            AssertSimThread();
            SetStats(aBaseStats, equipmentStats);
        }

        public void UpdateEquipmentStats(EquipmentStats aEquipmentStats)
        {
            AssertSimThread();
            equipmentStats = aEquipmentStats;
            SetStats(basePrimaryStats, equipmentStats);
        }

        void SetStats(BasePrimaryStats aBaseStats, EquipmentStats aEquipmentStats)
        {
            AssertSimThread();
            basePrimaryStats = aBaseStats;
            equipmentStats = aEquipmentStats;
            SetStats(BuildStats(aBaseStats, aEquipmentStats, unitData));
        }

        static int[] BuildStats(BasePrimaryStats aBaseStats, EquipmentStats aEquipmentStats, UnitData aUnitData)
        {
            int[] stats = new int[(int)PrimaryStat.Count];
            stats[(int)PrimaryStat.Strength] = ApplyTalentBonus(aBaseStats.Strength + aEquipmentStats.Strength, PrimaryStat.Strength, aUnitData);
            stats[(int)PrimaryStat.Agility] = ApplyTalentBonus(aBaseStats.Agility + aEquipmentStats.Agility, PrimaryStat.Agility, aUnitData);
            stats[(int)PrimaryStat.Intellect] = ApplyTalentBonus(aBaseStats.Intellect + aEquipmentStats.Intellect, PrimaryStat.Intellect, aUnitData);
            stats[(int)PrimaryStat.Spirit] = ApplyTalentBonus(aBaseStats.Spirit + aEquipmentStats.Spirit, PrimaryStat.Spirit, aUnitData);
            stats[(int)PrimaryStat.Stamina] = ApplyTalentBonus(aBaseStats.Stamina + aEquipmentStats.Stamina, PrimaryStat.Stamina, aUnitData);
            return stats;
        }

        static int ApplyTalentBonus(int aValue, PrimaryStat aStat, UnitData aUnitData)
        {
            if (aUnitData == null)
            {
                return aValue;
            }

            double flat = aUnitData.GetTalentPrimaryStatFlat(aStat);
            double percent = aUnitData.GetTalentPrimaryStatPercent(aStat);
            return (int)Math.Round((aValue + flat) * (1d + percent), MidpointRounding.AwayFromZero);
        }
    }
}
