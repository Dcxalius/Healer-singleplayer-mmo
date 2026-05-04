using Newtonsoft.Json;
using Project_1.GameObjects.Unit;
using Project_1.Textures;
using Project_1.World.GameObjects.Spells.SpellEffects;
using Project_1.World.GameObjects.Unit.Stats.Secondary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Spells
{
    internal struct SpellData
    {
        public enum TravelType
        {
            None,
            Instant,
            Projectile
        }

        public enum GroundTargetShapeType
        {
            Circle,
            Rectangle
        }

        public int Id => id;
        int id;
        public string Name { get => name; }
        string name;
        public string Description => description;
        string description;
        public double Cooldown { get => cooldown; }
        double cooldown;

        public bool Targetable(Relation.RelationToPlayer aTarget) { return acceptableTargets.Contains(aTarget); }
        Relation.RelationToPlayer[] acceptableTargets;

        public float ResourceCost { get => resourceCost; }
        float resourceCost;

        public float CastDistance { get => castDistance; }
        float castDistance;

        public double CastTime { get => castTime; }
        double castTime;

        public GfxPath ButtonGfxPath { get => new GfxPath(GfxType.SpellImage, buttonGfxName); }
        string buttonGfxName;

        public GfxPath HitGfxPath { get => new GfxPath(GfxType.Effect, hitEffectName); }
        string hitEffectName;

        public SpellEffect[] Effects { get => effects; }
        SpellEffect[] effects;

        public TravelType Travel { get => travelType; }
        TravelType travelType;

        public bool RequiresGroundTarget => requiresGroundTarget;
        bool requiresGroundTarget;

        public float GroundTargetWidth => groundTargetWidth;
        float groundTargetWidth;

        public float GroundTargetHeight => groundTargetHeight;
        float groundTargetHeight;

        public GroundTargetShapeType GroundTargetShape => groundTargetShape;
        GroundTargetShapeType groundTargetShape;

        public CastCondition CastCondition => castCondition;
        CastCondition castCondition;

        public bool IsBinary => isBinary;
        bool isBinary;
        public SpellSchool[] SpellSchools => spellSchools;
        SpellSchool[] spellSchools;

        public int FirstLevel => firstLevel;
        int firstLevel;

        public int LevelGap => levelGap;
        int levelGap;

        public int MaxRank => maxRank;
        int maxRank;

        public double Mod => mod;
        double mod;

        public int Add => add;
        int add;

        public double ResourceMulti => resourceMulti;
        double resourceMulti;

        public double RankCastTimeIncrease => rankCastTimeIncrease;
        double rankCastTimeIncrease;

        public double MaxCastTime => maxCastTime;
        double maxCastTime;

        public double CoolDownTimeIncrease => coolDownTimeIncrease;
        double coolDownTimeIncrease;

        public double MaxCooldown => maxCooldown;
        double maxCooldown;



        [JsonConstructor]
        public SpellData(
            int id,
            string name,
            string buttonGfx,
            string hitEffectGfx,
            string[] effects,
            TravelType travelType,
            Relation.RelationToPlayer[] acceptableTargets,
            float castDistance,
            SpellSchool[] spellSchools,
            bool isBinary = false,
            string description = null,
            double castTime = -1,
            double cooldown = -1,
            float resourceCost = -1,
            int firstLevel = 1,
            int levelGap = 0,
            int maxRank = 1,
            double mod = 1.0,
            int add = 0,
            double resourceMulti = 1.0,
            double rankCastTimeIncrease = 0.0,
            double maxCastTime = 0.0,
            double coolDownTimeIncrease = 0.0,
            double maxCooldown = 0.0,
            bool requiresGroundTarget = false,
            float groundTargetWidth = 0,
            float groundTargetHeight = -1,
            GroundTargetShapeType groundTargetShape = GroundTargetShapeType.Circle,
            CastCondition castCondition = CastCondition.None)
        {
            this.name = name;
            this.description = description ?? string.Empty;
            this.buttonGfxName = buttonGfx;
            this.cooldown = cooldown * 1000;
            this.castTime = castTime * 1000;
            this.hitEffectName = hitEffectGfx;

            List<SpellEffect> tempEffects = new List<SpellEffect>();
            for (int i = 0; i < effects.Length; i++)
            {
                tempEffects.Add(SpellFactory.GetSpellEffect(effects[i]));
            }
            this.id = id;
            this.effects = tempEffects.ToArray();
            this.resourceCost = resourceCost;
            this.acceptableTargets = acceptableTargets;
            this.castDistance = castDistance;
            this.travelType = travelType;
            this.isBinary = isBinary;
            this.spellSchools = spellSchools;
            this.firstLevel = Math.Max(1, firstLevel);
            this.levelGap = Math.Max(0, levelGap);
            this.maxRank = Math.Max(1, maxRank);
            this.mod = mod <= 0 ? 1.0 : mod;
            this.add = Math.Max(0, add);
            this.resourceMulti = resourceMulti <= 0 ? 1.0 : resourceMulti;
            this.rankCastTimeIncrease = Math.Max(0.0, rankCastTimeIncrease * 1000.0);
            this.maxCastTime = Math.Max(0.0, maxCastTime * 1000.0);
            this.coolDownTimeIncrease = Math.Max(0.0, coolDownTimeIncrease * 1000.0);
            this.maxCooldown = Math.Max(0.0, maxCooldown * 1000.0);
            this.requiresGroundTarget = requiresGroundTarget;
            this.groundTargetWidth = groundTargetWidth;
            this.groundTargetHeight = groundTargetHeight < 0 ? groundTargetWidth : groundTargetHeight;
            this.groundTargetShape = groundTargetShape;
            this.castCondition = castCondition;
            Assert();
        }

        public int ClampRank(int rank)
        {
            return Math.Clamp(rank, 1, maxRank);
        }

        public int GetMaxRankForLevel(int level)
        {
            if (level < firstLevel) return 0;
            if (maxRank <= 1) return 1;
            if (levelGap <= 0) return maxRank;

            int earnedRanks = 1 + (level - firstLevel) / levelGap;
            return Math.Clamp(earnedRanks, 1, maxRank);
        }

        public int GetRequiredLevelForRank(int rank)
        {
            int clampedRank = ClampRank(rank);
            // TODO: LevelOneSpells can grant rank 1 before this required level. If that remains intentional,
            // split "learned at start" from "trainer unlock level" so descriptors and trainer UI do not lie.
            if (clampedRank <= 1) return firstLevel;
            return firstLevel + levelGap * (clampedRank - 1);
        }

        public double GetCooldownForRank(int rank)
        {
            return ScaleFlatWithCap(cooldown, coolDownTimeIncrease, maxCooldown, ClampRank(rank));
        }

        public double GetCastTimeForRank(int rank)
        {
            return ScaleFlatWithCap(castTime, rankCastTimeIncrease, maxCastTime, ClampRank(rank));
        }

        public float GetResourceCostForRank(int rank)
        {
            int clampedRank = ClampRank(rank);
            double scaled = resourceCost * Math.Pow(resourceMulti, clampedRank - 1);
            return (float)scaled;
        }

        

        void Assert()
        {
            Debug.Assert(name != null, "Name was null in Spell.");
            Debug.Assert(cooldown >= 0, "Cooldown was not set in Spell.");
            Debug.Assert(castTime >= 0, "Cast Time was not set in Spell.");
            Debug.Assert(resourceCost != -1, "Resource Cost was not set in Spell.");
            Debug.Assert(effects.Length > 0, "Spell had no effects.");
            Debug.Assert(acceptableTargets.Length > 0, "Spell had no targets.");
            Debug.Assert(travelType > TravelType.None, "Spell never assigned travel type.");
            Debug.Assert(firstLevel > 0, "First level must be positive.");
            Debug.Assert(levelGap >= 0, "Level gap must not be negative.");
            Debug.Assert(maxRank > 0, "Max rank must be positive.");
            Debug.Assert(mod > 0, "Mod must be positive.");
            Debug.Assert(add >= 0, "Add must not be negative.");
            Debug.Assert(resourceMulti > 0, "Resource multiplier must be positive.");
            if (requiresGroundTarget)
            {
                Debug.Assert(groundTargetWidth > 0, "Ground target width was not set in Spell.");
                Debug.Assert(groundTargetHeight > 0, "Ground target height was not set in Spell.");
            }
        }

        static double ScaleFlatWithCap(double baseValue, double flatIncrease, double capValue, int rank)
        {
            double scaled = baseValue + flatIncrease * Math.Max(0, rank - 1);
            if (capValue > 0)
            {
                scaled = Math.Min(scaled, capValue);
            }
            return scaled;
        }

        public double ScaleSignedValue(double baseValue, int rank)
        {
            int rankOffset = Math.Max(0, rank - 1);
            if (rankOffset == 0) return baseValue;

            double sign = Math.Sign(baseValue);
            if (sign == 0) sign = 1;

            double adjusted = baseValue + sign * add * rankOffset;
            return adjusted * Math.Pow(mod, rankOffset);
        }
        //void Trigger(Entity aCaster, Entity aTarget)
        //{
        //    effects[0].Trigger(aCaster, aTarget);
        //}
    }
}
