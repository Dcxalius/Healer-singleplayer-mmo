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
        public HudSizeChangerSet(int uiElementId)
        {
            UiElementId = uiElementId;
        }

        public int UiElementId { get; }
    }

    internal readonly struct HudSaveRequested
    {
    }
}
