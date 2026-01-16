using Project_1.Managers.States;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.UI.HUD.Managers;
using Project_1.UI.UIElements;

namespace Project_1.Input
{
    /// <summary>
    /// Routes input events through HUD hit-testing on the UI thread, forwarding unhandled events to main.
    /// </summary>
    internal static class UiInputBridge
    {
        public static void PublishRelease(UIElement creator, InputManager.ClickType clickType)
        {
            bool[] heldModifiers = UiKeyboardStateCache.GetHoldModifiers();
            ReleaseEvent releaseEvent = new ReleaseEvent(creator, UiMouseStateCache.Relative, clickType, heldModifiers);
            Mailboxes.Ui.Publish(releaseEvent);
        }

        public static void Init()
        {
            Mailboxes.Ui.Subscribe<ClickEvent>(HandleClick);
            Mailboxes.Ui.Subscribe<ReleaseEvent>(HandleRelease);
            Mailboxes.Ui.Subscribe<ScrollEvent>(HandleScroll);
            Mailboxes.Ui.Subscribe<KeyboardSnapshot>(HandleKeyboardSnapshot);
            Mailboxes.Ui.Subscribe<KeyBindSnapshot>(HandleKeyBindSnapshot);
            Mailboxes.Ui.Subscribe<MouseSnapshot>(HandleMouseSnapshot);
        }

        static void HandleClick(ClickEvent clickEvent)
        {
            UiTextInputManager.Clear();
            if (StateManager.UiClick(clickEvent)) return;
            if (HUDManager.Click(clickEvent)) return;
            Mailboxes.Main.Publish(clickEvent);
        }

        static void HandleRelease(ReleaseEvent releaseEvent)
        {
            if (StateManager.UiRelease(releaseEvent)) return;
            if (HUDManager.Release(releaseEvent)) return;
            Mailboxes.Main.Publish(releaseEvent);
        }

        static void HandleScroll(ScrollEvent scrollEvent)
        {
            if (StateManager.UiScroll(scrollEvent)) return;
            if (HUDManager.Scroll(scrollEvent)) return;
            Mailboxes.Main.Publish(scrollEvent);
        }

        static void HandleKeyboardSnapshot(KeyboardSnapshot snapshot)
        {
            UiKeyboardStateCache.Update(snapshot);
            Mailboxes.Main.Publish(snapshot);
        }

        static void HandleKeyBindSnapshot(KeyBindSnapshot snapshot)
        {
            UiKeyBindStateCache.Update(snapshot);
            Mailboxes.Main.Publish(snapshot);
        }

        static void HandleMouseSnapshot(MouseSnapshot snapshot)
        {
            UiMouseStateCache.Update(snapshot);
            Mailboxes.Main.Publish(snapshot);
        }
    }
}
