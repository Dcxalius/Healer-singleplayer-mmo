namespace Project_1.Messaging.Events
{
    internal readonly struct LogicWindowSnapshotRequested
    {
        public LogicWindowSnapshotRequested(int memberRenderId)
        {
            MemberRenderId = memberRenderId;
        }

        public int MemberRenderId { get; }
    }

    internal readonly struct LogicNodeUiSnapshot
    {
        public LogicNodeUiSnapshot(int nodeId, int parentNodeId, int depth, string resultName)
        {
            NodeId = nodeId;
            ParentNodeId = parentNodeId;
            Depth = depth;
            ResultName = resultName;
        }

        public int NodeId { get; }
        public int ParentNodeId { get; }
        public int Depth { get; }
        public string ResultName { get; }
    }

    internal readonly struct LogicWindowSnapshotSet
    {
        public LogicWindowSnapshotSet(int memberRenderId, LogicNodeUiSnapshot[] nodes)
        {
            MemberRenderId = memberRenderId;
            Nodes = nodes ?? System.Array.Empty<LogicNodeUiSnapshot>();
        }

        public int MemberRenderId { get; }
        public LogicNodeUiSnapshot[] Nodes { get; }
    }

    internal readonly struct LogicWindowOpened
    {
        public LogicWindowOpened(int memberRenderId)
        {
            MemberRenderId = memberRenderId;
        }

        public int MemberRenderId { get; }
    }

    internal readonly struct InspectWindowToggled
    {
        public InspectWindowToggled(EntityUiSnapshot member)
        {
            Member = member;
        }
        public EntityUiSnapshot Member { get; }
    }

    internal readonly struct CharacterWindowToggled
    {
    }
}
