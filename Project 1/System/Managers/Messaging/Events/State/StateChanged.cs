namespace Project_1.Messaging.Events
{
    internal readonly struct StateChanged
    {
        public StateChanged(StateKind previous, StateKind current)
        {
            Previous = previous;
            Current = current;
        }

        public StateKind Previous { get; }
        public StateKind Current { get; }
    }
}
