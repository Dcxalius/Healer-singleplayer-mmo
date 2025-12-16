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
}
