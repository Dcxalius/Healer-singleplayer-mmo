namespace Project_1.Messaging.Events
{
    internal readonly struct HudSizeChangeRequested
    {
        public HudSizeChangeRequested(bool enabled)
        {
            Enabled = enabled;
        }

        public bool Enabled { get; }
    }
}
