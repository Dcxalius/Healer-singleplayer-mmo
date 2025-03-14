using Project_1.GameObjects.Spells;
using Project_1.GameObjects.Unit;
using Project_1.Managers;
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
            string[] learnables = aEntity.ClassData.LearnableSpells;
            for (int i = 0; i < learnables.Length; i++)
            {
                learnableSpells.Add(new Spell(learnables[i], owner));
            }

            string[] levelOneSpells = aEntity.ClassData.LevelOneSpells;

            for (int i = 0; i < levelOneSpells.Length; i++)
            {
                AddSpell(new Spell(levelOneSpells[i], owner));
            }

            for (int i = 0; i < loadedSpells.Length; i++)
            {
                if (levelOneSpells.Contains(loadedSpells[i])) continue;
                LearnSpell(loadedSpells[i]);
            }

            if (DebugManager.Mode(DebugMode.LearnKill))
            {
                AddSpell(new Spell("Kill", owner));
            }
        }

        public void LearnSpell(string aSpellName)
        {
            Spell s = learnableSpells.Find(x => x.Name == aSpellName);
            if (s == null)
            {
                DebugManager.Print(GetType(), "Tried to learn spell named " + aSpellName + " but it was null.");
                return;
            }

            AddSpell(s);
        }

        public void AddSpell(Spell aSpell)
        {
            knownSpells.Add(aSpell);
            HUDManager.AddSpellToSpellBook(aSpell);
        }


        public bool HasSpell(Spell aSpell) => knownSpells.Contains(aSpell);
    }
}
