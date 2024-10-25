using Newtonsoft.Json;
using Project_1.GameObjects.Entities;
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

        string name;
        double cooldown;
        double castTime;
        SpellEffect[] effects;
        UnitData.RelationToPlayer[] acceptableTargets;

        [JsonConstructor]
        public SpellData(string name, int[] effectIds, UnitData.RelationToPlayer[] acceptableTargets, double castTime = -1, double cooldown = -1)
        {
            this.name = name;
            this.cooldown = cooldown;
            this.castTime = castTime;

            List<SpellEffect> tempEffects = new List<SpellEffect>();
            for (int i = 0; i < effectIds.Length; i++)
            {
                tempEffects.Add(SpellFactory.GetSpellEffect(effectIds[i]));
            }
            effects = tempEffects.ToArray();

            this.acceptableTargets = acceptableTargets;
        }
        
        void Assert()
        {
            Debug.Assert(name != null, "Name was null in Spell.");
            Debug.Assert(cooldown != -1, "Cooldown was not set in Spell.");
            Debug.Assert(castTime != -1, "Cast Time was not set in Spell.");
            Debug.Assert(effects.Length > 0, "Spell had no effects.");
            Debug.Assert(acceptableTargets.Length > 0, "Spell had no targets.");
        }
    }
}
