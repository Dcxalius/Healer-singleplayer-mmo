namespace Project_1.Messaging.Events
{
    internal readonly struct ChatCommandRequested
    {
        public ChatCommandRequested(string commandText)
        {
            CommandText = commandText;
        }

        public string CommandText { get; }
    }
}
