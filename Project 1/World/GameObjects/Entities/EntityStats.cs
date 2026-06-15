using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects.Entities.Corspes;
using Project_1.GameObjects.FloatingTexts;
using Project_1.GameObjects.Spawners;
using Project_1.GameObjects.Unit;
using Project_1.GameObjects.Unit.Classes;
using Project_1.GameObjects.Unit.Resources;
using Project_1.GameObjects.Unit.Stats;
using Project_1.Items;
using Project_1.Particles;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Project_1.GameObjects.Unit.Equipment;
using Project_1.GameObjects.Entities.Friendlies;
using System.Globalization;
using Project_1.Managers;
using Project_1.World.Items.Enchantments;
using Project_1.Tiles;
using Project_1.World.GameObjects.Unit.Stats.Secondary;

namespace Project_1.GameObjects.Entities
{
    internal partial class Entity
    {
        public int GetTalentRank(int aTalentId) => unitData.LearntTalents.SingleOrDefault(x => x.id == aTalentId).rank;

        protected UnitData UnitData => unitData;
        UnitData unitData;

        protected bool HasNamePlate => hasNamePlate;
        bool hasNamePlate;
        public UnitType UnitType => unitData.UnitType;

        public virtual Color MinimapColor => Color.White;

        public bool HasDestination => unitData.Destination.HasDestination;
        public Destination Destination => unitData.Destination;
        public Color RelationColor => unitData.RelationData.RelationColor();
        public Relation.RelationToPlayer RelationToPlayer => unitData.RelationData.ToPlayer;
        protected Relation Relation => unitData.RelationData;
        public string Name => unitData.Name;
        public string Class => unitData.ClassData.Name;
        public virtual ClassData ClassData => unitData.ClassData;
        public int CurrentLevel => unitData.Level.CurrentLevel;
        public bool Alive => unitData.Health.CurrentHealth > 0;
        public bool FullHealth => unitData.Health.MaxHealth == unitData.Health.CurrentHealth;
        public double MaxHealth => unitData.Health.MaxHealth;
        public double CurrentHealth
        {
            get => unitData.Health.CurrentHealth;
            set
            {
                unitData.Health.CurrentHealth = value;
                FlagForRefresh();
            }
        }

        public SecondaryStats SecondaryStats => unitData.SecondaryStats;
        internal void RefreshSecondaryStats()
        {
            ThreadAffinity.AssertSimThread();
            unitData.SecondaryStats.Refresh(unitData);
        }
        public int DefenseSkill => unitData.DefenseSkill;
        public WeaponSkill WeaponSkill => unitData.WeaponSkill;
        public bool IsDualWielding => Equipment.IsDualWielding;
        public Equipment Equipment => unitData.Equipment;
        public Level Level => unitData.Level;
        public Resource.ResourceType ResourceType => unitData.Resource.Type;
        public float MaxResource => unitData.Resource.MaxValue;
        public float CurrentResource => unitData.Resource.Value;
        public Resource Resource => unitData.Resource;

        public Color ResourceColor => unitData.Resource.ResourceColor;

        public override float MaxSpeed => unitData.MovementData.MaxSpeed;

        public PairReport PrimaryStatReport => unitData.BaseStats.StatReport;

        
        internal EntityUiSnapshot BuildUiSnapshot()
        {
            return new EntityUiSnapshot(
                RenderId,
                Name,
                Class,
                RelationToPlayer.ToRelationToPlayerKind(),
                RelationColor,
                CurrentLevel,
                CurrentHealth,
                MaxHealth,
                CurrentResource,
                MaxResource,
                ResourceColor,
                FeetPosition,
                WorldRectangle.Height,
                ResolveNamePlateAnchorWorldPosition());
        }

