using Project_1.GameObjects.Spells;
using Project_1.GameObjects.Unit;
using Project_1.Managers;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.UI.HUD.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_1.GameObjects.Entities.Friendlies.Players;

namespace Project_1.GameObjects.Entities.Friendlies.Players
{
    internal class SpellBook
    {
        public Spell[] Spells
        {
            get
            {
                ThreadAffinity.AssertSimThread();
                return knownSpells.ToArray();
            }
        }
        List<Spell> knownSpells;
        List<Spell> learnableSpells;
        Entity owner;

        public string[] LearntSpells
        {
            get
            {
                ThreadAffinity.AssertSimThread();
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
            ThreadAffinity.AssertSimThread();
            loadedSpells = aSpellsAlreadyLearnt;
            knownSpells = new List<Spell>();
        }

        public SpellBook() : this(Array.Empty<string>())
        {
        }



        public void Init(Entity aEntity)
        {
            ThreadAffinity.AssertSimThread();
            if (aEntity.RelationToPlayer != Relation.RelationToPlayer.Self) return;
            owner = aEntity as Friendly;
            aEntity = aEntity as Friendly;
            learnableSpells = new List<Spell>();
            Friendly f = owner as Friendly;
            string[] learnables = f.ClassData.LearnableSpells;
            for (int i = 0; i < learnables.Length; i++)
            {
                learnableSpells.Add(new Spell(learnables[i]));
            }

            string[] levelOneSpells = f.ClassData.LevelOneSpells;

            for (int i = 0; i < levelOneSpells.Length; i++)
            {
                AddSpell(new Spell(levelOneSpells[i]));
            }

            for (int i = 0; i < loadedSpells.Length; i++)
            {
                if (levelOneSpells.Contains(loadedSpells[i])) continue;
                LearnSpell(loadedSpells[i]);
            }

            if (DebugManager.Mode(DebugMode.LearnKill))
            {
                AddSpell(new Spell("Kill"));
            }
        }

        public void LearnSpell(string aSpellName)
        {
            ThreadAffinity.AssertSimThread();
            Spell s = learnableSpells.Find(x => x.Name == aSpellName);
            if (s == null)
            {
                DebugManager.Print("Tried to learn spell named " + aSpellName + " but it was null.");
                return;
            }

            AddSpell(s);
        }

        public void AddSpell(Spell aSpell)
        {
            ThreadAffinity.AssertSimThread();
            knownSpells.Add(aSpell);
            Mailboxes.PublishUiEvent(new SpellbookRefreshed(owner.RenderId, knownSpells.Select(x => x.Name).ToArray()));
        }


        public bool HasSpell(Spell aSpell)
        {
            ThreadAffinity.AssertSimThread();
            return knownSpells.Contains(aSpell);
        }

        public bool TryGetSpell(string spellName, out Spell spell)
        {
            ThreadAffinity.AssertSimThread();
            spell = null;
            if (string.IsNullOrWhiteSpace(spellName)) return false;
            spell = knownSpells.Find(x => x.Name == spellName);
            return spell != null;
        }
    }
}
