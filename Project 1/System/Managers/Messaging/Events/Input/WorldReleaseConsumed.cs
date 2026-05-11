using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.Input;

namespace Project_1.Messaging.Events
{
    internal readonly struct WorldReleaseConsumed
    {
        public WorldReleaseConsumed(RelativeScreenPosition relativePos, ClickKind button, byte modifiersMask)
        {
            RelativePos = relativePos;
            Button = button;
            ModifiersMask = modifiersMask;
        }

        public RelativeScreenPosition RelativePos { get; }
        public ClickKind Button { get; }
        public byte ModifiersMask { get; }

        public static WorldReleaseConsumed FromReleaseEvent(ReleaseEvent releaseEvent)
        {
            return new WorldReleaseConsumed(releaseEvent.RelativePos, releaseEvent.ButtonPressed.ToClickKind(), releaseEvent.ModifierMask);
        }
    }
}
