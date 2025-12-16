using Project_1.Managers;
using Project_1.Managers.States;
using Project_1.Messaging;

namespace Project_1.Input
{
    /// <summary>
    /// Temporary bridge to route input events from the UI mailbox into existing StateManager handlers.
    /// Keeps current behavior while the UI thread is introduced.
    /// </summary>
    internal static class InputEventBridge
    {
        public static void Init()
        {
            ThreadAffinity.AssertMainThread();

            Mailboxes.Ui.Subscribe<ClickEvent>(e => StateManager.Click(e));
            Mailboxes.Ui.Subscribe<ReleaseEvent>(e => StateManager.Release(e));
            Mailboxes.Ui.Subscribe<ScrollEvent>(e => StateManager.Scroll(e));
        }
    }
}
