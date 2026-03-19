namespace Project_1.Messaging.Events
{
    internal readonly struct PartyWalkerAdded
    {
        public PartyWalkerAdded(int memberRenderId)
        {
            MemberRenderId = memberRenderId;
        }

        public int MemberRenderId { get; }
    }
}
