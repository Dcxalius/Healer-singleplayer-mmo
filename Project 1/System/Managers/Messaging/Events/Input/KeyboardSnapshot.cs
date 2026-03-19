using Microsoft.Xna.Framework.Input;

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
}
