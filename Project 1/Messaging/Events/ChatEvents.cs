namespace Project_1.Messaging.Events
{
    internal enum ChatMessageType
    {
        Say,
        Loot,
        Experience,
        Guild,
        Party,
        System,
        Count
    }

    internal readonly struct ChatMessagePosted
    {
        public ChatMessagePosted(ChatMessageType type, string message)
        {
            Type = type;
            Message = message;
        }

        public ChatMessageType Type { get; }
        public string Message { get; }
    }

    internal readonly struct ChatCommandRequested
    {
        public ChatCommandRequested(string commandText)
        {
            CommandText = commandText;
        }

        public string CommandText { get; }
    }

    internal readonly struct ChatFiltersChanged
    {
    }

    internal readonly struct ChatCleared
    {
    }
}
