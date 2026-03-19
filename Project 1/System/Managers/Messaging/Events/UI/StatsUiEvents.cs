namespace Project_1.Messaging.Events
{
    internal readonly struct StatsRefreshed
    {
        public StatsRefreshed(int ownerRenderId, RelationToPlayerKind ownerRelation, StatReportSnapshot primaryStats, StatReportSnapshot secondaryStats)
        {
            OwnerRenderId = ownerRenderId;
            OwnerRelation = ownerRelation;
            PrimaryStats = primaryStats;
            SecondaryStats = secondaryStats;
        }

        public int OwnerRenderId { get; }
        public RelationToPlayerKind OwnerRelation { get; }
        public StatReportSnapshot PrimaryStats { get; }
        public StatReportSnapshot SecondaryStats { get; }
    }

    internal readonly struct ExperienceRefreshed
    {
        public ExperienceRefreshed(int ownerRenderId, RelationToPlayerKind ownerRelation, int currentLevel, int currentExperience)
        {
            OwnerRenderId = ownerRenderId;
            OwnerRelation = ownerRelation;
            CurrentLevel = currentLevel;
            CurrentExperience = currentExperience;
        }

        public int OwnerRenderId { get; }
        public RelationToPlayerKind OwnerRelation { get; }
        public int CurrentLevel { get; }
        public int CurrentExperience { get; }
    }
}
