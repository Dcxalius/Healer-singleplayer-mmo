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
        

        public string Name { get => name; }
        public double Cooldown { get => cooldown; }

        public bool Targetable(UnitData.RelationToPlayer aTarget) { return acceptableTargets.Contains(aTarget); }

        public float ResourceCost { get => resourceCost; }

        public GfxPath GfxPath { get => new GfxPath(GfxType.SpellImage, gfxName); }
        string name;
        string gfxName;
        double cooldown;
        double castTime;
        float resourceCost;

        public SpellEffect[] Effects { get => effects; }
        SpellEffect[] effects;
        UnitData.RelationToPlayer[] acceptableTargets;

        [JsonConstructor]
        public SpellData(string name, string gfxName, string[] effects, UnitData.RelationToPlayer[] acceptableTargets, double castTime = -1, double cooldown = -1, float resourceCost = -1)
        {
            this.name = name;
            this.gfxName = gfxName;
            this.cooldown = cooldown * 1000;
            this.castTime = castTime * 1000;

            List<SpellEffect> tempEffects = new List<SpellEffect>();
            for (int i = 0; i < effects.Length; i++)
            {
                tempEffects.Add(SpellFactory.GetSpellEffect(effects[i]));
            }
            this.effects = tempEffects.ToArray();
            this.resourceCost = resourceCost;
            this.acceptableTargets = acceptableTargets;
        }
        
        void Assert()
        {
            Debug.Assert(name != null, "Name was null in Spell.");
            Debug.Assert(cooldown != -1, "Cooldown was not set in Spell.");
            Debug.Assert(castTime != -1, "Cast Time was not set in Spell.");
            Debug.Assert(resourceCost != -1, "Resource Cost was not set in Spell.");
            Debug.Assert(effects.Length > 0, "Spell had no effects.");
            Debug.Assert(acceptableTargets.Length > 0, "Spell had no targets.");
        }

        void Trigger(Entity aCaster, Entity aTarget)
        {
            effects[0].Trigger(aCaster, aTarget);
        }
    }
}
