namespace Project_1.Messaging.Events
{
    internal readonly struct DialogueClosed
    {
        public DialogueClosed(int dialogueBoxId)
        {
            DialogueBoxId = dialogueBoxId;
        }

        public int DialogueBoxId { get; }
    }
}
