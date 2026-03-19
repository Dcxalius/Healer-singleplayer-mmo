namespace Project_1.Messaging.Events
{
    internal readonly struct PartyMemberInviteRequested
    {
        public PartyMemberInviteRequested(int memberRenderId)
        {
            MemberRenderId = memberRenderId;
        }

        public int MemberRenderId { get; }
    }
}
