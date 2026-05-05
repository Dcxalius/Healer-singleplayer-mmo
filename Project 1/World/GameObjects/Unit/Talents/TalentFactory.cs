using Newtonsoft.Json;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Project_1.World.GameObjects.Unit.Talents
{
    internal static class TalentFactory
    {
        static Talent[] talents;
        static TalentTree[] talentTrees;
        static bool initialized;

        // ── JSON data classes ─────────────────────────────────────────────

        class ChangeEntry
        {
            public TalentChange Change { get; set; }
            public float Amount { get; set; }
            public bool Flat { get; set; }
        }

        class SpellChangeEntry
        {
            public int SpellDataId { get; set; }
            public ChangeEntry[] Changes { get; set; }
        }

        class StatChangeEntry
        {
            public string Stat { get; set; }
            public float Amount { get; set; }
            public bool Flat { get; set; }
        }

        class RequiredEntry
        {
            public int Id { get; set; }
            public int Amount { get; set; }
        }

        class TalentData
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string GfxName { get; set; }
            public int MaxRank { get; set; }
            public RequiredEntry[] Required { get; set; }
            public SpellChangeEntry[] SpellChanges { get; set; }
            public StatChangeEntry[] StatChanges { get; set; }
        }

        class TalentTreeData
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string GfxName { get; set; }
            public int[][] Rows { get; set; }
        }

        // ── Init ──────────────────────────────────────────────────────────

        public static void Init()
        {
            ThreadAffinity.AssertMainThread();
            if (initialized) return;
            initialized = true;

            LoadTalents();
            LoadTalentTrees();
        }

        static void LoadTalents()
        {
            string path = Game1.ContentManager.RootDirectory + "\\Data\\Class\\Talents\\";
            string[] files = Directory.GetFiles(path);

            List<Talent> list = new List<Talent>();
            foreach (string file in files)
            {
                string raw = File.ReadAllText(file);
                if (string.IsNullOrWhiteSpace(raw)) continue;
                TalentData data = JsonConvert.DeserializeObject<TalentData>(raw);
                if (data == null) continue;
                list.Add(BuildTalent(data));
            }

            list.Sort((a, b) => a.Id.CompareTo(b.Id));
            talents = list.ToArray();
        }

        static void LoadTalentTrees()
        {
            string path = Game1.ContentManager.RootDirectory + "\\Data\\Class\\Talents\\TalentTrees\\";
            string[] files = Directory.GetFiles(path);

            List<TalentTree> list = new List<TalentTree>();
            foreach (string file in files)
            {
                string raw = File.ReadAllText(file);
                if (string.IsNullOrWhiteSpace(raw)) continue;
                TalentTreeData[] treeArray = JsonConvert.DeserializeObject<TalentTreeData[]>(raw);
                if (treeArray == null) continue;
                foreach (TalentTreeData data in treeArray)
                    list.Add(BuildTalentTree(data));
            }

            list.Sort((a, b) => a.Id.CompareTo(b.Id));
            talentTrees = list.ToArray();
        }

        // ── Builders ──────────────────────────────────────────────────────

        static Talent BuildTalent(TalentData data)
        {
            (int id, int amount)[] required = data.Required?
                .Select(r => (r.Id, r.Amount))
                .ToArray()
                ?? Array.Empty<(int, int)>();

            List<((TalentChange change, float amount, bool flat)[] changes, int spellDataId)> changes = data.SpellChanges?
                .Select(sc => (
                    sc.Changes.Select(c => (c.Change, c.Amount, c.Flat)).ToArray(),
                    sc.SpellDataId
                ))
                .ToList()
                ?? new List<((TalentChange, float, bool)[], int)>();

            List<(string stat, float amount, bool flat)> statChanges = data.StatChanges?
                .Select(sc => (sc.Stat, sc.Amount, sc.Flat))
                .ToList()
                ?? new List<(string, float, bool)>();

            return new Talent(data.Id, data.Name, data.GfxName, data.MaxRank, required, changes, statChanges);
        }

        static TalentTree BuildTalentTree(TalentTreeData data)
        {
            Talent[][] rows = data.Rows
                .Select(row => row.Select(id => GetTalent(id)).ToArray())
                .ToArray();

            return new TalentTree(data.Id, rows, data.Name, data.GfxName);
        }

        // ── Queries ───────────────────────────────────────────────────────

        public static Talent GetTalent(int id)
        {
            return talents.Single(t => t.Id == id);
        }

        public static TalentTree GetTalentTree(int id)
        {
            return talentTrees.Single(t => t.Id == id);
        }
    }
}
