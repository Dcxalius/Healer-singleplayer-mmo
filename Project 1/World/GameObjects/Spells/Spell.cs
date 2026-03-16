using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Entities.Friendlies.Players;
using Project_1.GameObjects.Entities.Projectiles;
using Project_1.GameObjects.Unit;
using Project_1.GameObjects.Unit.Stats;
using Project_1.Managers;
using Project_1.Textures;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Spells
{
    internal class Spell : IDamager 
    {
        const double InstantScalarFloor = 0.4;
        const double FullScalarCastTimeMs = 3500.0;
        static void AssertSimThread() => ThreadAffinity.AssertSimThread();
        [DebuggerStepThrough]
        public static GfxPath GetGfxPath(Spell aSpell)
        {
            if (aSpell == null) return new GfxPath(GfxType.SpellImage, null);

            return aSpell.GfxPath;
        }

        public string Name { get => spellData.Name; }
        public float CastDistance { get => spellData.CastDistance; }
        public double CastTime { get => spellData.CastTime; }
        public float ResourceCost { get => spellData.ResourceCost; }
        public bool Targetable(Relation.RelationToPlayer aTarget) => spellData.Targetable(aTarget);
        public GfxPath GfxPath { get => spellData.ButtonGfxPath; }
        public SpellSchool[] SpellSchools => spellData.SpellSchools;
        public bool RequiresGroundTarget => spellData.RequiresGroundTarget;
        public float GroundTargetWidth => spellData.GroundTargetWidth;
        public float GroundTargetHeight => spellData.GroundTargetHeight;
        public SpellData.GroundTargetShapeType GroundTargetShape => spellData.GroundTargetShape;
        public GfxPath HitEffectGfxPath => spellData.HitGfxPath;
        public bool BinarySpell => spellData.IsBinary;
        public bool OffCooldown { get => lastTimeCasted + spellData.Cooldown < TimeManager.TotalFrameTime; }
        public double RatioOfCooldownDone { get => Math.Min((TimeManager.TotalFrameTime - lastTimeCasted) / spellData.Cooldown, 1); }
        double lastTimeCasted;


        SpellData spellData;

        public Spell(string aName)
        {
            spellData = SpellFactory.GetSpell(aName);
            lastTimeCasted = double.NegativeInfinity;
        }

        public bool Cast(Entity aTarget, Entity aCaster)
        {
            AssertSimThread();
            if (RequiresGroundTarget) return false;
            if (aTarget == null) return Cast(aCaster, aCaster);

            if (!TryCast(aTarget)) return false;
            
            ProccessCast(aTarget, aCaster);
            
            return true;
        }

        public bool CastAt(WorldSpace aTargetPosition, Entity aCaster)
        {
            AssertSimThread();
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

        bool TryCast(Entity aTarget)
        {
            AssertSimThread();
            if (!OffCooldown) return false;
            if (!spellData.Targetable(aTarget.RelationToPlayer)) return false;
            return true;
        }

        void ProccessCast(Entity aTarget, Entity aCaster)
        {
            AssertSimThread();
            lastTimeCasted = TimeManager.TotalFrameTime;

            if (spellData.Travel == SpellData.TravelType.Instant)
                Trigger(aCaster, aTarget);
            else
                ProjectileFactory.CreateProjectile(aCaster, aCaster.Centre, this, aTarget);
        }

        public bool Trigger(Entity aCaster, Entity aTarget)
        {
            AssertSimThread();
            double scalar = GetDirectEffectScalarFromCastTime(spellData.CastTime);
            for (int i = 0; i < spellData.Effects.Length; i++)
            {
                spellData.Effects[i].Trigger(aCaster, aTarget, scalar);
                aTarget.AddEffect(new VisualEffect(spellData.HitGfxPath, 1000));
            }
            return true;
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
                return MathF.Abs(targetPosition.X - center.X) <= GroundTargetWidth * 0.5f
                    && MathF.Abs(targetPosition.Y - center.Y) <= GroundTargetHeight * 0.5f;
            }

            float halfWidth = Math.Max(0.001f, GroundTargetWidth * 0.5f);
            float halfHeight = Math.Max(0.001f, GroundTargetHeight * 0.5f);
            float dx = (targetPosition.X - center.X) / halfWidth;
            float dy = (targetPosition.Y - center.Y) / halfHeight;
            return dx * dx + dy * dy <= 1f;
        }
    }
}
