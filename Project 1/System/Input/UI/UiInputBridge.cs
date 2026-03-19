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
            MailboxManager.PublishUiEvent(releaseEvent);
        }

        public static void Init()
        {
            MailboxManager.Ui.Subscribe<ClickEvent>(HandleClick);
            MailboxManager.Ui.Subscribe<ReleaseEvent>(HandleRelease);
            MailboxManager.Ui.Subscribe<ScrollEvent>(HandleScroll);
            MailboxManager.Ui.Subscribe<KeyboardSnapshot>(HandleKeyboardSnapshot);
            MailboxManager.Ui.Subscribe<KeyBindSnapshot>(HandleKeyBindSnapshot);
            MailboxManager.Ui.Subscribe<MouseSnapshot>(HandleMouseSnapshot);
            MailboxManager.Ui.Subscribe<EscapePressed>(HandleEscapePressed);
        }
    }
}
