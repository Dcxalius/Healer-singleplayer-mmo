using Project_1.Managers.States;

namespace Project_1.Messaging.Events
{
    internal readonly struct StateChanged
    {
        public StateChanged(StateManager.States previous, StateManager.States current)
        {
            Previous = previous;
            Current = current;
        }

        public StateManager.States Previous { get; }
        public StateManager.States Current { get; }
    }

    internal readonly struct StateChangeRequested
    {
        public StateChangeRequested(StateManager.States state)
        {
            State = state;
        }

        public StateManager.States State { get; }
    }

    internal readonly struct ResetToMainMenuRequested
    {
    }

    internal readonly struct CreateNewPlayerRequested
    {
        public CreateNewPlayerRequested(string name, string className)
        {
            Name = name;
            ClassName = className;
        }

        public string Name { get; }
        public string ClassName { get; }
    }
}
