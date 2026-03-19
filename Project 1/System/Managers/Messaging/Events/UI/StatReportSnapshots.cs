using PairReport = Project_1.GameObjects.Unit.PairReport;
using System;

namespace Project_1.Messaging.Events
{
    internal readonly struct StatLineSnapshot
    {
        public StatLineSnapshot(string name, double value, StatLineCategory category = StatLineCategory.Unspecified)
        {
            Name = name;
            Value = value;
            Category = category;
        }

        public string Name { get; }
        public double Value { get; }
        public StatLineCategory Category { get; }
    }

    internal enum StatLineCategory
    {
        Unspecified = 0,
        Primary = 1,
        Attack = 2,
        Spell = 3,
        Defense = 4
    }

    internal static class StatLineCategoryResolver
    {
        public static StatLineCategory ResolveCharacter(string statName)
        {
            if (string.IsNullOrWhiteSpace(statName))
            {
                return StatLineCategory.Unspecified;
            }

            return statName switch
            {
                "Strength" or "Agility" or "Intellect" or "Spirit" or "Stamina" => StatLineCategory.Primary,
                "Armor" or "Dodge Chance" or "Parry Chance" or "Block Chance" or "Block Value" => StatLineCategory.Defense,
                _ when statName.StartsWith("Spell ", StringComparison.Ordinal) => StatLineCategory.Spell,
                _ => StatLineCategory.Attack
            };
        }
    }

    internal readonly struct StatReportSnapshot
    {
        public static StatReportSnapshot Empty => new StatReportSnapshot(Array.Empty<StatLineSnapshot>(), SpellReportDetailsSnapshot.Empty);

        public StatReportSnapshot(StatLineSnapshot[] lines)
            : this(lines, SpellReportDetailsSnapshot.Empty)
        {
        }

        public StatReportSnapshot(StatLineSnapshot[] lines, SpellReportDetailsSnapshot spellDetails)
        {
            Lines = lines ?? Array.Empty<StatLineSnapshot>();
            SpellDetails = spellDetails;
        }

        public StatLineSnapshot[] Lines { get; }
        public SpellReportDetailsSnapshot SpellDetails { get; }
        public int Count => Lines?.Length ?? 0;

        public static StatReportSnapshot FromPairReport(
            PairReport report,
            SpellReportDetailsSnapshot spellDetails = default,
            Func<string, StatLineCategory> categoryResolver = null)
        {
            if (report == null || report.Count == 0) return Empty;
            StatLineSnapshot[] lines = new StatLineSnapshot[report.Count];
            for (int i = 0; i < report.Count; i++)
            {
                var line = report.Lines[i];
                StatLineCategory category = categoryResolver?.Invoke(line.Name) ?? StatLineCategory.Unspecified;
                lines[i] = new StatLineSnapshot(line.Name, line.Value, category);
            }

            return new StatReportSnapshot(lines, spellDetails);
        }
    }
}
