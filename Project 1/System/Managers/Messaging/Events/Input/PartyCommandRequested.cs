namespace Project_1.Messaging.Events
{
    internal readonly struct PartyCommandRequested
    {
        public PartyCommandRequested(PartyCommandAction action, int? memberRenderId)
        {
            Action = action;
            MemberRenderId = memberRenderId;
        }

        public PartyCommandAction Action { get; }
        public int? MemberRenderId { get; }
    }
}
