using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Unit.Stats
{
    internal class SpellDamage
    {
        int baseSpellDamage;
        Dictionary<SpellSchools, int> spellDamageBySchool;

        public int BaseSpellDamage => baseSpellDamage;
        public Dictionary<SpellSchools, int> SpellDamageBySchool => spellDamageBySchool;

        public int GetSpellDamage(SpellSchools aSchool) => baseSpellDamage + (spellDamageBySchool.TryGetValue(aSchool, out int schoolDamage) ? schoolDamage : 0);
        public int GetSpellDamage(SpellSchools[] aSchools, double[] aRatio) => (int)(baseSpellDamage + (aSchools.Select((school, index) => (spellDamageBySchool.TryGetValue(school, out int schoolDamage) ? schoolDamage : 0) * aRatio[index]).Sum()));


        public SpellDamage(int aBaseSpellDamage, Dictionary<SpellSchools, int> aSpellDamageBySchool)
        {
            baseSpellDamage = aBaseSpellDamage;
            spellDamageBySchool = aSpellDamageBySchool;
        }

        public SpellDamage(int aBaseSpellDamage) : this(aBaseSpellDamage, new Dictionary<SpellSchools, int>())
        {

        }

        public SpellDamage() : this(0, new Dictionary<SpellSchools, int>())
        {

        }

        [JsonConstructor]
        public SpellDamage(int baseSpellDamage, int[] spellDamageBySchool) : this(baseSpellDamage, new Dictionary<SpellSchools, int>())
        {
            Debug.Assert(spellDamageBySchool.Length == Enum.GetValues(typeof(SpellSchools)).Length);

            for (int i = 0; i < spellDamageBySchool.Length; i++)
            {
                this.spellDamageBySchool[(SpellSchools)i] = spellDamageBySchool[i];
            }   
        }
    }


    public enum SpellSchools
    {
        Arcane,
        Fire,
        Frost,
        Holy,
        Nature,
        Shadow
    }
}