        //Q: Is this wanted? Or should Nameplates only have their 2d screen pos
        WorldSpace3D ResolveNamePlateAnchorWorldPosition()
        {
            float heightInWorldUnits = Math.Max(0f, WorldRectangle.Height / (float)Math.Max(1, Tile.Size.Y));
            return new WorldSpace3D(
                FeetPosition.X / Tile.Size.X,
                ResolveSurfaceHeight(FeetPosition) + heightInWorldUnits,
                FeetPosition.Y / Tile.Size.Y);
        }

        //Q: Why is this in Entity? 
        static float ResolveSurfaceHeight(WorldSpace aFeetPosition)
        {
            Chunk chunk = TileManager.GetChunkUnder(aFeetPosition);
            if (chunk == null) return 0f;

            Point gridPosition = TileManager.GetGridPos(aFeetPosition);
            int localX = PositiveModulo(gridPosition.X, Chunk.ChunkSize.X);
            int localY = PositiveModulo(gridPosition.Y, Chunk.ChunkSize.Y);

            for (int z = Chunk.ChunkHeight - 1; z >= 0; z--)
            {
                if (chunk.GetBlock(localX, localY, z) != null)
                {
                    return z + 1f;
                }
            }

            return 0f;
        }

        //TODO: Move to helper
        static int PositiveModulo(int aValue, int aDivisor)
        {
            int result = aValue % aDivisor;
            return result < 0 ? result + aDivisor : result;
        }

        //TODO: Misplace fields
        bool namePlateRequiresUpdate;
        ParticleBase bloodsplatter;

        //TODO: Handle running delete from here maybe?
        bool AmIDead()
        {
            if (!Alive)
            {
                Death();
                return true;
            }
            return false;
        }

        void TargetAliveCheck()
        {
            //TODO: Move this into a seperate target handling system that dispatches things like removing dead targets, cycling between them, focusing, and dispatching attacks
            if (Target == null) return;
            if (Target.Alive) return;

            RemoveTarget();

        }
        protected virtual void Death()
        {
            ThreadAffinity.AssertSimThread();
            Events.Clear();
            for (int i = 0; i < aggroTablesIAmOn.Count; i++)
            {
                aggroTablesIAmOn[i].RemoveFromAggroTable(this);
            }

            ObjectManager.RemoveEntity(this);
            RemoveNamePlate();
            Corpse c = new Corpse(unitData.CorpseGfxPath, unitData.LootTable, FeetPosition);

        }


        public bool ResourceGain(Entity aEntity, float aValue, Resource.ResourceType aResourceType)
        {
            //TODO: Feels clunky, at least needs breaking up
            //TODO: Write summary, what entails with a false/true return?
            ThreadAffinity.AssertSimThread();
            if (aResourceType != ResourceType) return false;

            if (MaxResource == CurrentResource) return false;

            float value = aValue;

            if (MaxResource < CurrentResource + value)
            {
                value = MaxResource - CurrentResource;
            }

            for (int i = 0; i < aggroTablesIAmOn.Count; i++)
            {
                aggroTablesIAmOn[i].AddToAggroTable(aEntity, value);
            }

            unitData.Resource.Value += value;
            FlagForRefresh();
            return true;
        }

       
        public virtual bool TakeHealing(Entity aHealer, float aHealingTaken)
        {
            //TODO: Break up
            ThreadAffinity.AssertSimThread();
            if (FullHealth) return false;

            double value = CalculateHealing(aHealingTaken);
            for (int i = 0; i < aggroTablesIAmOn.Count; i++)
            {
                aggroTablesIAmOn[i].AddToAggroTable(aHealer, (float)(value / aggroTablesIAmOn.Count));
            }

            WorldSpace dir = GetDirOfFloatingText(aHealer.FeetPosition);

            SpawnFlyingText(FormatHealthDelta(value), dir, Color.Green, Color.Black, 1f);//TODO: Change color to green once text border has been implemented
            FlagForRefresh();
            return true;
        }

        double CalculateHealing(double aHealingTaken) => ApplyHealthDelta(aHealingTaken); //TODO: Shift to prop/getters

