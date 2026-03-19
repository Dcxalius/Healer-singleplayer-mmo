using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.Input;

namespace Project_1.Messaging.Events
{
    internal readonly struct WorldScrollRequested
    {
        public WorldScrollRequested(RelativeScreenPosition relativePos, int steps, bool up, byte modifiersMask)
        {
            RelativePos = relativePos;
            Steps = steps;
            Up = up;
            ModifiersMask = modifiersMask;
        }

        public RelativeScreenPosition RelativePos { get; }
        public int Steps { get; }
        public bool Up { get; }
        public byte ModifiersMask { get; }

        public bool Down => !Up;

        public static WorldScrollRequested FromScrollEvent(ScrollEvent scrollEvent)
        {
            return new WorldScrollRequested(scrollEvent.RelativePos, scrollEvent.Steps, scrollEvent.Up, scrollEvent.ModifierMask);
        }
    }
}
