using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Entities.Players;
using Project_1.GameObjects.Entities.Projectiles;
using Project_1.GameObjects.Unit;
using Project_1.GameObjects.Unit.Stats;
using Project_1.Managers;
using Project_1.Textures;
using SharpDX.DXGI;
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
        [DebuggerStepThrough]
        public static GfxPath GetGfxPath(Spell aSpell)
        {
            if (aSpell == null) return new GfxPath(GfxType.SpellImage, null);

            return new GfxPath(GfxType.SpellImage, aSpell.spellData.Name);
        }

        public string Name { get => spellData.Name; }
        public float CastDistance { get => spellData.CastDistance; }
        public double CastTime { get => spellData.CastTime; }
        public float ResourceCost { get => spellData.ResourceCost; }
        public bool Targetable(Relation.RelationToPlayer aTarget) => spellData.Targetable(aTarget);
        public GfxPath GfxPath { get => spellData.ButtonGfxPath; }
        public SpellSchool[] SpellSchools => spellData.SpellSchools;
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
            if (aTarget == null) return Cast(aCaster, aCaster);

            if (!TryCast(aTarget)) return false;
            
            ProccessCast(aTarget, aCaster);
            
            return true;
        }

        bool TryCast(Entity aTarget)
        {
            if (!OffCooldown) return false;
            if (!spellData.Targetable(aTarget.RelationToPlayer)) return false;
            return true;
        }

        void ProccessCast(Entity aTarget, Entity aCaster)
        {
            lastTimeCasted = TimeManager.TotalFrameTime;

            if (spellData.Travel == SpellData.TravelType.Instant)
                Trigger(aCaster, aTarget);
            else
                ProjectileFactory.CreateProjectile(aCaster, aCaster.Centre, this, aTarget);
        }

        public bool Trigger(Entity aTarget, Entity aCaster)
        {
            for (int i = 0; i < spellData.Effects.Length; i++)
            {
                spellData.Effects[i].Trigger(aTarget, aCaster);
                aTarget.AddEffect(new VisualEffect(spellData.HitGfxPath, 1000));
            }
            return true;
        }
    }
}
