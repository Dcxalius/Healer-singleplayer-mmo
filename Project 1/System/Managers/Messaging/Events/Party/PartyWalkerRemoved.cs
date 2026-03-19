namespace Project_1.Messaging.Events
{
    internal readonly struct PartyWalkerRemoved
    {
        public PartyWalkerRemoved(int memberRenderId)
        {
            MemberRenderId = memberRenderId;
        }

        public int MemberRenderId { get; }
    }
}
