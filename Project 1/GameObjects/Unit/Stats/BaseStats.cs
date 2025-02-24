using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Unit.Resources;
using Project_1.UI.HUD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Project_1.GameObjects.Unit.Stats.BasePrimaryStats;

namespace Project_1.GameObjects.Unit.Stats
{
    internal class BaseStats
    {
        Entity owner;
        public Health Health => health;
        Health health;

        public Resource Resource => resource;
        Resource resource;

        public AttackData FistAttack => fistAttack;
        AttackData fistAttack;

        public Armor TotalArmor => baseArmor + totalPrimaryStats.Agility.Armor;
        Armor baseArmor;

        ClassData classData;

        public TotalPrimaryStats TotalPrimaryStats => totalPrimaryStats;
        TotalPrimaryStats totalPrimaryStats;
        BasePrimaryStats basePrimaryStats;

        public PairReport StatReport
        {
            get
            {
                PairReport report = TotalPrimaryStats.NewReport;
                report.AddLine("Armor", baseArmor);
                return report;
            }
        }

        public int GetAttackPower(ClassData aClassData) => totalPrimaryStats.Agility.GetMeleeAttackPower(aClassData) + totalPrimaryStats.Strength.GetMeleeAttackPower(aClassData);


        public BaseStats(ClassData aClassData, int aLevel, EquipmentStats aEquipmentStats, float aCurrentHealth = float.MaxValue, float aCurrentResource = float.MaxValue)
        {
            basePrimaryStats = new BasePrimaryStats(aClassData.BaseStats, aClassData.PerLevelStats, aLevel);
            totalPrimaryStats = new TotalPrimaryStats(basePrimaryStats, aEquipmentStats);
            health = new Health(aClassData, basePrimaryStats, aLevel, aCurrentHealth);
            classData = aClassData;
            switch (aClassData.Resource)
            {
                case Resource.ResourceType.Mana:
                    //int aBaseManaFromClass;
                    float manaPer5 = 5; //TODO: Extranct these values from class
                    int maxResource = /*baseFromClass + */ 1;

                    resource = new Mana(maxResource, TotalPrimaryStats, aCurrentResource, manaPer5, aLevel);
                    break;
                case Resource.ResourceType.Energy:
                    resource = new Energy(aCurrentResource);
                    break;
                case Resource.ResourceType.Rage:
                    throw new NotImplementedException();
                case Resource.ResourceType.None:
                    resource = new None();
                    break;
                default:
                    throw new NotImplementedException();
            }

            fistAttack = new AttackData(AttackData.AttackStyle.OneHander, new Attack(aClassData.FistMinAttackDamage, aClassData.FistMaxAttackDamage, aClassData.FistAttackSpeed), null);
            fistAttack.AttackPower = GetAttackPower(aClassData);
            baseArmor = new Armor(0);
        }

        public void SetOwner(Entity aEntity) => owner = aEntity;

        public bool CheckIfResourceRegened()
        {
            if (resource.GetType() != typeof(Mana)) return false;
            return (resource as Mana).CheckIfTicked();
        }


        public void LevelUp()
        {
            basePrimaryStats.LevelUp(classData.PerLevelStats);
            totalPrimaryStats.UpdateBaseStats(basePrimaryStats);
            health.LevelUp(classData.PerLevelHp, basePrimaryStats.Stamina);
            resource.LevelUp();

            RefreshStats();
        }

        public void RefreshStats()
        {
            health.Refresh(TotalPrimaryStats);
            resource.Refresh(TotalPrimaryStats);
            owner.Equipment.MeleeAttackPower = GetAttackPower(classData);
            fistAttack.AttackPower = GetAttackPower(classData);


            if (!(owner is Friendly)) return;
            HUDManager.RefreshCharacterWindowStats(StatReport, owner as Friendly);
        }

        public void RefreshEquipmentStats(EquipmentStats aEquipmentStats)
        {
            totalPrimaryStats.UpdateEquipmentStats(aEquipmentStats);
            baseArmor = aEquipmentStats.TotalArmor;
            RefreshStats();
        }
    }
}
