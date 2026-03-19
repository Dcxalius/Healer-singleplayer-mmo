namespace Project_1.Messaging.Events
{
    internal readonly struct GuildMemberAdded
    {
        public GuildMemberAdded(EntityUiSnapshot member)
        {
            Member = member;
        }

        public EntityUiSnapshot Member { get; }
    }
}
