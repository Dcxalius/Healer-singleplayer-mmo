using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Unit.Stats;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Spells
{
    internal class SpellEffect
    {
        static int GetId => nextId++;
        static int nextId;
        public int Id { get; private set; }
        int id;
        public string Name => name;
        string name;

        public bool IsBinary => isBinary;
        bool isBinary;

        public HashSet<SpellSchool> SpellSchools => spellSchools;
        HashSet<SpellSchool> spellSchools;

        public SpellEffect(string aName, bool aIsBinary, HashSet<SpellSchool> aSpellSchools)
        {
            spellSchools = aSpellSchools;
            id = GetId;
            name = aName;
            isBinary = aIsBinary;

            Debug.Assert(name != null, "No name");
        }

        public virtual bool Trigger(Entity aTarget, Entity aCaster)
        {
            

            return false;
        }
    }
}
