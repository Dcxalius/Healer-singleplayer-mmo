namespace Project_1.Messaging.Events
{
    internal readonly struct PartyTargetOrderRequested
    {
        public PartyTargetOrderRequested(int targetRenderId)
        {
            TargetRenderId = targetRenderId;
        }

        public int TargetRenderId { get; }
    }
}
