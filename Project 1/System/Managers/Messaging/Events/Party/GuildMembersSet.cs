namespace Project_1.Messaging.Events
{
    internal readonly struct GuildMembersSet
    {
        public GuildMembersSet(EntityUiSnapshot[] members)
        {
            Members = members;
        }

        public EntityUiSnapshot[] Members { get; }
    }
}
