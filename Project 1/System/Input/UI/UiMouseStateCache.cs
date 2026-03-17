using Project_1.Camera;
using Project_1.Managers;
using Project_1.Messaging.Events;

namespace Project_1.Input
{
    /// <summary>
    /// UI-thread mouse state derived from UI mailbox snapshots.
    /// </summary>
    internal static class UiMouseStateCache
    {
        static AbsoluteScreenPosition absolute;
        static RelativeScreenPosition relative;
        static int scrollWheelValue;
        static int scrollDelta;

        public static AbsoluteScreenPosition Absolute
        {
            get
            {
                AssertUiOrMainThread();
                return absolute;
            }
        }

        public static RelativeScreenPosition Relative
        {
            get
            {
                AssertUiOrMainThread();
                return relative;
            }
        }

        public static int ScrollWheelValue
        {
            get
            {
                AssertUiOrMainThread();
                return scrollWheelValue;
            }
        }

        public static int ScrollDelta
        {
            get
            {
                AssertUiOrMainThread();
                return scrollDelta;
            }
        }

        public static bool Update(MouseSnapshot snapshot)
        {
            ThreadAffinity.AssertUiThread();
            bool moved = snapshot.Absolute != absolute || snapshot.Relative.X != relative.X || snapshot.Relative.Y != relative.Y;
            bool scrolled = snapshot.ScrollDelta != scrollDelta;
            absolute = snapshot.Absolute;
            relative = snapshot.Relative;
            scrollWheelValue = snapshot.ScrollWheelValue;
            scrollDelta = snapshot.ScrollDelta;
            return moved || scrolled;
        }

        static void AssertUiOrMainThread()
        {
            if (ThreadAffinity.IsMainThread) return;
            ThreadAffinity.AssertUiThread();
        }
    }
}
