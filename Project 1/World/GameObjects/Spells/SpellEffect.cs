using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Unit.Stats;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Spells
{
    internal enum AbilityStatSource
    {
        Spell,
        Attack
    }

    internal class SpellEffect
    {
        static void AssertSimThread() => ThreadAffinity.AssertSimThread();
        static int GetId => nextId++;
        static int nextId;
        public int Id { get; private set; }
        int id;
        public string Name => name;
        string name;

        public bool IsBinary => isBinary;
        bool isBinary;

        public virtual string Description => "";

        public bool sourceStackable;

        public int MaxStackCount;

        public virtual string GetRankDescription(SpellData spellData, int spellRank)
        {
            return Description;
        }

        public virtual AbilityStatSource StatSource => AbilityStatSource.Spell;

        public HashSet<SpellSchool> SpellSchools => spellSchools;
        HashSet<SpellSchool> spellSchools;

        public SpellEffect(string aName, bool aIsBinary, HashSet<SpellSchool> aSpellSchools)
        {
            AssertSimThread();
            spellSchools = aSpellSchools;
            id = GetId;
            name = aName;
            isBinary = aIsBinary;

            Debug.Assert(name != null, "No name");
        }

        public virtual bool Trigger(Entity aCaster, Entity aTarget, double aSpellPowerScalar = 1.0)
        {
            AssertSimThread();
            throw new NotImplementedException($"Tried to trigger effect {name} with no trigger implementation.");
        }
    }
}
