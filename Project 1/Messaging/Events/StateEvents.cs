using Project_1.Managers.Saves;
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

    internal readonly struct StateChangeRequested
    {
        public StateChangeRequested(StateKind state)
        {
            State = state;
        }

        public StateKind State { get; }
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
        public SaveLoadParsed(SaveLoadPayload payload, int requestId)
        {
            Payload = payload;
            RequestId = requestId;
        }

        public SaveLoadPayload Payload { get; }
        public int RequestId { get; }
    }
}
