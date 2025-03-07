using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Project_1.Camera;
using Project_1.GameObjects.Entities;
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
    class UnitData
    {
        public string Name => name;
        string name;

        [JsonProperty(PropertyName = "ClassName")]
        string className => classData.Name;

        [JsonIgnore]
        public ClassData ClassData => classData;
        ClassData classData;

        [JsonProperty]
        Relation.RelationToPlayer Relation => relationData.ToPlayer;
        [JsonIgnore]
        public Relation RelationData => relationData;
        Relation relationData;

        [JsonProperty]
        int Experience => level.Experience;
        [JsonProperty("Level")]
        int LevelAsInt => level.CurrentLevel;
        [JsonIgnore]
        public Level Level => level;
        Level level;

        [JsonProperty]
        float CurrentHp => baseStats.Health.CurrentHealth;
        [JsonProperty]
        float CurrentResource => baseStats.Resource.Value;

        [JsonIgnore]
        public BaseStats BaseStats => baseStats;
        BaseStats baseStats;

        [JsonIgnore]
        public Health Health => baseStats.Health;
        [JsonIgnore]
        public Resource Resource => baseStats.Resource;

        #region Movement
        [JsonProperty("Destinations")]
        public List<WorldSpace> Destinations => destination.DestinationsAsWP;
        [JsonIgnore]
        public Destination Destination => destination;
        Destination destination;
        
        [JsonIgnore]
        public Movement MovementData => classData.Movement;
        public WorldSpace Position
        {
            get => position;
            set => position = value;
        }
        WorldSpace position;

        public WorldSpace Momentum
        {
            get => momentum;
            set => momentum = value;
        }
        WorldSpace momentum;

        public WorldSpace Velocity
        {
            get => velocity;
            set => velocity = value;
        }

        WorldSpace velocity;
        #endregion

        #region Attack
        [JsonIgnore]
        public AttackData AttackData
        {
            get
            {
                AttackData weaponAttacks = equipment.GetWeaponAttacks();

                if (weaponAttacks.Style == AttackData.AttackStyle.None) return baseStats.FistAttack;

                return weaponAttacks;
            }
        }
        [JsonIgnore]
        public ref TimeSpan NextAvailableMainHandAttack 
        {
            get => ref nextAvailableMainHandAttack;
        }
        TimeSpan nextAvailableMainHandAttack;//TODO: Load from save


        [JsonIgnore]
        
        public ref TimeSpan NextAvailableOffHandAttack 
        {
            get => ref nextAvailableOffHandAttack;
        }
        TimeSpan nextAvailableOffHandAttack;//TODO: Load from save
        #endregion

        [JsonProperty("Equipment")]
        int?[] EquipmentAsId => equipment.GetEquipementAsIds;

        [JsonIgnore]
        public Equipment Equipment =>  equipment;
        Equipment equipment;

        #region gfx
        [JsonIgnore] 
        public GfxPath GfxPath => gfxPath;
        readonly GfxPath gfxPath;

        [JsonIgnore] 
        public GfxPath CorpseGfxPath => corpseGfxPath;
        readonly GfxPath corpseGfxPath;
        #endregion

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
            destination = new Destination(null);
            Assert();
        }


        [JsonConstructor]
        public UnitData(string name, string corpseGfxName, string className, Relation.RelationToPlayer? relation, int level, int experience,
            float currentHp, float currentResource, int?[] equipment, WorldSpace position, WorldSpace momentum, WorldSpace velocity, List<WorldSpace> destinations)
        {
            this.name = name;
            Debug.Assert(relation.HasValue);
            relationData = new Relation(relation);
            classData = new ClassData(relation.Value, className);
            this.level = new Level(level, experience);
            if (equipment != null)
            {
                this.equipment = new Equipment(equipment);

            }
            else
            {
                this.equipment = new Equipment();

            }
            this.position = new WorldSpace(position);
            this.momentum = new WorldSpace(momentum);
            this.velocity = new WorldSpace(velocity);
            this.destination = new Destination(destinations);
            
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

            nextAvailableMainHandAttack = TimeSpan.Zero;
            nextAvailableOffHandAttack = TimeSpan.Zero;
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
            baseStats.LevelUp();
        }

    
    }
}
