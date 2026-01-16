using Project_1.Managers;
using Project_1.Managers.States;
using Project_1.Messaging;
using Project_1.Messaging.Events;

namespace Project_1.Input
{
    /// <summary>
    /// Temporary bridge to route input events from the main mailbox into existing StateManager handlers.
    /// Keeps current behavior while UI and simulation threads are introduced.
    /// </summary>
    internal static class InputEventBridge
    {
        public static void Init()
        {
            ThreadAffinity.AssertMainThread();

            Mailboxes.Main.Subscribe<ClickEvent>(e => StateManager.Click(e));
            Mailboxes.Main.Subscribe<ReleaseEvent>(e => StateManager.Release(e));
            Mailboxes.Main.Subscribe<ScrollEvent>(e => StateManager.Scroll(e));
            Mailboxes.Main.Subscribe<KeyboardSnapshot>(e => KeyboardStateCache.Update(e));
            Mailboxes.Main.Subscribe<KeyBindSnapshot>(e => KeyBindStateCache.Update(e));
            Mailboxes.Main.Subscribe<MouseSnapshot>(e => MouseStateCache.Update(e));
        }
    }
}
