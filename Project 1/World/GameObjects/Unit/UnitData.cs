using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Project_1.Camera;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Spawners;
using Project_1.GameObjects.Unit.Classes;
using Project_1.GameObjects.Unit.Resources;
using Project_1.GameObjects.Unit.Stats;
using Project_1.Items;
using Project_1.Items.SubTypes;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Project_1.GameObjects.Entities.Friendlies.Players;
using Project_1.Managers;
using Project_1.World.GameObjects.Unit.Stats.Primary;
using Project_1.World.GameObjects.Unit.Talents;

namespace Project_1.GameObjects.Unit
{
    enum UnitType
    {
        Player,
        Normal,
        Elite,
        Boss
    }
    class UnitData
    {
        static void AssertSimThread() => ThreadAffinity.AssertSimThread();
        static readonly JsonSerializer equipmentSerializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });
        
        public string Name => name;
        string name;

        [JsonProperty(PropertyName = "ClassName")]
        string className => classData.Name;

        public (int id, int rank)[] LearntTalents => learntTalents;
        (int id, int rank)[] learntTalents;

        public double GetTalentPrimaryStatFlat(PrimaryStats.PrimaryStat aStat) => GetTalentPrimaryStatChange(aStat, true);
        public double GetTalentPrimaryStatPercent(PrimaryStats.PrimaryStat aStat) => GetTalentPrimaryStatChange(aStat, false);
        Entity owner;

        internal void SetOwner(Entity aOwner)
        {
            owner = aOwner;
        }

        public double GetStatusStatFlat(string aStat) => owner?.GetStatusStatFlat(aStat) ?? 0d;
        public double GetStatusStatPercent(string aStat) => owner?.GetStatusStatPercent(aStat) ?? 0d;

        public double ApplyStatusModifiers(string aStat, double aValue)
        {
            return (aValue + GetStatusStatFlat(aStat)) * (1d + GetStatusStatPercent(aStat));
        }

        public int ApplyStatusModifiersInt(string aStat, double aValue)
        {
            return (int)Math.Round(ApplyStatusModifiers(aStat, aValue), MidpointRounding.AwayFromZero);
        }

        public T GetTalentSecondaryStat<T>(string aSecondaryStat)
        {
            double value = 0d;
            if (learntTalents == null)
            {
                return default;
            }

            for (int i = 0; i < learntTalents.Length; i++)
            {
                if (learntTalents[i].rank <= 0) continue;
                value += TalentFactory.GetTalent(learntTalents[i].id).GetSecondaryStatChange(aSecondaryStat, learntTalents[i].rank);
            }

            if (typeof(T) == typeof(int))
            {
                return (T)(object)(int)Math.Round(value, MidpointRounding.AwayFromZero);
            }

            if (typeof(T) == typeof(float))
            {
                return (T)(object)(float)value;
            }

            if (typeof(T) == typeof(double))
            {
                return (T)(object)value;
            }

            throw new NotImplementedException();
        }

        double GetTalentPrimaryStatChange(PrimaryStats.PrimaryStat aStat, bool aFlat)
        {
            double value = 0d;
            if (learntTalents == null)
            {
                return 0d;
            }

            for (int i = 0; i < learntTalents.Length; i++)
            {
                if (learntTalents[i].rank <= 0) continue;
                value += TalentFactory.GetTalent(learntTalents[i].id).GetPrimaryStatChange(aStat, learntTalents[i].rank, aFlat);
            }

            return value;
        }


        public UnitType UnitType => unitType;
        UnitType unitType;

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
        double CurrentHp => baseStats.Health.CurrentHealth;
        [JsonProperty]
        double CurrentResource => baseStats.Resource.Value;

        [JsonIgnore]
        public BaseStats BaseStats => baseStats;
        BaseStats baseStats;

        [JsonIgnore]
        public SecondaryStats SecondaryStats => secondaryStats;
        SecondaryStats secondaryStats;

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
        public WeaponSkill WeaponSkill => weaponSkill;
        WeaponSkill weaponSkill;

        public int DefenseSkill => defenseSkill;
        int defenseSkill;

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

        public void InitializeWeaponSkill(Entity aOwner)
        {
            AssertSimThread();
            if (weaponSkill == null)
            {
                weaponSkill = new WeaponSkill(aOwner, classData);
                return;
            }
            weaponSkill.SetOwner(aOwner);
            weaponSkill.EnsureClassSkills(classData);
        }

        [JsonProperty("Equipment", TypeNameHandling = TypeNameHandling.Auto)]
        Items.SubTypes.Equipment[] SerializableEquipment => equipment.EquippedItems;

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

            baseStats = new BaseStats(this, classData, level.CurrentLevel, equipment.EquipmentStats);

            gfxPath = aData.GfxPath;
            corpseGfxPath = aData.CorpseGfxPath;

            learntTalents = new (int id, int rank)[0];
            position = aSpawn;
            velocity = WorldSpace.Zero;
            momentum = WorldSpace.Zero;
            destination = new Destination(null);
            secondaryStats = new SecondaryStats(this);
            unitType = aData.UnitType;
            Assert();
        }


        [JsonConstructor]
        public UnitData(string name, string corpseGfxName, string className, Relation.RelationToPlayer? relation, int level, int experience, (int, int)[] learntTalents,
            float currentHp, float currentResource, object equipment, WorldSpace position, WorldSpace momentum, WorldSpace velocity, List<WorldSpace> destinations, int defenseSkill, WeaponSkill weaponSkill = null)
        {
            this.name = name;
            Debug.Assert(relation.HasValue);
            relationData = new Relation(relation);
            SetClassData(relation.Value, className);
            this.level = new Level(level, experience);
            SetTalents(learntTalents);
            SetEquipment(equipment);
            this.position = new WorldSpace(position);
            this.momentum = new WorldSpace(momentum);
            this.velocity = new WorldSpace(velocity);
            this.destination = new Destination(destinations);
            this.defenseSkill = defenseSkill;
            this.weaponSkill = weaponSkill;
            baseStats = new BaseStats(this, classData, this.level.CurrentLevel, this.equipment.EquipmentStats, currentHp, currentResource);
            this.unitType = UnitType.Player;

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
            this.secondaryStats = new SecondaryStats(this);
            Assert();
        }

        void SetTalents((int id, int rank)[] aLearntTalents)
        {
            if (aLearntTalents == null)
            {
                this.learntTalents = classData.GenerateEmptyTalents;
                return;
            }
            
            (int id, int rank)[] emptyTalents = classData.GenerateEmptyTalents;
            if (emptyTalents.Length == aLearntTalents.Length)
            {
                this.learntTalents = aLearntTalents;
                return;
            }
            (int, int)[] newTalents = new (int, int)[emptyTalents.Length];

            for (int i = 0; i < emptyTalents.Length; i++)
            {
                (int id, int rank) emptyTalent = emptyTalents[i];
                if (aLearntTalents.Contains(emptyTalent))
                {
                    newTalents[i] = aLearntTalents.Single(x => x.id == emptyTalent.id);
                    continue;
                }
                newTalents[i] = emptyTalent;
            }

            learntTalents = newTalents;
        }
        void SetEquipment(object aEquipment)
        {
            if (aEquipment == null)
            {
                if (relationData.ToPlayer == Unit.Relation.RelationToPlayer.Self || relationData.ToPlayer == Unit.Relation.RelationToPlayer.Friendly)
                {
                    equipment = new Equipment((classData as FriendlyClassData).GearAllowed);
                }
                else equipment = new Equipment();
                return;
            }

            if (TryDeserializeEquippedItems(aEquipment, out Items.SubTypes.Equipment[] equippedItems))
            {
                if (relationData.ToPlayer == Unit.Relation.RelationToPlayer.Self || relationData.ToPlayer == Unit.Relation.RelationToPlayer.Friendly)
                {
                    equipment = new Equipment((classData as FriendlyClassData).GearAllowed, equippedItems);
                }
                else equipment = new Equipment(equippedItems);
                return;
            }

            int?[] legacyEquipment = aEquipment switch
            {
                int?[] directIds => directIds,
                JToken token => token.ToObject<int?[]>(),
                _ => null
            };
            if (legacyEquipment == null)
            {
                if (relationData.ToPlayer == Unit.Relation.RelationToPlayer.Self || relationData.ToPlayer == Unit.Relation.RelationToPlayer.Friendly)
                {
                    equipment = new Equipment((classData as FriendlyClassData).GearAllowed);
                }
                else equipment = new Equipment();
                return;
            }

            if (relationData.ToPlayer == Unit.Relation.RelationToPlayer.Self || relationData.ToPlayer == Unit.Relation.RelationToPlayer.Friendly)
            {
                equipment = new Equipment((classData as FriendlyClassData).GearAllowed, legacyEquipment);
            }
            else equipment = new Equipment(legacyEquipment);
        }

        static bool TryDeserializeEquippedItems(object source, out Items.SubTypes.Equipment[] equippedItems)
        {
            equippedItems = source as Items.SubTypes.Equipment[];
            if (equippedItems != null) return true;

            if (source is not JToken token || token.Type != JTokenType.Array)
            {
                return false;
            }

            if (token.Children().Any(x => x?.Type == JTokenType.Object))
            {
                equippedItems = token.ToObject<Items.SubTypes.Equipment[]>(equipmentSerializer);
                return equippedItems != null;
            }

            return false;
        }

        public void SetClassData(Relation.RelationToPlayer aRelation, string aClassName)
        {
            classData = aRelation switch
            {
                Unit.Relation.RelationToPlayer.Self => ObjectFactory.GetPlayerClass(aClassName),
                Unit.Relation.RelationToPlayer.Friendly => ObjectFactory.GetAllyClass(aClassName),
                Unit.Relation.RelationToPlayer.Neutral or Unit.Relation.RelationToPlayer.Hostile => ObjectFactory.GetMobClass(aClassName),
                _ => throw new Exception("Incorrect relation."),
            };
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

        public bool Tick(bool aInCombat)
        {
            AssertSimThread();
            bool healthChanged = Health.HealthRegenTick(aInCombat, SecondaryStats.Defense.Hp5, SecondaryStats.Defense.SpiritHp5);
            float previousResource = Resource.Value;
            Resource.TickRegen(aInCombat);
            if (Resource.Value != previousResource)
            {
                healthChanged = true;
            }

            return healthChanged;
        }

        public void GainExp(int aExpAmount)
        {
            AssertSimThread();
            int previousLevel = level.CurrentLevel;
            if (!level.GainExp(aExpAmount)) return;

            int levelsGained = level.CurrentLevel - previousLevel;
            for (int i = 0; i < levelsGained; i++)
            {
                baseStats.LevelUp();
            }
        }

    
    }
}
