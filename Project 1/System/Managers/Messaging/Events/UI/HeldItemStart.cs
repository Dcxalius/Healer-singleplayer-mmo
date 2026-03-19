using Project_1.Camera;

namespace Project_1.Messaging.Events
{
    internal readonly struct HeldItemStart
    {
        public HeldItemStart(int sourceUiElementId, AbsoluteScreenPosition grabOffset)
        {
            SourceUiElementId = sourceUiElementId;
            GrabOffset = grabOffset;
        }

        public int SourceUiElementId { get; }
        public AbsoluteScreenPosition GrabOffset { get; }
    }
}
