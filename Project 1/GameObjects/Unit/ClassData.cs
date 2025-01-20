using Newtonsoft.Json;
using Project_1.GameObjects.Unit.Resources;
using Project_1.GameObjects.Unit.Stats;
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

        public PrimaryStats BaseStats => baseStats;
        readonly PrimaryStats baseStats;

        public PrimaryStats PerLevelStats => perLevelStats;
        readonly PrimaryStats perLevelStats;

        public int BaseHealth => baseHp;
        readonly int baseHp;

        public int PerLevelHp => perLevelHp;
        readonly int perLevelHp;

        public float HpPer5 => baseHpPer5;
        readonly float baseHpPer5;

        public float FistAttackSpeed => fistAttackSpeed;
        readonly float fistAttackSpeed;

        public float FistMinAttackDamage => fistMinAttackDamage;
        readonly float fistMinAttackDamage;

        public float FistMaxAttackDamage => fistMaxAttackDamage;
        readonly float fistMaxAttackDamage;

        public Movement Movement => movementData;
        Movement movementData;


        [JsonConstructor]
        public ClassData(string name, Resource.ResourceType resource, int[] baseStats, int[] perLevelStats, int baseHp, int perLevelHp, float baseHpPer5, float fistAttackSpeed, float fistMinAttackDamage, float fistMaxAttackDamage, float speed, float maxSpeed)
        {
            this.name = name;
            this.resource = resource;
            this.baseStats = new PrimaryStats(baseStats);
            this.perLevelStats = new PrimaryStats(perLevelStats);
            this.baseHp = baseHp;
            this.perLevelHp = perLevelHp;
            this.baseHpPer5 = baseHpPer5;
            this.fistMinAttackDamage = fistMinAttackDamage;
            this.fistMaxAttackDamage = fistMaxAttackDamage;
            this.fistAttackSpeed = fistAttackSpeed;
            movementData = new Movement(speed, maxSpeed);

            Assert();
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
            Debug.Assert(name != null && baseHp > 0 && perLevelHp > 0 && baseStats != null && perLevelStats != null && baseHpPer5 > 0 && fistMinAttackDamage > 0 && fistAttackSpeed > 0);
        }
    }
}
