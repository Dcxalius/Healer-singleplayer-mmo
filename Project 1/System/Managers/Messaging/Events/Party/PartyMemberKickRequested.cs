namespace Project_1.Messaging.Events
{
    internal readonly struct PartyMemberKickRequested
    {
        public PartyMemberKickRequested(int memberRenderId)
        {
            MemberRenderId = memberRenderId;
        }

        public int MemberRenderId { get; }
    }
}
