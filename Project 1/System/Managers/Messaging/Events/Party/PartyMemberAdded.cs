namespace Project_1.Messaging.Events
{
    internal readonly struct PartyMemberAdded
    {
        public PartyMemberAdded(EntityUiSnapshot member)
        {
            Member = member;
        }

        public EntityUiSnapshot Member { get; }
    }
}
