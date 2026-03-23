using Project_1.Textures;

namespace Project_1.Messaging.Events
{
    internal readonly struct SpellTrainingEntrySnapshot
    {
        public SpellTrainingEntrySnapshot(string spellKey, string displayName, int requiredLevel, int cost, bool learned, bool learnable, GfxPath gfxPath)
        {
            SpellKey = spellKey ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            RequiredLevel = requiredLevel;
            Cost = cost;
            Learned = learned;
            Learnable = learnable;
            GfxPath = gfxPath;
            Descriptor = default;
            HasDescriptor = false;
        }

        public SpellTrainingEntrySnapshot(string spellKey, string displayName, int requiredLevel, int cost, bool learned, bool learnable, GfxPath gfxPath, SpellDescriptorSnapshot descriptor)
        {
            SpellKey = spellKey ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            RequiredLevel = requiredLevel;
            Cost = cost;
            Learned = learned;
            Learnable = learnable;
            GfxPath = gfxPath;
            Descriptor = descriptor;
            HasDescriptor = true;
        }

        public string SpellKey { get; }
        public string DisplayName { get; }
        public int RequiredLevel { get; }
        public int Cost { get; }
        public bool Learned { get; }
        public bool Learnable { get; }
        public GfxPath GfxPath { get; }
        public SpellDescriptorSnapshot Descriptor { get; }
        public bool HasDescriptor { get; }
    }

    internal readonly struct SpellTrainingOpened
    {
        public SpellTrainingOpened(SpellTrainingEntrySnapshot[] entries, string trainerName)
        {
            Entries = entries ?? System.Array.Empty<SpellTrainingEntrySnapshot>();
            TrainerName = trainerName ?? string.Empty;
        }

        public SpellTrainingEntrySnapshot[] Entries { get; }
        public string TrainerName { get; }
    }

    internal readonly struct SpellTrainingClosed
    {
    }
}
