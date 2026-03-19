namespace Project_1.Messaging.Events
{
    internal readonly struct PartyMemberRemoved
    {
        public PartyMemberRemoved(int memberRenderId)
        {
            MemberRenderId = memberRenderId;
        }

        public int MemberRenderId { get; }
    }
}
