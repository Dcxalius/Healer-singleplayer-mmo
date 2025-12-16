using Project_1.GameObjects.Entities.Players;
using Project_1.UI.UIElements.Buttons;
using System.Collections.Generic;

namespace Project_1.Messaging.Events
{
    internal readonly struct PartyControlCleared
    {
        public PartyControlCleared(IList<GuildMember> members)
        {
            Members = members;
        }
        public IList<GuildMember> Members { get; }
    }

    internal readonly struct PartyWalkerAdded
    {
        public PartyWalkerAdded(GuildMember member)
        {
            Member = member;
        }
        public GuildMember Member { get; }
    }

    internal readonly struct PartyWalkerRemoved
    {
        public PartyWalkerRemoved(GuildMember member)
        {
            Member = member;
        }
        public GuildMember Member { get; }
    }

    internal readonly struct PartyMemberAdded
    {
        public PartyMemberAdded(GuildMember member)
        {
            Member = member;
        }
        public GuildMember Member { get; }
    }

    internal readonly struct PartyMemberRemoved
    {
        public PartyMemberRemoved(GuildMember member)
        {
            Member = member;
        }
        public GuildMember Member { get; }
    }

    internal readonly struct GuildMembersSet
    {
        public GuildMembersSet(GuildMember[] members)
        {
            Members = members;
        }
        public GuildMember[] Members { get; }
    }

    internal readonly struct GuildMemberAdded
    {
        public GuildMemberAdded(GuildMember member)
        {
            Member = member;
        }
        public GuildMember Member { get; }
    }

    internal readonly struct PartyCleared
    {
    }
}
