namespace Project_1.Messaging.Events
{
    internal readonly struct StateChangeRequested
    {
        public StateChangeRequested(StateKind state)
        {
            State = state;
        }

        public StateKind State { get; }
    }
}
