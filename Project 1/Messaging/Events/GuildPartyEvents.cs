using System;

namespace Project_1.Messaging.Events
{
    internal readonly struct PartyControlCleared
    {
        public PartyControlCleared(int[] memberRenderIds)
        {
            MemberRenderIds = memberRenderIds ?? Array.Empty<int>();
        }
        public int[] MemberRenderIds { get; }
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
