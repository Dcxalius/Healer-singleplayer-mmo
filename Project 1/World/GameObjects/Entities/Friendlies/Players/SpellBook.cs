using Project_1.GameObjects.Spells;
using Project_1.GameObjects.Entities;
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
                return castableSpells.ToArray();
            }
        }
        List<(int rank, Spell spell)> knownSpells;
        readonly List<Spell> castableSpells;
        readonly Dictionary<string, Spell> castableSpellsByKey;
        HashSet<string> learnableSpells;
        HashSet<string> levelOneSpells;
        Entity owner;

        public string[] LearntSpells
        {
            get
            {
                ThreadAffinity.AssertSimThread();
                string[] returnable = new string[knownSpells.Count];
                for (int i = 0; i < returnable.Length; i++)
                {
                    returnable[i] = Spell.BuildSpellKey(knownSpells[i].spell.Name, knownSpells[i].rank);
                }
                return returnable;
            }
        }
        string[] loadedSpells;

        public SpellBook(string[] aSpellsAlreadyLearnt)
        {
            ThreadAffinity.AssertSimThread();
            loadedSpells = aSpellsAlreadyLearnt;
            knownSpells = new List<(int, Spell)>();
            castableSpells = new List<Spell>();
            castableSpellsByKey = new Dictionary<string, Spell>();
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
            Friendly f = owner as Friendly;
            learnableSpells = new HashSet<string>(f.ClassData.LearnableSpells ?? Array.Empty<string>());
            levelOneSpells = new HashSet<string>(f.ClassData.LevelOneSpells ?? Array.Empty<string>());

            foreach (string levelOneSpell in levelOneSpells)
            {
                AddSpell(levelOneSpell, 1, false);
            }

            for (int i = 0; i < loadedSpells.Length; i++)
            {
                RestoreSpellProgress(loadedSpells[i]);
            }

            if (DebugManager.Mode(DebugMode.LearnKill))
            {
                AddSpell("Kill", 1);
            }
        }

        public void LearnSpell(string aSpellName)
        {
            ThreadAffinity.AssertSimThread();
            if (!Spell.TryParseSpellKey(aSpellName, out string spellName, out int explicitRank))
            {
                return;
            }

            if (!learnableSpells.Contains(spellName) && !levelOneSpells.Contains(spellName))
            {
                DebugManager.Print("Tried to learn spell named " + spellName + " but it was null.");
                return;
            }

            int currentRank = GetKnownRank(spellName);
            int targetRank = explicitRank > 1 ? explicitRank : currentRank + 1;
            if (currentRank <= 0)
            {
                targetRank = Math.Max(1, explicitRank);
            }

            AddSpell(spellName, targetRank);
        }

        public void AddSpell(Spell aSpell)
        {
            ThreadAffinity.AssertSimThread();
            if (aSpell == null) return;
            AddSpell(aSpell.Name, aSpell.Rank);
        }


        public bool HasSpell(Spell aSpell)
        {
            ThreadAffinity.AssertSimThread();
            return aSpell != null && castableSpellsByKey.ContainsKey(aSpell.SpellKey);
        }

        public bool TryGetSpell(string spellName, out Spell spell)
        {
            ThreadAffinity.AssertSimThread();
            spell = null;
            if (string.IsNullOrWhiteSpace(spellName)) return false;
            if (!Spell.TryParseSpellKey(spellName, out string parsedName, out int parsedRank)) return false;

            string exactKey = Spell.BuildSpellKey(parsedName, parsedRank);
            if (castableSpellsByKey.TryGetValue(exactKey, out spell))
            {
                return true;
            }

            int knownRank = GetKnownRank(parsedName);
            if (knownRank <= 0) return false;

            return castableSpellsByKey.TryGetValue(Spell.BuildSpellKey(parsedName, knownRank), out spell);
        }

        void RestoreSpellProgress(string spellIdentifier)
        {
            ThreadAffinity.AssertSimThread();
            if (!Spell.TryParseSpellKey(spellIdentifier, out string spellName, out int savedRank)) return;
            if (levelOneSpells.Contains(spellName) && savedRank <= 1) return;
            AddSpell(spellName, Math.Max(1, savedRank), false);
        }

        void AddSpell(string spellName, int rank, bool publish = true)
        {
            ThreadAffinity.AssertSimThread();
            SpellData spellData = SpellFactory.GetSpell(spellName);
            int clampedRank = spellData.ClampRank(rank);
            int index = knownSpells.FindIndex(x => x.spell.Name == spellName);
            if (index >= 0)
            {
                if (clampedRank <= knownSpells[index].rank) return;
                knownSpells[index] = (clampedRank, new Spell(owner, spellName, clampedRank));
            }
            else
            {
                knownSpells.Add((clampedRank, new Spell(owner, spellName, clampedRank)));
            }

            RebuildCastableSpells();
            if (spellData.CastCondition != CastCondition.None)
                owner?.RegisterReactiveHook(spellData.CastCondition);
            if (publish)
            {
                PublishSpellbookRefreshed();
            }
        }

        void RebuildCastableSpells()
        {
            ThreadAffinity.AssertSimThread();
            Dictionary<string, Spell> previous = new Dictionary<string, Spell>(castableSpellsByKey);
            castableSpells.Clear();
            castableSpellsByKey.Clear();

            for (int i = 0; i < knownSpells.Count; i++)
            {
                (int rank, Spell highestSpell) entry = knownSpells[i];
                Spell resolvedHighestRankSpell = null;
                for (int rank = 1; rank <= entry.rank; rank++)
                {
                    string key = Spell.BuildSpellKey(entry.highestSpell.Name, rank);
                    if (!previous.TryGetValue(key, out Spell spell))
                    {
                        spell = new Spell(owner, entry.highestSpell.Name, rank);
                    }

                    castableSpells.Add(spell);
                    castableSpellsByKey[key] = spell;
                    if (rank == entry.rank)
                    {
                        resolvedHighestRankSpell = spell;
                    }
                }

                knownSpells[i] = (entry.rank, resolvedHighestRankSpell ?? new Spell(owner, entry.highestSpell.Name, entry.rank));
            }
        }

        public int GetKnownRank(string spellIdentifier)
        {
            if (!Spell.TryParseSpellKey(spellIdentifier, out string spellName, out _)) return 0;
            int index = knownSpells.FindIndex(x => x.spell.Name == spellName);
            if (index < 0) return 0;
            return knownSpells[index].rank;
        }

        void PublishSpellbookRefreshed()
        {
            if (owner == null) return;
            MailboxManager.PublishUiEvent(new SpellbookRefreshed(owner.RenderId, castableSpells.Select(x => x.SpellKey).ToArray()));
        }
    }
}
