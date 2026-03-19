namespace Project_1.Messaging.Events
{
    internal readonly struct CastChannelProgress
    {
        public CastChannelProgress(float progress01)
        {
            Progress01 = progress01;
        }

        public float Progress01 { get; }
    }
}
