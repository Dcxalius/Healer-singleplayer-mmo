using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Project_1.GameObjects.Unit.Resources;
using Project_1.Items;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Unit
{
    struct UnitData
    {
        public string Name => name;
        string name;

        public ClassData ClassData => classData;
        ClassData classData;

        public Relation RelationData => relationData;
        Relation relationData;

        public Level Level => level;
        Level level;

        public PrimaryStats PrimaryStats => primaryStats;
        PrimaryStats primaryStats;

        
        public Health HealthData => primaryStats.Health;
        public Resource Resource => primaryStats.Resource;


        public (Equipment.AttackStyle, Attack, Attack) AttackData
        {
            get
            {
                var weaponAttacks = eq.GetWeaponAttacks();

                if (weaponAttacks.Item1 == Equipment.AttackStyle.None) return (Equipment.AttackStyle.None, primaryStats.Attack, null);
                
                return weaponAttacks;
            }
        }

        public Equipment eq;

        public Movement MovementData => movementData;
        Movement movementData;



        [JsonIgnore]
        public LootTable LootTable { get => LootFactory.GetData(name); }

        [JsonConstructor]
        public UnitData(string name, string className, Relation.RelationToPlayer? relation, int level, int experience,
            float currentHp, float currentResource,
            float speed, float maxSpeed, //TODO: These are sus
            float secondsPerAttack, float attackDamage, float attackRange) //And these too
        {
            this.name = name;
            relationData = new Relation(relation);
            this.level = new Level(level, experience);
            movementData = new Movement(speed, maxSpeed);

            Debug.Assert(relation.HasValue);
            classData = new ClassData(relation.Value, className);
            
            primaryStats = new PrimaryStats(classData, this.level.CurrentLevel, currentHp, currentResource);
            attackData = new Attack(attackDamage, secondsPerAttack);

            eq = new Equipment();

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
            Resource.Update();
        }

        public void Tick()
        {
            HealthData.HealthRegenTick();
        }

        public void GainExp(int aExpAmount)
        {
            if (!level.GainExp(aExpAmount)) return;
            //Level up
            primaryStats.LevelUp(classData);
        }

    
    }
}
