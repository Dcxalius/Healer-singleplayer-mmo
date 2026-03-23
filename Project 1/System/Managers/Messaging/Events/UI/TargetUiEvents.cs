namespace Project_1.Messaging.Events
{
    internal readonly struct TargetChanged
    {
        public TargetChanged(RelationToPlayerKind ownerRelation, EntityUiSnapshot? targetSnapshot)
        {
            OwnerRelation = ownerRelation;
            TargetSnapshot = targetSnapshot;
        }

        public RelationToPlayerKind OwnerRelation { get; }
        public EntityUiSnapshot? TargetSnapshot { get; }
    }

    internal readonly struct TargetRequested
    {
        public TargetRequested(int? targetRenderId)
        {
            TargetRenderId = targetRenderId;
        }

        public int? TargetRenderId { get; }
    }

    internal readonly struct PlateRefreshRequested
    {
        public PlateRefreshRequested(EntityUiSnapshot snapshot)
        {
            Snapshot = snapshot;
        }

        public EntityUiSnapshot Snapshot { get; }
    }

    internal readonly struct NamePlateAdded
    {
        public NamePlateAdded(EntityUiSnapshot snapshot)
        {
            Snapshot = snapshot;
        }

        public EntityUiSnapshot Snapshot { get; }
    }

    internal readonly struct NamePlateRemoved
    {
        public NamePlateRemoved(int renderId)
        {
            RenderId = renderId;
        }

        public int RenderId { get; }
    }

    internal readonly struct PlayerPlateSet
    {
        public PlayerPlateSet(EntityUiSnapshot snapshot)
        {
            Snapshot = snapshot;
        }

        public EntityUiSnapshot Snapshot { get; }
    }

    internal readonly struct PlateLayerCleared
    {
    }
}
