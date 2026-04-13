using Newtonsoft.Json;
using Project_1.GameObjects.Spells;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.World.GameObjects.Unit.Talents
{
    internal class Talent
    {
        public bool HasChanges(Spell aSpell) => changes.Any(x => x.spellDataId == aSpell.SpellDataId);
        public GfxPath GfxPath => gfxPath;

        GfxPath gfxPath;

        public int Id => id;
        int id;
        public (int id, int amount)[] Required => required;
        (int id, int amount)[] required;
        public int MaxRank => maxRank;
        int maxRank;

        public double GetChanges(int id, int rank, TalentChange change, bool flat) => changes.Where(x => x.spellDataId == id).SelectMany(x => x.changes).Where(x => x.change == change && flat == x.flat).Sum(x => x.amount * rank);
        List<((TalentChange change, float amount, bool flat)[] changes, int spellDataId)> changes;


        public string Name => name;
        string name;
        string description;
        public string Description => description ??= GenerateDescription();

        [JsonConstructor]
        public Talent(int id, string name, string gfxName, int maxRank, (int id, int amount)[] required, List<((TalentChange change, float amount, bool flat)[] changes, int spellId)> changes)
        {
            //TODO: Should the gfxtype be spell image? Maybe talents should have their own gfx type?
            gfxPath = new GfxPath(GfxType.SpellImage, gfxName);

            this.name = name;
            this.id = id;
            this.maxRank = maxRank;
            this.required = required ?? Array.Empty<(int, int)>();
            this.changes = changes ?? new List<((TalentChange, float, bool)[], int)>();

            for (int i = 0; i < this.changes.Count; i++)
            {
                for (int j = 0; j < this.changes.Count; j++)
                {
                    if (i == j) continue;
                    Debug.Assert(this.changes[i].spellDataId != this.changes[j].spellDataId);
                }

                for (int k = 0; k < this.changes[i].changes.Length; k++)
                {
                    for (int l = 0; l < this.changes[i].changes.Length; l++)
                    {
                        if (k == l) continue;
                        Debug.Assert(this.changes[i].changes[k].change != this.changes[i].changes[l].change || this.changes[i].changes[k].flat != this.changes[i].changes[l].flat);
                    }
                }
            }
        }

        string GenerateDescription()
        {
            List<string> sections = new List<string>();

            if (required.Length > 0)
            {
                sections.Add(FormatRequirements());
            }

            if (changes.Count > 0)
            {
                List<string> clauses = BuildChangeClauses();
                if (clauses.Count > 0)
                {
                    sections.Add(CapitalizeFirst(JoinWithAnd(clauses)) + ".");
                }
            }

            return string.Join("\n\n", sections);
        }

        List<string> BuildChangeClauses()
        {
            Dictionary<(TalentChange change, bool flat), List<(string spellName, float amount)>> groupedChanges = new Dictionary<(TalentChange, bool), List<(string, float)>>();
            List<(TalentChange change, bool flat)> order = new List<(TalentChange, bool)>();

            for (int i = 0; i < changes.Count; i++)
            {
                string spellName = SpellFactory.GetSpell(changes[i].spellDataId).Name;
                for (int j = 0; j < changes[i].changes.Length; j++)
                {
                    (TalentChange change, bool flat) key = (changes[i].changes[j].change, changes[i].changes[j].flat);
                    if (!groupedChanges.TryGetValue(key, out List<(string spellName, float amount)> entries))
                    {
                        entries = new List<(string spellName, float amount)>();
                        groupedChanges.Add(key, entries);
                        order.Add(key);
                    }

                    entries.Add((spellName, changes[i].changes[j].amount));
                }
            }

            return order.Select(key => FormatChangeClause(key.change, key.flat, groupedChanges[key])).ToList();
        }

        string FormatRequirements()
        {
            return $"Requires {JoinWithAnd(required.Select(FormatRequirementTarget))}.";
        }

        static string FormatRequirementTarget((int id, int amount) requirement)
        {
            string pointText = requirement.amount == 1 ? "point" : "points";
            return $"{requirement.amount.ToString(CultureInfo.InvariantCulture)} {pointText} in {TalentFactory.GetTalent(requirement.id).Name}";
        }

        static string FormatChangeClause(TalentChange change, bool flat, List<(string spellName, float amount)> entries)
        {
            if (entries.Count == 0) return string.Empty;

            if (entries.All(x => SameAmount(x.amount, entries[0].amount)))
            {
                return $"{GetChangePrefix(change)} {JoinWithAnd(entries.Select(x => x.spellName))} by {FormatAmount(change, entries[0].amount, flat)}";
            }

            return $"{GetChangePrefix(change)} {JoinWithAnd(entries.Select(x => $"{x.spellName} by {FormatAmount(change, x.amount, flat)}"))}";
        }

        static string GetChangePrefix(TalentChange change)
        {
            return change switch
            {
                TalentChange.Amount => "increase the effect of",
                TalentChange.Duration => "increase the duration of",
                TalentChange.Range => "increase the range of",
                TalentChange.Radius => "increase the area of effect of",
                TalentChange.CastSpeed => "reduce the casting time of",
                TalentChange.Cooldown => "reduce the cooldown of",
                TalentChange.Cost => "reduce the cost of",
                _ => "change"
            };
        }

        static string FormatAmount(TalentChange change, float amount, bool flat)
        {
            float magnitude = Math.Abs(amount);
            if (!flat)
            {
                return $"{(magnitude * 100f).ToString("0.##", CultureInfo.InvariantCulture)}%";
            }

            return change switch
            {
                TalentChange.CastSpeed or TalentChange.Duration or TalentChange.Cooldown
                    => $"{magnitude.ToString("0.##", CultureInfo.InvariantCulture)} seconds",
                _ => magnitude.ToString("0.##", CultureInfo.InvariantCulture)
            };
        }

        static bool SameAmount(float left, float right)
        {
            return Math.Abs(left - right) < 0.0001f;
        }

        static string JoinWithAnd(IEnumerable<string> values)
        {
            string[] items = values.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            if (items.Length == 0) return string.Empty;
            if (items.Length == 1) return items[0];
            if (items.Length == 2) return $"{items[0]} and {items[1]}";

            return $"{string.Join(", ", items.Take(items.Length - 1))}, and {items[^1]}";
        }

        static string CapitalizeFirst(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return char.ToUpperInvariant(value[0]) + value[1..];
        }
    }

    public enum TalentChange
    {
        CastSpeed,
        Amount,
        Duration,
        Range,
        Radius,
        Cooldown,
        Cost
    }
}
