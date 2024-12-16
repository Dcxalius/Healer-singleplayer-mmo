using Newtonsoft.Json;
using Project_1.GameObjects.Entities;
using Project_1.Textures;
using SharpDX.Direct3D11;
using SharpDX.Direct3D9;
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

        public string Name { get => name; }
        string name;
        public double Cooldown { get => cooldown; }
        double cooldown;

        public bool Targetable(UnitData.RelationToPlayer aTarget) { return acceptableTargets.Contains(aTarget); }
        UnitData.RelationToPlayer[] acceptableTargets;

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

        [JsonConstructor]
        public SpellData(string name, string buttonGfx, string hitEffectGfx, string[] effects, TravelType travelType, UnitData.RelationToPlayer[] acceptableTargets, float castDistance, double castTime = -1, double cooldown = -1, float resourceCost = -1)
        {
            this.name = name;
            this.buttonGfxName = buttonGfx;
            this.cooldown = cooldown * 1000;
            this.castTime = castTime * 1000;
            this.hitEffectName = hitEffectGfx;

            List<SpellEffect> tempEffects = new List<SpellEffect>();
            for (int i = 0; i < effects.Length; i++)
            {
                tempEffects.Add(SpellFactory.GetSpellEffect(effects[i]));
            }
            this.effects = tempEffects.ToArray();
            this.resourceCost = resourceCost;
            this.acceptableTargets = acceptableTargets;
            this.castDistance = castDistance;
            this.travelType = travelType;
            Assert();
        }
        
        void Assert()
        {
            Debug.Assert(name != null, "Name was null in Spell.");
            Debug.Assert(cooldown >= 0, "Cooldown was not set in Spell.");
            Debug.Assert(castTime >= 0, "Cast Time was not set in Spell.");
            Debug.Assert(resourceCost != -1, "Resource Cost was not set in Spell.");
            Debug.Assert(effects.Length > 0, "Spell had no effects.");
            Debug.Assert(castDistance > 0, "distance had no value.");
            Debug.Assert(acceptableTargets.Length > 0, "Spell had no targets.");
            Debug.Assert(travelType > TravelType.None, "Spell never assigned travel type.");
        }

        //void Trigger(Entity aCaster, Entity aTarget)
        //{
        //    effects[0].Trigger(aCaster, aTarget);
        //}
    }
}
