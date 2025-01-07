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
                (Equipment.AttackStyle, Attack, Attack) weaponAttacks = equipment.GetWeaponAttacks();

                if (weaponAttacks.Item1 == Equipment.AttackStyle.None) return (Equipment.AttackStyle.None, primaryStats.Attack, null);
                
                return weaponAttacks;
            }
        }

        public Equipment Equipment => equipment;
        Equipment equipment;

        public Movement MovementData => movementData;
        Movement movementData;

        public Attack Attack => primaryStats.Attack;



        [JsonIgnore]
        public LootTable LootTable { get => LootFactory.GetData(name); }

        [JsonConstructor]
        public UnitData(string name, string className, Relation.RelationToPlayer? relation, int level, int experience,
            float currentHp, float currentResource, int?[] equipment)
        {
            this.name = name;
            relationData = new Relation(relation);
            classData = new ClassData(relation.Value, className);
            this.level = new Level(level, experience);
            movementData = new Movement(classData.Speed, classData.MaxSpeed);
            this.equipment = new Equipment(equipment);

            Debug.Assert(relation.HasValue);
            
            primaryStats = new PrimaryStats(classData, this.level.CurrentLevel, currentHp, currentResource);


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
