using Microsoft.Xna.Framework;

namespace Project_1.Messaging.Events
{
    internal readonly struct HudRescaleRequested
    {
        public HudRescaleRequested(Point windowSize)
        {
            WindowSize = windowSize;
        }

        public Point WindowSize { get; }
    }
}
