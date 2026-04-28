namespace Project_1.Messaging.Events
{
    internal readonly struct SaveDataStarted
    {
        public SaveDataStarted(string message)
        {
            Message = string.IsNullOrWhiteSpace(message) ? "Saving..." : message;
        }

        public string Message { get; }
    }

    internal readonly struct SaveDataFinished
    {
        public SaveDataFinished(string message)
        {
            Message = string.IsNullOrWhiteSpace(message) ? "Saved" : message;
        }

        public string Message { get; }
    }

    internal readonly struct SaveDataFailed
    {
        public SaveDataFailed(string message)
        {
            Message = string.IsNullOrWhiteSpace(message) ? "Save failed" : message;
        }

        public string Message { get; }
    }
}
