using Project_1.GameObjects.Entities.Friendlies.GuildMembers;

namespace Project_1.Messaging.Events
{
    internal readonly struct InspectWindowToggled
    {
        public InspectWindowToggled(GuildMember member)
        {
            Member = member;
        }
        public GuildMember Member { get; }
    }

    internal readonly struct CharacterWindowToggled
    {
    }
}
