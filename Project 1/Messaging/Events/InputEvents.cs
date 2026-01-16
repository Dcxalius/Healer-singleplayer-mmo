using Microsoft.Xna.Framework.Input;
using Project_1.Camera;

namespace Project_1.Messaging.Events
{
    internal readonly struct KeyboardSnapshot
    {
        public KeyboardSnapshot(Keys[] downKeys)
        {
            DownKeys = downKeys;
        }

        public Keys[] DownKeys { get; }
    }

    internal readonly struct KeyBindSnapshot
    {
        public KeyBindSnapshot(bool[] pressed, bool[] held, bool[] released)
        {
            Pressed = pressed;
            Held = held;
            Released = released;
        }

        public bool[] Pressed { get; }
        public bool[] Held { get; }
        public bool[] Released { get; }
    }

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
