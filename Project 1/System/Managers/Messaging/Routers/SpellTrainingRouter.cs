using Project_1.GameObjects;
using Project_1.GameObjects.Entities.Friendlies.Npcs;
using Project_1.GameObjects.Entities.Friendlies.Players;
using Project_1.GameObjects.Spells;
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
        const int SpellTrainingCost = 10;
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
            if (player.Gold < SpellTrainingCost) return;

            player.ChangeGold(-SpellTrainingCost);
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
                        SpellTrainingCost,
                        learned,
                        learnable,
                        new Spell(spellName, rank).GfxPath,
                        BuildDescriptor(spellData, rank)));
                }
            }

            return entries.ToArray();
        }

        static string BuildDisplayName(string spellName, int rank, int maxRank)
        {
            if (maxRank <= 1) return spellName;
            return $"{spellName} (Rank {rank})";
        }

        static SpellDescriptorSnapshot BuildDescriptor(SpellData spellData, int rank)
        {
            Spell spell = new Spell(spellData.Name, rank);
            string description = string.IsNullOrWhiteSpace(spellData.Description)
                ? "No description."
                : spellData.Description;

            List<string> lines = new List<string>
            {
                $"Required Level: {spellData.GetRequiredLevelForRank(rank)}"
            };

            if (spell.CastTime > 0)
            {
                lines.Add($"Cast Time: {(spell.CastTime / 1000d).ToString("0.##", CultureInfo.InvariantCulture)} sec");
            }
            else
            {
                lines.Add("Cast Time: Instant");
            }

            if (spellData.GetCooldownForRank(rank) > 0)
            {
                lines.Add($"Cooldown: {(spellData.GetCooldownForRank(rank) / 1000d).ToString("0.##", CultureInfo.InvariantCulture)} sec");
            }

            lines.Add($"Cost: {spell.ResourceCost.ToString("0.##", CultureInfo.InvariantCulture)}");
            lines.Add($"Range: {spell.CastDistance.ToString("0.##", CultureInfo.InvariantCulture)}");

            return new SpellDescriptorSnapshot(
                BuildDisplayName(spellData.Name, rank, spellData.MaxRank),
                description,
                string.Join("\n", lines),
                true);
        }
    }
}
