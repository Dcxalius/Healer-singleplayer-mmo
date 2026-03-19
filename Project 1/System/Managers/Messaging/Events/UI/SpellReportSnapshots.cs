using System;

namespace Project_1.Messaging.Events
{
    internal readonly struct SpellSchoolBonusSnapshot
    {
        public SpellSchoolBonusSnapshot(string schoolName, double value)
        {
            SchoolName = schoolName;
            Value = value;
        }

        public string SchoolName { get; }
        public double Value { get; }
    }

    internal readonly struct SpellReportDetailsSnapshot
    {
        public static SpellReportDetailsSnapshot Empty => new SpellReportDetailsSnapshot(
            Array.Empty<SpellSchoolBonusSnapshot>(),
            Array.Empty<SpellSchoolBonusSnapshot>(),
            Array.Empty<SpellSchoolBonusSnapshot>());

        public SpellReportDetailsSnapshot(
            SpellSchoolBonusSnapshot[] damageBonuses,
            SpellSchoolBonusSnapshot[] critChanceBonuses,
            SpellSchoolBonusSnapshot[] hitChanceBonuses)
        {
            DamageBonuses = damageBonuses ?? Array.Empty<SpellSchoolBonusSnapshot>();
            CritChanceBonuses = critChanceBonuses ?? Array.Empty<SpellSchoolBonusSnapshot>();
            HitChanceBonuses = hitChanceBonuses ?? Array.Empty<SpellSchoolBonusSnapshot>();
        }

        public SpellSchoolBonusSnapshot[] DamageBonuses { get; }
        public SpellSchoolBonusSnapshot[] CritChanceBonuses { get; }
        public SpellSchoolBonusSnapshot[] HitChanceBonuses { get; }
    }
}
