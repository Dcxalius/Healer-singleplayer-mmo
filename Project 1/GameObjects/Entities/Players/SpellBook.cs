using Project_1.GameObjects.Spells;
using Project_1.UI.HUD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Entities.Players
{
    internal class SpellBook
    {
        public Spell[] Spells { get => knownSpells.ToArray(); }
        List<Spell> knownSpells;
        List<Spell> learnableSpells;
        Entity owner;

        public string[] LearntSpells
        {
            get
            {
                string[] returnable = new string[knownSpells.Count];
                for (int i = 0; i < returnable.Length; i++)
                {
                    returnable[i] = knownSpells[i].Name;
                }
                return returnable;
            }
        }
        string[] loadedSpells;

        public SpellBook(string[] aSpellsAlreadyLearnt)
        {
            loadedSpells = aSpellsAlreadyLearnt;
            knownSpells = new List<Spell>();
        }

        public SpellBook() : this(Array.Empty<string>())
        {
        }



        public void Init(Entity aEntity)
        {
            owner = aEntity;
            learnableSpells = new List<Spell>();
            //if (owner.Class)
            //{
            //  Add spells from class here.
            //}
            for (int i = 0; i < loadedSpells.Length; i++) LearnSpell(loadedSpells[i]);
        }

        public void LearnSpell(string aSpellName) => knownSpells.Add(new Spell(aSpellName, owner));

        public void AddSpell(Spell aSpell)
        {
            knownSpells.Add(aSpell);
            HUDManager.AddSpellToSpellBook(aSpell);
        }


        public bool HasSpell(Spell aSpell) => knownSpells.Contains(aSpell);
    }
}
