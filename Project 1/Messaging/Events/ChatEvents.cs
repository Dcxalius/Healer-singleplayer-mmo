namespace Project_1.Messaging.Events
{
    internal enum ChatSpeakerType
    {
        None,
        Player,
        Npc,
        System
    }

    internal readonly struct ChatMessage
    {
        public ChatMessage(ChatMessageType type, string content, string senderName = null, ChatSpeakerType speakerType = ChatSpeakerType.None)
        {
            Type = type;
            Content = content ?? string.Empty;
            SenderName = senderName ?? string.Empty;
            SpeakerType = speakerType;
        }

        public ChatMessageType Type { get; }
        public string Content { get; }
        public string SenderName { get; }
        public ChatSpeakerType SpeakerType { get; }
        public bool HasSender => !string.IsNullOrWhiteSpace(SenderName);
        public string DisplayText => HasSender ? $"{SenderName}: {Content}" : Content;
    }

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

    internal readonly struct ChatCommandRequested
    {
        public ChatCommandRequested(string commandText)
        {
            CommandText = commandText;
        }

        public string CommandText { get; }
    }

    internal readonly struct ChatSayRequested
    {
        public ChatSayRequested(string messageText)
        {
            MessageText = messageText;
        }

        public string MessageText { get; }
    }

    internal readonly struct ChatFiltersChanged
    {
    }

    internal readonly struct ChatCleared
    {
    }
}
