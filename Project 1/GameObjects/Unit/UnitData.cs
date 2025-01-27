using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Project_1.Camera;
using Project_1.GameObjects.Spawners;
using Project_1.GameObjects.Unit.Resources;
using Project_1.GameObjects.Unit.Stats;
using Project_1.Items;
using Project_1.Textures;
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

        public BaseStats BaseStats => baseStats;
        BaseStats baseStats;

        
        public Health Health => baseStats.Health;
        public Resource Resource => baseStats.Resource;

        public WorldSpace Position => position;
        WorldSpace position;

        public WorldSpace Momentum => momentum;
        WorldSpace momentum;

        public WorldSpace Velocity => velocity;
        WorldSpace velocity;

        public (Equipment.AttackStyle, Attack, Attack) AttackData
        {
            get
            {
                (Equipment.AttackStyle, Attack, Attack) weaponAttacks = equipment.GetWeaponAttacks();

                if (weaponAttacks.Item1 == Equipment.AttackStyle.None) return (Equipment.AttackStyle.None, baseStats.Attack, null);
                
                return weaponAttacks;
            }
        }

        public Equipment Equipment =>  equipment;
        Equipment equipment;

        public Movement MovementData => classData.Movement;

        public Attack Attack => baseStats.Attack;

        public GfxPath GfxPath => gfxPath;
        readonly GfxPath gfxPath;

        public GfxPath CorpseGfxPath => corpseGfxPath;
        readonly GfxPath corpseGfxPath;


        [JsonIgnore]
        public LootTable LootTable { get => LootFactory.GetData(name); }

        public UnitData(MobData aData, WorldSpace aSpawn)
        {
            name = aData.Name;
            relationData = aData.RelationData;
            classData = aData.ClassData;
            level = aData.Level;
            equipment = aData.Equipment;

            baseStats = new BaseStats(classData, level.CurrentLevel, equipment.EquipmentStats);

            gfxPath = aData.GfxPath;
            corpseGfxPath = aData.CorpseGfxPath;

            position = aSpawn;
            velocity = WorldSpace.Zero;
            momentum = WorldSpace.Zero;
            Assert();
        }


        [JsonConstructor]
        public UnitData(string name, string corpseGfxName, string className, Relation.RelationToPlayer? relation, int level, int experience,
            float currentHp, float currentResource, int?[] equipment, Vector2 position, Vector2 momentum, Vector2 velocity)
        {
            this.name = name;
            Debug.Assert(relation.HasValue);
            relationData = new Relation(relation);
            classData = new ClassData(relation.Value, className);
            this.level = new Level(level, experience);
            this.equipment = new Equipment(equipment);
            this.position = new WorldSpace(position);
            this.momentum = new WorldSpace(momentum);
            this.velocity = new WorldSpace(velocity);
            
            baseStats = new BaseStats(classData, this.level.CurrentLevel, this.equipment.EquipmentStats, currentHp, currentResource);

            gfxPath = new GfxPath(GfxType.Object, className);

            if (corpseGfxName != null)
            {
                corpseGfxPath = new GfxPath(GfxType.Corpse, corpseGfxName);
            }
            else
            {
                corpseGfxPath = new GfxPath(GfxType.Corpse, "Corpse");
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

        //public void Update()
        //{
        //    Resource.Update();
        //}

        public void Tick()
        {
            Health.HealthRegenTick();
            Resource.TickRegen();
        }

        public void GainExp(int aExpAmount)
        {
            if (!level.GainExp(aExpAmount)) return;
            //Level up
            baseStats.LevelUp(classData);
        }

    
    }
}
