using Project_1.Managers.States;
using Project_1.Managers.Saves;
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

    internal readonly struct SaveDataRequested
    {
    }

    internal readonly struct LoadSaveRequested
    {
        public LoadSaveRequested(string saveName)
        {
            SaveName = saveName;
        }

        public string SaveName { get; }
    }

    internal readonly struct ContinueLastSaveRequested
    {
    }

    internal readonly struct NewGameRequested
    {
    }

    internal readonly struct SaveLoadParsed
    {
        public SaveLoadParsed(SaveLoadPayload payload)
        {
            Payload = payload;
        }

        public SaveLoadPayload Payload { get; }
    }
}
