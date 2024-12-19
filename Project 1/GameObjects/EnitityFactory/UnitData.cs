using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Project_1.GameObjects.EnitityFactory.Resources;
using Project_1.Items;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.EnitityFactory
{
    struct UnitData
    {
        public string Name => name;
        string name;

        public Level Level => level;
        Level level;

        public BasePrimaryStats PrimaryStats => primaryStats;
        BasePrimaryStats primaryStats;

        public Health HealthData => healthData;
        Health healthData;
        
        public Attack AttackData => attackData;
        Attack attackData;

        public Relation RelationData => relationData;
        Relation relationData;

        public Movement MovementData => movementData;
        Movement movementData;

        
        

        public Resource Resource => resource;
        Resource resource;





        [JsonIgnore]
        public LootTable LootTable { get => LootFactory.GetData(name); }

        [JsonConstructor]
        public UnitData(string name, float maxHealth, Relation.RelationToPlayer? relation, float speed, float maxSpeed,
            float secondsPerAttack, float attackDamage, float attackRange, float healthPer5, Resource.ResourceType resourceType,
            float maxResource, float resourceRegen, int strength, int agility, int intellect, int spirit, int stamina, int level, int experience)
        {
            this.name = name;
            primaryStats = new BasePrimaryStats(strength, agility, intellect, spirit, stamina);
            healthData = new Health(maxHealth, healthPer5, primaryStats);
            attackData = new Attack(attackRange, attackDamage, secondsPerAttack);
            relationData = new Relation(relation);
            movementData = new Movement(speed, maxSpeed);
            this.level = new Level(level, experience);

            switch (resourceType)
            {
                case Resource.ResourceType.Mana:
                    resource = new Mana(maxResource, resourceRegen);
                    break;
                case Resource.ResourceType.Energy:
                    resource = new Energy(maxResource);
                    break;
                case Resource.ResourceType.Rage:
                    throw new NotImplementedException();
                case Resource.ResourceType.None:
                    resource = new None();
                    break;
                default:
                    throw new NotImplementedException();
            }

            Assert();
        }

        void Assert()
        {

            if (name == null)
            {
                throw new Exception("UnitData improperly set");
            }
        }

        public void Update()
        {
            resource.Update();
        }

        public void Tick()
        {
            healthData.HealthRegenTick();
        }

        public void GainExp(int aExpAmount)
        {
            if (!level.GainExp(aExpAmount)) return;


        }

        public int ExpReward(int aLevelOfKiller)
        {
            if (aLevelOfKiller >= 60) return 0;

            int levelOfMob = level.CurrentLevel;
            int xp = levelOfMob * 5 + 45;
            if (levelOfMob > aLevelOfKiller)
            {
                xp *= (int)(1 + 0.05 * Math.Min(levelOfMob - aLevelOfKiller, 4));   
            }
            else if (levelOfMob < aLevelOfKiller) 
            { 
                xp *= 1 - (aLevelOfKiller - levelOfMob) / Level.ZD(levelOfMob); 
            }

            return xp;
        }
    
    }
}
