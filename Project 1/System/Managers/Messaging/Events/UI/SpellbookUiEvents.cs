namespace Project_1.Messaging.Events
{
    internal readonly struct SpellbookRefreshed
    {
        public SpellbookRefreshed(int ownerRenderId, string[] spellNames)
        {
            OwnerRenderId = ownerRenderId;
            SpellNames = spellNames;
        }

        public int OwnerRenderId { get; }
        public string[] SpellNames { get; }
    }

    internal readonly struct SpellbarLoaded
    {
        public SpellbarLoaded(int ownerRenderId, string[] spellNames)
        {
            OwnerRenderId = ownerRenderId;
            SpellNames = spellNames;
        }

        public int OwnerRenderId { get; }
        public string[] SpellNames { get; }
    }

    internal readonly struct SpellbarSnapshotRequested
    {
        public static readonly SpellbarSnapshotRequested Instance = new SpellbarSnapshotRequested();
    }

    internal readonly struct SpellbarSnapshotUpdated
    {
        public SpellbarSnapshotUpdated(string[] spells)
        {
            Spells = spells;
        }

        public string[] Spells { get; }
    }
}
