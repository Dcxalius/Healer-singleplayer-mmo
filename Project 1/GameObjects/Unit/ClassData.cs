using Project_1.GameObjects.Unit.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Unit
{
    internal struct ClassData
    {
        public enum Type
        {
            None,
            Player,
            Ally,
            Mob
        }

        public string Name => name;
        readonly string name;

        public Resource.ResourceType Resource => resource;
        readonly Resource.ResourceType resource;

        public BasePrimaryStats BaseStats => baseStats;
        readonly BasePrimaryStats baseStats;

        public BasePrimaryStats PerLevelStats => perLevelStats;
        readonly BasePrimaryStats perLevelStats;

        public int BaseHealth => baseHp;
        readonly int baseHp;

        public int PerLevelHp => perLevelHp;
        readonly int perLevelHp;

        public float HpPer5 => hpPer5;
        readonly float hpPer5;

        public float FistAttackSpeed => fistAttackSpeed;
        readonly float fistAttackSpeed;

        public float FistAttackDamage => fistAttackDamage;
        readonly float fistAttackDamage;

        public ClassData(string name, Resource.ResourceType resource, int[] baseStats, int[] perLevelStats, int baseHp, int perLevelHp, float hpPer5, float fistAttackSpeed, float fistAttackDamage)
        {
            this.name = name;
            this.resource = resource;
            this.baseStats = new BasePrimaryStats(baseStats);
            this.perLevelStats = new BasePrimaryStats(perLevelStats);
            this.baseHp = baseHp;
            this.perLevelHp = perLevelHp;
            this.hpPer5 = hpPer5;
            this.fistAttackDamage = fistAttackDamage;
            this.fistAttackSpeed = fistAttackSpeed;
        }

        public ClassData(Relation.RelationToPlayer aRelation, string aClassName)
        {
            switch (aRelation)
            {
                case Relation.RelationToPlayer.Self:
                    this = ObjectFactory.GetPlayerClass(aClassName);
                    break;
                case Relation.RelationToPlayer.Friendly:
                    this = ObjectFactory.GetAllyClass(aClassName);
                    break;
                case Relation.RelationToPlayer.Neutral:
                case Relation.RelationToPlayer.Hostile:
                    this = ObjectFactory.GetMobClass(aClassName);
                    break;
                default:
                    throw new Exception("Incorrect relation.");
            }
        }

        void Assert()
        {
            Debug.Assert(name != null && resource != Resources.Resource.ResourceType.None && baseHp > 0 && perLevelHp > 0);
        }
    }
}
