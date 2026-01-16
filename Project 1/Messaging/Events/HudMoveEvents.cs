using Project_1.UI.UIElements;

namespace Project_1.Messaging.Events
{
    internal readonly struct HudMovableChanged
    {
        public HudMovableChanged(bool enabled)
        {
            Enabled = enabled;
        }
        public bool Enabled { get; }
    }

    internal readonly struct HudSizeChangeRequested
    {
        public HudSizeChangeRequested(bool enabled)
        {
            Enabled = enabled;
        }
        public bool Enabled { get; }
    }

    internal readonly struct HudSizeChangerSet
    {
        public HudSizeChangerSet(UIElement element)
        {
            Element = element;
        }

        public UIElement Element { get; }
    }

    internal readonly struct HudSaveRequested
    {
    }
}
