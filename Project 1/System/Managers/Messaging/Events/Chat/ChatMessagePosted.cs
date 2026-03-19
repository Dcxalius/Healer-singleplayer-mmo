namespace Project_1.Messaging.Events
{
    internal readonly struct ChatMessagePosted
    {
        public ChatMessagePosted(ChatMessage message)
        {
            ChatMessage = message;
        }

        public ChatMessagePosted(ChatMessageType type, string message)
        {
            ChatMessage = new ChatMessage(type, message);
        }

        public ChatMessage ChatMessage { get; }
        public ChatMessageType Type => ChatMessage.Type;
        public string Message => ChatMessage.Content;
    }
}
