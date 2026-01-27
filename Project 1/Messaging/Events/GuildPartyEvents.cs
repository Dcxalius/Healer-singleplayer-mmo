using GuildMemberEntity = Project_1.GameObjects.Entities.Friendlies.GuildMembers.GuildMember;
using Project_1.UI.UIElements.Buttons;
using System.Collections.Generic;
using Project_1.GameObjects.Entities.Friendlies;
using Project_1.GameObjects.Entities.Friendlies.GuildMembers;

namespace Project_1.Messaging.Events
{
    internal readonly struct PartyControlCleared
    {
        public PartyControlCleared(IList<GuildMemberEntity> members)
        {
            Members = members;
        }
        public IList<GuildMemberEntity> Members { get; }
    }

    internal readonly struct PartyWalkerAdded
    {
        public PartyWalkerAdded(GuildMemberEntity member)
        {
            Member = member;
        }
        public GuildMemberEntity Member { get; }
    }

    internal readonly struct PartyWalkerRemoved
    {
        public PartyWalkerRemoved(GuildMemberEntity member)
        {
            Member = member;
        }
        public GuildMemberEntity Member { get; }
    }

    internal readonly struct PartyMemberAdded
    {
        public PartyMemberAdded(GuildMemberEntity member)
        {
            Member = member;
        }
        public GuildMemberEntity Member { get; }
    }

    internal readonly struct PartyMemberRemoved
    {
        public PartyMemberRemoved(GuildMemberEntity member)
        {
            Member = member;
        }
        public GuildMemberEntity Member { get; }
    }

    internal readonly struct PartyMemberInviteRequested
    {
        public PartyMemberInviteRequested(GuildMemberEntity member)
        {
            Member = member;
        }
        public GuildMemberEntity Member { get; }
    }

    internal readonly struct PartyMemberKickRequested
    {
        public PartyMemberKickRequested(GuildMemberEntity member)
        {
            Member = member;
        }
        public GuildMemberEntity Member { get; }
    }

    internal readonly struct GuildMembersSet
    {
        public GuildMembersSet(Friendly[] members)
        {
            Members = members;
        }
        public Friendly[] Members { get; }
    }

    internal readonly struct GuildMemberAdded
    {
        public GuildMemberAdded(GuildMemberEntity member)
        {
            Member = member;
        }
        public GuildMemberEntity Member { get; }
    }

    internal readonly struct PartyCleared
    {
    }
}
