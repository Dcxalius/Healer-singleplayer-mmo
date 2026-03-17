using System;

namespace Project_1.Messaging.Events
{
    internal readonly struct PartyControlCleared
    {
        public const int MaxMembers = 4;

        public PartyControlCleared(int memberCount, int memberRenderId0, int memberRenderId1, int memberRenderId2, int memberRenderId3)
        {
            if (memberCount < 0 || memberCount > MaxMembers)
            {
                throw new ArgumentOutOfRangeException(nameof(memberCount), $"PartyControlCleared supports 0..{MaxMembers} members.");
            }

            MemberCount = memberCount;
            MemberRenderId0 = memberRenderId0;
            MemberRenderId1 = memberRenderId1;
            MemberRenderId2 = memberRenderId2;
            MemberRenderId3 = memberRenderId3;
        }

        public int MemberCount { get; }
        public int MemberRenderId0 { get; }
        public int MemberRenderId1 { get; }
        public int MemberRenderId2 { get; }
        public int MemberRenderId3 { get; }

        public int GetMemberRenderId(int index)
        {
            return index switch
            {
                0 => MemberRenderId0,
                1 => MemberRenderId1,
                2 => MemberRenderId2,
                3 => MemberRenderId3,
                _ => throw new ArgumentOutOfRangeException(nameof(index)),
            };
        }
    }

    internal readonly struct PartyWalkerAdded
    {
        public PartyWalkerAdded(int memberRenderId)
        {
            MemberRenderId = memberRenderId;
        }
        public int MemberRenderId { get; }
    }

    internal readonly struct PartyWalkerRemoved
    {
        public PartyWalkerRemoved(int memberRenderId)
        {
            MemberRenderId = memberRenderId;
        }
        public int MemberRenderId { get; }
    }

    internal readonly struct PartyMemberAdded
    {
        public PartyMemberAdded(EntityUiSnapshot member)
        {
            Member = member;
        }
        public EntityUiSnapshot Member { get; }
    }

    internal readonly struct PartyMemberRemoved
    {
        public PartyMemberRemoved(int memberRenderId)
        {
            MemberRenderId = memberRenderId;
        }
        public int MemberRenderId { get; }
    }

    internal readonly struct PartyMemberInviteRequested
    {
        public PartyMemberInviteRequested(int memberRenderId)
        {
            MemberRenderId = memberRenderId;
        }
        public int MemberRenderId { get; }
    }

    internal readonly struct PartyMemberKickRequested
    {
        public PartyMemberKickRequested(int memberRenderId)
        {
            MemberRenderId = memberRenderId;
        }
        public int MemberRenderId { get; }
    }

    internal readonly struct GuildMembersSet
    {
        public GuildMembersSet(EntityUiSnapshot[] members)
        {
            Members = members;
        }
        public EntityUiSnapshot[] Members { get; }
    }

    internal readonly struct GuildMemberAdded
    {
        public GuildMemberAdded(EntityUiSnapshot member)
        {
            Member = member;
        }
        public EntityUiSnapshot Member { get; }
    }

    internal readonly struct PartyCleared
    {
    }
}
