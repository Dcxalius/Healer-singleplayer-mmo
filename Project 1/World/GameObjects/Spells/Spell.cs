using Newtonsoft.Json.Linq;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Entities.Friendlies.Players;
using Project_1.GameObjects.Entities.Projectiles;
using Project_1.GameObjects.Unit;
using Project_1.Managers;
using Project_1.Textures;
using Project_1.World.GameObjects.Spells.SpellEffects;
using Project_1.World.GameObjects.Unit.Stats.Secondary;
using Project_1.World.GameObjects.Unit.Talents;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Spells
{
    internal class Spell : IDamager 
    {
        Entity owner;

        const double InstantScalarFloor = 0.4;
        const double FullScalarCastTimeMs = 3500.0;
        public string Name => spellData.Name;

        public int SpellDataId => spellData.Id;
        public int Id => id;
        int id;
        static int spellIds = 0;
        public int Rank => rank;
        public string SpellKey => BuildSpellKey(Name, Rank);
        public double CastDistance => (spellData.CastDistance + TalentFlatChange(TalentChange.Range)) * (1.0 + TalentPercentChange(TalentChange.Range));
        public double CastTime => (spellData.GetCastTimeForRank(rank) - TalentFlatChange(TalentChange.CastSpeed)) * (1.0 + TalentPercentChange(TalentChange.CastSpeed));
        public double ResourceCost => (spellData.GetResourceCostForRank(rank) - TalentFlatChange(TalentChange.Cost)) * (1.0 + TalentPercentChange(TalentChange.Cost));
        public bool Targetable(Relation.RelationToPlayer aTarget) => spellData.Targetable(aTarget);
        public GfxPath GfxPath => spellData.ButtonGfxPath;
        public SpellSchool[] SpellSchools => spellData.SpellSchools;
        public bool HasTag(string tag) => spellData.HasTag(tag);
        public bool RequiresGroundTarget => spellData.RequiresGroundTarget;
        public double GroundTargetWidth => (spellData.GroundTargetWidth + TalentFlatChange(TalentChange.Radius)) * (1.0 + TalentPercentChange(TalentChange.Radius));
        public double GroundTargetHeight => (spellData.GroundTargetHeight + TalentFlatChange(TalentChange.Radius)) * (1.0 + TalentPercentChange(TalentChange.Radius));
        public SpellData.GroundTargetShapeType GroundTargetShape => spellData.GroundTargetShape;
        public GfxPath HitEffectGfxPath => spellData.HitGfxPath;
        public CastCondition CastCondition => spellData.CastCondition;
        public bool BinarySpell => spellData.IsBinary;
        public bool OffCooldown => Cooldown <= 0 || lastTimeCasted + Cooldown < TimeManager.TotalFrameTime;
        public double RatioOfCooldownDone => Cooldown <= 0 ? 1.0 : Math.Min((TimeManager.TotalFrameTime - lastTimeCasted) / Cooldown, 1);
        double Cooldown => (spellData.GetCooldownForRank(rank) - TalentFlatChange(TalentChange.Cooldown)) * (1.0 + TalentPercentChange(TalentChange.Cooldown));

        double lastTimeCasted;

        SpellData spellData;
        readonly int rank;
        public bool IsRanked => spellData.MaxRank > 1;
        public double TalentFlatChange(TalentChange change) => talents.Sum(t => t.GetChanges(SpellDataId, owner.GetTalentRank(t.Id), change, true));
        public double TalentPercentChange(TalentChange change) => talents.Sum(t => t.GetChanges(SpellDataId, owner.GetTalentRank(t.Id), change, false));

        public void AddTalent(Talent aTalent) => talents.Add(aTalent);
        List<Talent> talents;

        public double GetPower(SpellEffect aEffect)
        {
            Debug.Assert(spellData.Effects.Contains(aEffect), "Effect does not belong to this spell.");
            return aEffect.CalculatePower(this, rank);
        }


        public static string BuildSpellKey(string spellName, int rank)
        {
            if (string.IsNullOrWhiteSpace(spellName)) return string.Empty;
            if (rank <= 1) return spellName;
            return spellName + "#" + rank;
        }

        public static bool TryParseSpellKey(string spellIdentifier, out string spellName, out int rank)
        {
            spellName = spellIdentifier?.Trim() ?? string.Empty;
            rank = 1;
            if (string.IsNullOrWhiteSpace(spellName)) return false;

            int separator = spellName.LastIndexOf('#');
            if (separator <= 0 || separator >= spellName.Length - 1) return true;

            string rankText = spellName[(separator + 1)..];
            if (!int.TryParse(rankText, out int parsedRank) || parsedRank <= 0) return true;

            spellName = spellName[..separator].TrimEnd();
            rank = parsedRank;
            return !string.IsNullOrWhiteSpace(spellName);
        }

        public Spell(Entity owner, string aName, int aRank)
        {
            if (!TryParseSpellKey(aName, out string spellName, out _))
            {
                throw new ArgumentException("Spell identifier was invalid.", nameof(aName));
            }

            this.owner = owner;
            spellData = SpellFactory.GetSpell(spellName);
            rank = spellData.ClampRank(aRank);
            lastTimeCasted = double.NegativeInfinity;
            id = spellIds++;

            //TODO: Search through owners talent trees
            talents = new List<Talent>();
        }

        public bool Cast(Entity aTarget, Entity aCaster)
        {
            ThreadAffinity.AssertSimThread();
            if (RequiresGroundTarget) return false;
            if (aTarget == null) return Cast(aCaster, aCaster);

            if (!TryCast(aTarget, aCaster)) return false;
            
            ProccessCast(aTarget, aCaster);
            
            return true;
        }

        public bool CastAt(WorldSpace aTargetPosition, Entity aCaster)
        {
            ThreadAffinity.AssertSimThread();
            if (!RequiresGroundTarget) return false;
            if (!OffCooldown) return false;

            lastTimeCasted = TimeManager.TotalFrameTime;

            Entity[] allTargets = ObjectManager.GetAllEntitiesSnapshot();
            for (int i = 0; i < allTargets.Length; i++)
            {
                Entity target = allTargets[i];
                if (target == null) continue;
                if (!spellData.Targetable(target.RelationToPlayer)) continue;
                if (!IsInsideGroundArea(aTargetPosition, target.FeetPosition)) continue;

                Trigger(aCaster, target);
            }

            return true;
        }

        bool TryCast(Entity aTarget, Entity aCaster)
        {
            ThreadAffinity.AssertSimThread();
            if (!OffCooldown) return false;
            if (!spellData.Targetable(aTarget.RelationToPlayer)) return false;
            if (spellData.Effects.Any(x => x is InstantEffect)) return true;
            List<Buff.Buff> targetBuffs = aTarget.GetAllBuffs();
            bool[] failures = new bool[spellData.Effects.Length];
            for (int i = 0; i < spellData.Effects.Length; i++)
            {
                if (spellData.Effects[i] is not StatusEffect statusEffect && spellData.Effects[i] is not OverTimeEffect)
                {
                    continue;
                }

                for (int j = 0; j < targetBuffs.Count; j++)
                {
                    if (targetBuffs[j].EffectId != spellData.Effects[i].Id) continue;
                    Buff.Buff existing = targetBuffs[j];
                    if (existing.MultipleSourceStackable && !existing.SameCaster(aCaster)) continue;
                    //Check if spell weak
                    bool numerable = spellData.Effects[i] is OverTimeEffect overTime && overTime.Numerable;
                    double effectPower = spellData.Effects[i].CalculatePower(this, rank);
                    double effectDuration = spellData.Effects[i] is LastingEffect lasting ? lasting.Duration : 0;
                    if ((!numerable && existing.Rank > rank) || (numerable && existing.Power > effectPower)) failures[i] = true;
                    //Check if time would increase or stack count would increase
                    if (((!numerable && existing.Rank == rank) || (numerable && existing.Power == effectPower)) && (existing.DurationRemaining > effectDuration && existing.MaxStackCount == existing.Count)) failures[i] = true;
                }
            }
            //TODO: Think about how failures should be handled when there are multiple effects.
            //Other posibilites are single failure, or list of failable effects (or vice versa, list of must succeed effects).
            if (failures.All(x => x)) return false;
            return true;
        }

        void ProccessCast(Entity aTarget, Entity aCaster)
        {
            ThreadAffinity.AssertSimThread();
            lastTimeCasted = TimeManager.TotalFrameTime;

            if (spellData.Travel == SpellData.TravelType.Instant)
                Trigger(aCaster, aTarget);
            else
                ProjectileFactory.CreateProjectile(aCaster, aCaster.Centre, this, aTarget);
        }

        public bool Trigger(Entity aCaster, Entity aTarget)
        {
            ThreadAffinity.AssertSimThread();
            double scalar = GetDirectEffectScalarFromCastTime(CastTime);
            for (int i = 0; i < spellData.Effects.Length; i++)
            {
                SpellEffect effect = spellData.Effects[i];
                if (effect is InstantEffect instant)
                {
                    instant.Trigger(aCaster, aTarget, this);
                }
                else if (effect is OverTimeEffect overTime)
                {
                    overTime.Trigger(aCaster, aTarget, this);
                }
                else
                {
                    effect.Trigger(aCaster, aTarget, this);
                }
                aTarget.AddEffect(new VisualEffect(spellData.HitGfxPath, 1000));
            }
            return true;
        }

        public double GetScalar(SpellEffect aEffect)
        {
            if (aEffect is InstantEffect)
            {
                return GetDirectEffectScalarFromCastTime(CastTime);
            }
            else if (aEffect is OverTimeEffect || aEffect is StatusEffect)
            {
                return 1.0;
            }
            else
            {
                throw new ArgumentException("Unknown effect type.", nameof(aEffect));
            }
        }

        static double GetDirectEffectScalarFromCastTime(double aCastTimeMs)
        {
            if (aCastTimeMs <= 0)
            {
                return InstantScalarFloor;
            }

            if (aCastTimeMs >= FullScalarCastTimeMs)
            {
                return 1.0;
            }

            double ratio = aCastTimeMs / FullScalarCastTimeMs;
            return InstantScalarFloor + (1.0 - InstantScalarFloor) * ratio;
        }

        bool IsInsideGroundArea(WorldSpace center, WorldSpace targetPosition)
        {
            if (GroundTargetShape == SpellData.GroundTargetShapeType.Rectangle)
            {
                return Math.Abs(targetPosition.X - center.X) <= GroundTargetWidth * 0.5
                    && Math.Abs(targetPosition.Y - center.Y) <= GroundTargetHeight * 0.5;
            }

            double halfWidth = Math.Max(0.001, GroundTargetWidth * 0.5);
            double halfHeight = Math.Max(0.001, GroundTargetHeight * 0.5);
            double dx = (targetPosition.X - center.X) / halfWidth;
            double dy = (targetPosition.Y - center.Y) / halfHeight;
            return dx * dx + dy * dy <= 1;
        }

        public (int, int) ScaleInstantValueForRankAndTalent((int min, int max) baseValue, int rank)
        {
            (int min, int max) returnV;
            returnV.min = (int)Math.Round(spellData.ScaleSignedValue(baseValue.min, spellData.ClampRank(rank)), MidpointRounding.AwayFromZero);
            returnV.max = (int)Math.Round(spellData.ScaleSignedValue(baseValue.max, spellData.ClampRank(rank)), MidpointRounding.AwayFromZero);
            returnV.min = (int)Math.Round((returnV.min + TalentFlatChange(TalentChange.Amount)) * (1.0 + TalentPercentChange(TalentChange.Amount)), MidpointRounding.AwayFromZero);
            returnV.max = (int)Math.Round((returnV.max + TalentFlatChange(TalentChange.Amount)) * (1.0 + TalentPercentChange(TalentChange.Amount)), MidpointRounding.AwayFromZero);
            return returnV;
        }

        public (int, int) ScaleOverTimeTickValueForRank((int min, int max) val, int tickCount, int rank)
        {
            Debug.Assert(tickCount > 0, "Tick count must be greater than 0.");
            (double min, double max) scaled = (spellData.ScaleSignedValue(val.min * tickCount, spellData.ClampRank(rank)), spellData.ScaleSignedValue(val.max * tickCount, spellData.ClampRank(rank)));
            scaled.min = (scaled.min + TalentFlatChange(TalentChange.Amount) * tickCount) * (1.0 + TalentPercentChange(TalentChange.Amount));
            scaled.max = (scaled.max + TalentFlatChange(TalentChange.Amount) * tickCount) * (1.0 + TalentPercentChange(TalentChange.Amount));
            return ((int)Math.Round(scaled.min / tickCount), (int)Math.Round(scaled.max / tickCount)) ;
        }


    }
}
