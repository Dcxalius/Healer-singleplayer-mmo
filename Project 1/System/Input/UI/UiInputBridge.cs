using Project_1.Managers.States;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.Managers;
using Project_1.UI.UIElements;

namespace Project_1.Input
{
    /// <summary>
    /// Routes input events through HUD hit-testing on the UI thread, forwarding unhandled events to main.
    /// </summary>
    internal static partial class UiInputBridge
    {
        public static void PublishRelease(UIElement creator, InputManager.ClickType clickType)
        {
            ThreadAffinity.AssertUiThread();
            bool[] heldModifiers = UiKeyboardStateCache.GetHoldModifiers();
            ReleaseEvent releaseEvent = new ReleaseEvent(creator, UiMouseStateCache.Relative, clickType, heldModifiers);
            Mailboxes.PublishUiEvent(releaseEvent);
        }

        public static void Init()
        {
            Mailboxes.Ui.Subscribe<ClickEvent>(HandleClick);
            Mailboxes.Ui.Subscribe<ReleaseEvent>(HandleRelease);
            Mailboxes.Ui.Subscribe<ScrollEvent>(HandleScroll);
            Mailboxes.Ui.Subscribe<KeyboardSnapshot>(HandleKeyboardSnapshot);
            Mailboxes.Ui.Subscribe<KeyBindSnapshot>(HandleKeyBindSnapshot);
            Mailboxes.Ui.Subscribe<MouseSnapshot>(HandleMouseSnapshot);
            Mailboxes.Ui.Subscribe<EscapePressed>(HandleEscapePressed);
        }
    }
}
