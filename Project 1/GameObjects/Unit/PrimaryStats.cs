using Project_1.GameObjects.Unit.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Project_1.GameObjects.Unit.BasePrimaryStats;

namespace Project_1.GameObjects.Unit
{
    internal class PrimaryStats : BasePrimaryStats
    {
        public Health Health => health;
        Health health;

        public Resource Resource => resource;
        Resource resource;

        public Attack Attack => attack;
        Attack attack;

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

        public PrimaryStats(ClassData aClassData, int aLevel, float aCurrentHealth, float aCurrentResource) : base(aClassData.BaseStats, aClassData.PerLevelStats, aLevel)
        {
            health = new Health(aClassData, this, aLevel, aCurrentHealth);

            switch (aClassData.Resource)
            {
                case Resource.ResourceType.Mana:
                    //int aBaseManaFromClass;
                    float manaPer5 = 5; //TODO: Extranct these values from class
                    int maxResource = /*baseFromClass + */ 1;

                    resource = new Mana(maxResource, this, aCurrentResource, manaPer5, aLevel);
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

        }



        public void LevelUp(ClassData aClassData)
        {
            strength += aClassData.PerLevelStats.Strength;
            agility += aClassData.PerLevelStats.Agility;
            intellect += aClassData.PerLevelStats.Intellect;
            spirit += aClassData.PerLevelStats.Spirit;
            stamina += aClassData.PerLevelStats.Stamina;

            health.LevelUp(aClassData.PerLevelHp, Stamina);
            resource.LevelUp();

            RefreshStats();
        }

        public void RefreshStats()
        {
            health.Refresh(this);
            resource.Refresh(this);
        }
    }
}