        double ApplyHealthDelta(double aDelta)
        {
            double before = unitData.Health.CurrentHealth;
            double after = before + aDelta;
            if (after < 0) after = 0;
            if (after > MaxHealth) after = MaxHealth;
            unitData.Health.CurrentHealth = after;
            return after - before;
        }

        //TODO: Create a health class to store and do this type of work
        string FormatHealthDelta(double aDelta)
        {
            double abs = Math.Abs(aDelta);
            const double epsilon = 0.005d;
            if (abs < epsilon) return "0";
            double rounded = Math.Round(abs);
            if (Math.Abs(abs - rounded) < epsilon) return rounded.ToString(CultureInfo.InvariantCulture);
            return abs.ToString("0.##", CultureInfo.InvariantCulture);
        }

        //TODO: Move to setter
        public abstract void ExpToParty(int aExpAmount);

        //TODO: What does it mean for mobs/npcs to gain exp
        public void GainExperience(int aExpAmount)
        {
            ThreadAffinity.AssertSimThread();
            unitData.GainExp(aExpAmount);

            if (!(this is Friendly)) return;
            MailboxManager.PublishUiEvent(new ExperienceRefreshed(RenderId, RelationToPlayer.ToRelationToPlayerKind(), CurrentLevel, Level.Experience));
        }


        protected void CreateNamePlate()
        {
            //TODO: Since this doesn't Create but rather sends a dispatch to create, it should prob not have Create in the name
            ThreadAffinity.AssertSimThread();
            hasNamePlate = true;
            MailboxManager.PublishUiEvent(new NamePlateAdded(BuildUiSnapshot()));

        }

        protected void RemoveNamePlate()
        {
            //TODO: Since this doesn't Remove but rather sends a dispatch to remove, it should prob not have Remove in the name
            ThreadAffinity.AssertSimThread();
            hasNamePlate = false;
            MailboxManager.PublishUiEvent(new NamePlateRemoved(RenderId));
        }

        protected void FlagForRefresh() => namePlateRequiresUpdate = true; //TODO: Shift to prop/getters

        public virtual void RefreshPlates()
        {
            ThreadAffinity.AssertSimThread();
            if (!namePlateRequiresUpdate) return;
            MailboxManager.PublishUiEvent(new PlateRefreshRequested(BuildUiSnapshot()));
        }

        //TODO: Summaries which should give a heads up that this can return null
        public Item EquipInParticularSlot(Items.SubTypes.Equipment aEquipment, Slot aSlot)
        {
            ThreadAffinity.AssertSimThread();
            Item item = Equipment.EquipInParticularSlot(aEquipment, aSlot);
            unitData.BaseStats.RefreshEquipmentStats(Equipment.EquipmentStats);
            FlagForRefresh();
            return item;
        }
        public Item Equip(Items.SubTypes.Equipment aEquipment)
        {
            ThreadAffinity.AssertSimThread();
            Item item = Equipment.Equip(aEquipment);
            unitData.BaseStats.RefreshEquipmentStats(Equipment.EquipmentStats);
            FlagForRefresh();

            return item;
        }

        public (Item, Item) EquipTwoHander(Items.SubTypes.Equipment aEquipment)
        {
            ThreadAffinity.AssertSimThread();
            (Item, Item) returnable = Equipment.EquipTwoHander(aEquipment);
            unitData.BaseStats.RefreshEquipmentStats(Equipment.EquipmentStats);
            FlagForRefresh();

            return returnable;
        }

        public bool ApplyPermanentEnchantment(Slot aSlot, EnchantmentData enchantmentData)
        {
            ThreadAffinity.AssertSimThread();
            if (!Equipment.ApplyPermanentEnchantment(aSlot, enchantmentData)) return false;

            unitData.BaseStats.RefreshEquipmentStats(Equipment.EquipmentStats);
            FlagForRefresh();
            return true;
        }
    }
}
