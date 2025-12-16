using Project_1.UI.UIElements.Boxes;

namespace Project_1.Messaging.Events
{
    internal readonly struct DialogueOpened
    {
        public DialogueOpened(DialogueBox box)
        {
            Box = box;
        }
        public DialogueBox Box { get; }
    }

    internal readonly struct DialogueClosed
    {
        public DialogueClosed(DialogueBox box)
        {
            Box = box;
        }
        public DialogueBox Box { get; }
    }
}
