using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Unit.Stats
{
    public enum SpellSchool
    {
        Arcane,
        Fire,
        Frost,
        Holy,
        Nature,
        Shadow
    }

    internal class SpellDamage //TODO: Redo this possibly? Instead make an instance of a class per spellschool (including non/base) and then return base + the correct school. Crit, spell etc should be packed in there aswell
    {
        static public SpellSchool DamageToSpellType(DamageType aDamageType)
        {
            return aDamageType switch
            {
                DamageType.Arcane => SpellSchool.Arcane,
                DamageType.Fire => SpellSchool.Fire,
                DamageType.Frost => SpellSchool.Frost,
                DamageType.Holy => SpellSchool.Holy,
                DamageType.Nature => SpellSchool.Nature,
                DamageType.Shadow => SpellSchool.Shadow,
                DamageType.Physical => throw new ArgumentException("Invalid damage type for spell school conversion"),
                DamageType.True => throw new ArgumentException("Invalid damage type for spell school conversion"),
                _ => throw new ArgumentException("Invalid damage type for spell school conversion"),
            };
        }

        static public DamageType SpellToDamageType(SpellSchool aSpellSchool)
        {
            return aSpellSchool switch
            {
                SpellSchool.Arcane => DamageType.Arcane,
                SpellSchool.Fire => DamageType.Fire,
                SpellSchool.Frost => DamageType.Frost,
                SpellSchool.Holy => DamageType.Holy,
                SpellSchool.Nature => DamageType.Nature,
                SpellSchool.Shadow => DamageType.Shadow,
                _ => throw new ArgumentException("Invalid spell school for damage type conversion"),
            };
        }

        int baseSpellDamage;
        Dictionary<SpellSchool, int> spellDamageBySchool;

        public int BaseSpellDamage => baseSpellDamage;
        public Dictionary<SpellSchool, int> SpellDamageBySchool => spellDamageBySchool;

        public int GetSpellDamage(SpellSchool aSchool) => baseSpellDamage + (spellDamageBySchool.TryGetValue(aSchool, out int schoolDamage) ? schoolDamage : 0);
        public int GetSpellDamage(SpellSchool[] aSchools, double[] aRatio) => (int)(baseSpellDamage + (aSchools.Select((school, index) => (spellDamageBySchool.TryGetValue(school, out int schoolDamage) ? schoolDamage : 0) * aRatio[index]).Sum()));

        public void Refresh(UnitData unitData)
        {
            baseSpellDamage = unitData.Equipment.GetBaseSpellPower;
            spellDamageBySchool = unitData.Equipment.GetSchoolSpellDamage;
        }

        public SpellDamage(UnitData unitData) : this(unitData.Equipment.GetBaseSpellPower, unitData.Equipment.GetSchoolSpellDamage)
        {
            
        }

        public SpellDamage(int aBaseSpellDamage) : this(aBaseSpellDamage, new Dictionary<SpellSchool, int>())
        {

        }

        public SpellDamage() : this(0, new Dictionary<SpellSchool, int>())
        {

        }

        public SpellDamage(int aBaseSpellDamage, Dictionary<SpellSchool, int> aSpellDamageBySchool)
        {
            baseSpellDamage = aBaseSpellDamage;
            spellDamageBySchool = aSpellDamageBySchool;
        }



        [JsonConstructor]
        public SpellDamage(int baseSpellDamage, int[] spellDamageBySchool) : this(baseSpellDamage, new Dictionary<SpellSchool, int>())
        {
            Debug.Assert(spellDamageBySchool.Length == Enum.GetValues(typeof(SpellSchool)).Length);

            for (int i = 0; i < spellDamageBySchool.Length; i++)
            {
                this.spellDamageBySchool[(SpellSchool)i] = spellDamageBySchool[i];
            }   
        }

    }
}
