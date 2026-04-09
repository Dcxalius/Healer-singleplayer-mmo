using Project_1.GameObjects;
using Project_1.GameObjects.Entities.Friendlies.Npcs;
using Project_1.GameObjects.Entities.Friendlies.Players;
using Project_1.GameObjects.Spells;
using Project_1.GameObjects.Unit;
using Project_1.Managers;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Project_1.GameObjects.Entities.Friendlies.Players
{
    internal static class SpellTrainingRouter
    {
        static bool initialized;

        public static void Init()
        {
            ThreadAffinity.AssertMainThread();
            if (initialized) return;
            initialized = true;

            MailboxManager.RegisterSimCommandType<SpellTrainingWindowRequested>();
            MailboxManager.RegisterSimCommandType<SpellTrainingPurchaseRequested>();
            MailboxManager.Sim.Subscribe<SpellTrainingWindowRequested>(HandleWindowRequested);
            MailboxManager.Sim.Subscribe<SpellTrainingPurchaseRequested>(HandlePurchaseRequested);
        }

        static void HandleWindowRequested(SpellTrainingWindowRequested _)
        {
            ThreadAffinity.AssertSimThread();
            if (!TryGetTrainingContext(out Player player, out Npc trainer)) return;
            PublishTrainerWindow(player, trainer.Name);
        }

        static void HandlePurchaseRequested(SpellTrainingPurchaseRequested e)
        {
            ThreadAffinity.AssertSimThread();
            if (!TryGetTrainingContext(out Player player, out Npc trainer)) return;
            if (!Spell.TryParseSpellKey(e.SpellKey, out string spellName, out int rank)) return;

            SpellData spellData = SpellFactory.GetSpell(spellName);
            int requiredLevel = spellData.GetRequiredLevelForRank(rank);
            int highestKnownRank = player.SpellBook.GetKnownRank(spellName);
            bool learned = highestKnownRank >= rank;
            bool previousRanksLearnt = rank <= 1 || highestKnownRank >= rank - 1;
            bool appropriateLevel = player.CurrentLevel >= requiredLevel;
            if (learned || !previousRanksLearnt || !appropriateLevel) return;
            int cost = SpellTrainingCost(spellData.GetRequiredLevelForRank(rank));
            if (player.Gold < cost) return;

            player.ChangeGold(-cost);
            player.SpellBook.LearnSpell(Spell.BuildSpellKey(spellName, rank));
            PublishTrainerWindow(player, trainer.Name);
        }

        static bool TryGetTrainingContext(out Player player, out Npc trainer)
        {
            ThreadAffinity.AssertSimThread();
            player = ObjectManager.Player;
            trainer = null;
            if (player == null) return false;
            trainer = player.Target as Npc;
            if (trainer == null) return false;
            if (!trainer.InConversationRange(player.FeetPosition)) return false;
            return true;
        }

        static void PublishTrainerWindow(Player player, string trainerName)
        {
            ThreadAffinity.AssertSimThread();
            MailboxManager.PublishUiEvent(new SpellTrainingOpened(BuildEntries(player), trainerName));
        }

        static SpellTrainingEntrySnapshot[] BuildEntries(Player player)
        {
            ThreadAffinity.AssertSimThread();
            if (player?.ClassData == null) return Array.Empty<SpellTrainingEntrySnapshot>();

            HashSet<string> spellNames = new HashSet<string>(StringComparer.Ordinal);
            string[] levelOneSpells = player.ClassData.LevelOneSpells ?? Array.Empty<string>();
            string[] learnableSpells = player.ClassData.LearnableSpells ?? Array.Empty<string>();
            for (int i = 0; i < levelOneSpells.Length; i++) spellNames.Add(levelOneSpells[i]);
            for (int i = 0; i < learnableSpells.Length; i++) spellNames.Add(learnableSpells[i]);

            List<SpellTrainingEntrySnapshot> entries = new List<SpellTrainingEntrySnapshot>();
            foreach (string spellName in spellNames)
            {
                SpellData spellData = SpellFactory.GetSpell(spellName);
                int highestKnownRank = player.SpellBook.GetKnownRank(spellName);
                for (int rank = 1; rank <= spellData.MaxRank; rank++)
                {
                    bool learned = highestKnownRank >= rank;
                    bool previousRanksLearnt = rank <= 1 || highestKnownRank >= rank - 1;
                    int requiredLevel = spellData.GetRequiredLevelForRank(rank);
                    bool learnable = !learned && previousRanksLearnt && player.CurrentLevel >= requiredLevel;
                    entries.Add(new SpellTrainingEntrySnapshot(
                        Spell.BuildSpellKey(spellName, rank),
                        BuildDisplayName(spellName, rank, spellData.MaxRank),
                        requiredLevel,
                        SpellTrainingCost(requiredLevel),
                        learned,
                        player.CurrentLevel >= requiredLevel,
                        previousRanksLearnt,
                        new Spell(player, spellName, rank).GfxPath,
                        BuildDescriptor(player, spellData, rank)));
                }
            }

            return entries.ToArray();
        }

        static int SpellTrainingCost(int aLevel)
        {
            if (aLevel <= 10) return (int)Math.Round(0.065 * Math.Pow(aLevel, 2d));
            if (aLevel <= 16) return (int)Math.Round(1.1 * aLevel - 2.5);
            if (aLevel <= 24) return (int)Math.Round(13 + 3.4 * (aLevel - 16));
            return ((int)Math.Round(0.082 * Math.Pow(aLevel, 2) + 4.26 * aLevel - 116.8) / 10) * 10;
        }

        static string BuildDisplayName(string spellName, int rank, int maxRank)
        {
            if (maxRank <= 1) return spellName;
            return $"{spellName} (Rank {rank})";
        }

        static SpellDescriptorSnapshot BuildDescriptor(Entity e, SpellData spellData, int rank)
        {
            return SpellDescriptorSnapshot.FromSpell(new Spell(e, spellData.Name, rank));
        }
    }
}
