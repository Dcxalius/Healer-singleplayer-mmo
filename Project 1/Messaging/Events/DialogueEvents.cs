using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Textures;
using Project_1.UI.UIElements.Boxes;

namespace Project_1.Messaging.Events
{
    internal readonly struct DialogueOpened
    {
        public DialogueOpened(string text, Color textColor, DialogueBox.LocationOfPopUp location, DialogueBox.PausesGame pauses, string title, GfxPath background, RelativeScreenPosition pos, RelativeScreenPosition size, string closeText)
        {
            Text = text;
            TextColor = textColor;
            Location = location;
            Pauses = pauses;
            Title = title;
            Background = background;
            Pos = pos;
            Size = size;
            CloseText = closeText;
        }

        public string Text { get; }
        public Color TextColor { get; }
        public DialogueBox.LocationOfPopUp Location { get; }
        public DialogueBox.PausesGame Pauses { get; }
        public string Title { get; }
        public GfxPath Background { get; }
        public RelativeScreenPosition Pos { get; }
        public RelativeScreenPosition Size { get; }
        public string CloseText { get; }
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
