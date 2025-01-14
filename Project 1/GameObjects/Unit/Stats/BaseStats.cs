using Project_1.GameObjects.Unit.Resources;
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
        public Health Health => health;
        Health health;

        public Resource Resource => resource;
        Resource resource;

        public Attack Attack => attack;
        Attack attack;

        public BasePrimaryStats PrimaryStats => primaryStats;
        BasePrimaryStats primaryStats;

        public int AttackPower
        {
            get { return attackPower; }
            set
            {
                attackPower = value;
                attack.AttackPower = value;
            }
        }
        int attackPower;

        public BaseStats(ClassData aClassData, int aLevel, float aCurrentHealth = float.MaxValue, float aCurrentResource = float.MaxValue)
        {
            primaryStats = new BasePrimaryStats(aClassData.BaseStats, aClassData.PerLevelStats, aLevel);
            health = new Health(aClassData, primaryStats, aLevel, aCurrentHealth);
            switch (aClassData.Resource)
            {
                case Resource.ResourceType.Mana:
                    //int aBaseManaFromClass;
                    float manaPer5 = 5; //TODO: Extranct these values from class
                    int maxResource = /*baseFromClass + */ 1;

                    resource = new Mana(maxResource, primaryStats, aCurrentResource, manaPer5, aLevel);
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

            attack = new Attack(aClassData.FistMinAttackDamage, aClassData.FistMaxAttackDamage, aClassData.FistAttackSpeed);

        }



        public void LevelUp(ClassData aClassData)
        {
            primaryStats.LevelUp(aClassData.PerLevelStats);
            health.LevelUp(aClassData.PerLevelHp, primaryStats.Stamina);
            resource.LevelUp();

            RefreshStats();
        }

        public void RefreshStats()
        {
            health.Refresh(primaryStats);
            resource.Refresh(primaryStats);
        }
    }
}
