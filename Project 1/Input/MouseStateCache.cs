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
        static AbsoluteScreenPosition absolute;
        static RelativeScreenPosition relative;
        static int scrollWheelValue;
        static int scrollDelta;

        public static AbsoluteScreenPosition Absolute
        {
            get
            {
                ThreadAffinity.AssertSimThread();
                return absolute;
            }
        }

        public static RelativeScreenPosition Relative
        {
            get
            {
                ThreadAffinity.AssertSimThread();
                return relative;
            }
        }

        public static int ScrollWheelValue
        {
            get
            {
                ThreadAffinity.AssertSimThread();
                return scrollWheelValue;
            }
        }

        public static int ScrollDelta
        {
            get
            {
                ThreadAffinity.AssertSimThread();
                return scrollDelta;
            }
        }

        public static void Update(MouseSnapshot snapshot)
        {
            ThreadAffinity.AssertSimThread();
            absolute = snapshot.Absolute;
            relative = snapshot.Relative;
            scrollWheelValue = snapshot.ScrollWheelValue;
            scrollDelta = snapshot.ScrollDelta;
        }
    }
}
