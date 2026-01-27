using Project_1.Camera;
using Project_1.Managers;
using Project_1.Messaging.Events;

namespace Project_1.Input
{
    /// <summary>
    /// Sim-thread mouse state derived from UI-routed snapshots.
    /// </summary>
    internal static class MouseStateCache
    {
        public static AbsoluteScreenPosition Absolute { get; private set; }
        public static RelativeScreenPosition Relative { get; private set; }
        public static int ScrollWheelValue { get; private set; }
        public static int ScrollDelta { get; private set; }

        public static void Update(MouseSnapshot snapshot)
        {
            ThreadAffinity.AssertSimThread();
            Absolute = snapshot.Absolute;
            Relative = snapshot.Relative;
            ScrollWheelValue = snapshot.ScrollWheelValue;
            ScrollDelta = snapshot.ScrollDelta;
        }
    }
}
