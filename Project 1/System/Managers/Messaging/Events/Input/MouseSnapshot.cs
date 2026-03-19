using Project_1.Camera;

namespace Project_1.Messaging.Events
{
    internal readonly struct MouseSnapshot
    {
        public MouseSnapshot(AbsoluteScreenPosition absolute, RelativeScreenPosition relative, int scrollWheelValue, int scrollDelta)
        {
            Absolute = absolute;
            Relative = relative;
            ScrollWheelValue = scrollWheelValue;
            ScrollDelta = scrollDelta;
        }

        public AbsoluteScreenPosition Absolute { get; }
        public RelativeScreenPosition Relative { get; }
        public int ScrollWheelValue { get; }
        public int ScrollDelta { get; }
    }
}
