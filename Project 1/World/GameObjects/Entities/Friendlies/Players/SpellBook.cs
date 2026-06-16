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
using System.Diagnostics;

namespace Project_1.GameObjects.Entities.Friendlies.Players
{
    internal class SpellBook
    {
        public enum PublishSpellUpdate
        {
            No,
            Yes
        }

        public Spell[] Spells
        {
            get
            {
                ThreadAffinity.AssertSimThread();
                return castableSpells.ToArray();
            }
        }

        public (int spellId, int rank)[] SaveSpells
        {
            get
            {
                return new (int, int)[] { (0, 0) }; //TODO: Implement this
            }
        }

        List<(int rank, Spell spell)> knownSpells;
        readonly List<Spell> castableSpells; //Q: What does these two track? Aren't all known spells per definition castable?
        readonly Dictionary<string, Spell> castableSpellsByKey; //TODO: This should be done with ID + rank?
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

        //public SpellBook((int id, int rank) aSpellsAlreadyLearnt)
        public SpellBook(string[] aSpellsAlreadyLearnt) //TODO: Spells should move to an ID system, rather than a namebased one.
        {
            ThreadAffinity.AssertSimThread();
            loadedSpells = aSpellsAlreadyLearnt;
            knownSpells = new List<(int, Spell)>();
            castableSpells = new List<Spell>();
            castableSpellsByKey = new Dictionary<string, Spell>();
        }

        public SpellBook() : this(Array.Empty<string>()){}



        public void Init(Entity aEntity)
        {
            ThreadAffinity.AssertSimThread();
            if (aEntity.RelationToPlayer != Relation.RelationToPlayer.Self) return; //TODO: Npcs should also be able to have a spellbook.
            owner = aEntity as Friendly;
            aEntity = aEntity as Friendly;
            Friendly f = owner as Friendly;
            learnableSpells = new HashSet<string>(f.ClassData.LearnableSpells ?? Array.Empty<string>());
            levelOneSpells = new HashSet<string>(f.ClassData.LevelOneSpells ?? Array.Empty<string>());

            foreach (string levelOneSpell in levelOneSpells)
            {
                AddSpell(levelOneSpell, 1, PublishSpellUpdate.No);
            }

            for (int i = 0; i < loadedSpells.Length; i++)
            {
                RestoreSpellProgress(loadedSpells[i]);
            }

            if (DebugManager.Mode(DebugMode.LearnKill) && owner is Player) //TODO: Currently this implementation causes the debug to spit out an error message when we restore a player with Kill on bars.
            {
                AddSpell("Kill", 1);
            }

            PublishSpellbookRefreshed(); //Q: Just added this, feels weird that we add spells and then pressumably update them elsewhere
        }

        public void LearnSpell(string aSpellName) //TODO: Should use ID instead
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

        public bool TryGetSpell(string spellName, out Spell spell)
        {
            ThreadAffinity.AssertSimThread();
            spell = null;
            if (string.IsNullOrWhiteSpace(spellName)) return false;
            if (!Spell.TryParseSpellKey(spellName, out string parsedName, out int parsedRank)) return false; //TODO: This is very ugly and due to a misshandling of saved spells. Spells in spellbook should be saved as ID, Rank array. That way we won't have to do this wonky string stuff

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
            if (!Spell.TryParseSpellKey(spellIdentifier, out string spellName, out int savedRank)) return; //
            if (levelOneSpells.Contains(spellName) && savedRank <= 1) return;
            AddSpell(spellName, Math.Max(1, savedRank), PublishSpellUpdate.No); //Q: Why do we Max 1 here? If a spell has rank 0 we have bigger problems somewhere I feel like
        }

        void AddSpell(string spellName, int rank, PublishSpellUpdate publishSpellUpdate = PublishSpellUpdate.Yes) //TODO: Should use ID instead
        {
            ThreadAffinity.AssertSimThread();
            SpellData spellData = SpellFactory.GetSpell(spellName);
            int clampedRank = spellData.ClampRank(rank); //Q: If spellrank is invalid, don't we wanna alert instead just clamping and ignoring?
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
            if (spellData.CastCondition != CastCondition.None) owner?.RegisterReactiveHook(spellData.CastCondition);
            if (publishSpellUpdate == PublishSpellUpdate.Yes) PublishSpellbookRefreshed(); 
        }

        void RebuildCastableSpells()
        {
            //Q: Why is this done? The spellbook already has a list of all castable spell, the ones in the knownSpells Dict 
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

        public int GetKnownRank(string spellIdentifier) //TODO: Should use id instead. Also probably a namechange to GetHighestKnownRank
        {
            if (!Spell.TryParseSpellKey(spellIdentifier, out string spellName, out _)) return 0;
            int index = knownSpells.FindIndex(x => x.spell.Name == spellName);
            if (index < 0) return 0;
            return knownSpells[index].rank;
        }

        void PublishSpellbookRefreshed()
        {
            Debug.Assert(owner != null);
            MailboxManager.PublishUiEvent(new SpellbookRefreshed(owner.RenderId, castableSpells.Select(x => x.SpellKey).ToArray()));
        }
    }
}
