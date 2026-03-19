namespace Project_1.Messaging.Events
{
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
}
