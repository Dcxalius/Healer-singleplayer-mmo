using System;
using Project_1.Textures;

namespace Project_1.Messaging.Events
{
    internal readonly struct TalentWindowSnapshotRequested
    {
        public TalentWindowSnapshotRequested(int? memberRenderId)
        {
            MemberRenderId = memberRenderId;
        }

        public int? MemberRenderId { get; }
    }

    internal readonly struct TalentWindowSet
    {
        public TalentWindowSet(TalentWindowSnapshot snapshot)
        {
            Snapshot = snapshot;
        }

        public TalentWindowSnapshot Snapshot { get; }
    }

    internal readonly struct TalentWindowToggled
    {
        public TalentWindowToggled(EntityUiSnapshot member)
        {
            Member = member;
        }

        public EntityUiSnapshot Member { get; }
    }

    internal readonly struct TalentWindowSnapshot
    {
        public TalentWindowSnapshot(EntityUiSnapshot ownerSnapshot, TalentTreeUiSnapshot[] trees)
        {
            OwnerSnapshot = ownerSnapshot;
            Trees = trees ?? Array.Empty<TalentTreeUiSnapshot>();
        }

        public EntityUiSnapshot OwnerSnapshot { get; }
        public TalentTreeUiSnapshot[] Trees { get; }
    }

    internal readonly struct TalentTreeUiSnapshot
    {
        public TalentTreeUiSnapshot(string name, GfxPath background, TalentUiSnapshot[][] rows)
        {
            Name = name ?? string.Empty;
            Background = background ?? GfxPath.NullPath;
            Rows = rows ?? Array.Empty<TalentUiSnapshot[]>();
        }

        public string Name { get; }
        public GfxPath Background { get; }
        public TalentUiSnapshot[][] Rows { get; }
    }

    internal readonly struct TalentUiSnapshot
    {
        public TalentUiSnapshot(int id, string name, string description, GfxPath gfxPath, int rank, int maxRank)
        {
            Id = id;
            Name = name ?? string.Empty;
            Description = description ?? string.Empty;
            GfxPath = gfxPath ?? GfxPath.NullPath;
            Rank = rank;
            MaxRank = maxRank;
        }

        public int Id { get; }
        public string Name { get; }
        public string Description { get; }
        public GfxPath GfxPath { get; }
        public int Rank { get; }
        public int MaxRank { get; }
    }
}
