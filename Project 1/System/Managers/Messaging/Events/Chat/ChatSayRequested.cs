namespace Project_1.Messaging.Events
{
    internal readonly struct ChatSayRequested
    {
        public ChatSayRequested(string messageText)
        {
            MessageText = messageText;
        }

        public string MessageText { get; }
    }
}
