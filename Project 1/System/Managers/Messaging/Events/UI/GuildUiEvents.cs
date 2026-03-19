using Project_1.Textures;

namespace Project_1.Messaging.Events
{
    internal enum InviteStatus
    {
        Pending,
        Accepted
    }

    internal readonly struct GuildInviteStatusUpdated
    {
        public GuildInviteStatusUpdated(string[] memberNames, InviteStatus[] statuses)
        {
            MemberNames = memberNames ?? System.Array.Empty<string>();
            Statuses = statuses ?? System.Array.Empty<InviteStatus>();
        }

        public string[] MemberNames { get; }
        public InviteStatus[] Statuses { get; }
    }

    internal readonly struct BuffAdded
    {
        public BuffAdded(int ownerRenderId, BuffUiSnapshot buff)
        {
            OwnerRenderId = ownerRenderId;
            Buff = buff;
        }

        public int OwnerRenderId { get; }
        public BuffUiSnapshot Buff { get; }
    }

    internal readonly struct GossipOpened
    {
        public GossipOpened(GossipUiSnapshot data)
        {
            Data = data;
        }

        public GossipUiSnapshot Data { get; }
    }

    internal readonly struct GossipUiSnapshot
    {
        public GossipUiSnapshot(string[][] options, int[][] linkTree, int startIndex, string speakerName)
        {
            Options = CloneJagged(options);
            LinkTree = CloneJagged(linkTree);
            StartIndex = startIndex;
            SpeakerName = speakerName;
        }

        public string[][] Options { get; }
        public int[][] LinkTree { get; }
        public int StartIndex { get; }
        public string SpeakerName { get; }

        static string[][] CloneJagged(string[][] source)
        {
            if (source == null) return null;
            string[][] clone = new string[source.Length][];
            for (int i = 0; i < source.Length; i++)
            {
                if (source[i] == null) continue;
                clone[i] = (string[])source[i].Clone();
            }

            return clone;
        }

        static int[][] CloneJagged(int[][] source)
        {
            if (source == null) return null;
            int[][] clone = new int[source.Length][];
            for (int i = 0; i < source.Length; i++)
            {
                if (source[i] == null) continue;
                clone[i] = (int[])source[i].Clone();
            }

            return clone;
        }
    }

    internal readonly struct GossipClosed
    {
    }
}
