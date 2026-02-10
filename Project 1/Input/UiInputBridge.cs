using Project_1.Managers.States;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.Managers;
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

        static void HandleClick(ClickEvent clickEvent)
        {
            ThreadAffinity.AssertUiThread();
            UiTextInputManager.Clear();
            if (StateManager.UiClick(clickEvent))
            {
                StateManager.UiInvalidate();
                return;
            }
            if (HUDManager.Click(clickEvent)) return;
            Mailboxes.PublishSimCommand(WorldClickRequested.FromClickEvent(clickEvent));
        }

        static void HandleRelease(ReleaseEvent releaseEvent)
        {
            ThreadAffinity.AssertUiThread();
            // Release can change pressed/held visual state even when no UI target captures it.
            StateManager.UiInvalidate();
            if (StateManager.UiRelease(releaseEvent))
            {
                return;
            }
            if (HUDManager.Release(releaseEvent)) return;
            Mailboxes.PublishSimCommand(WorldReleaseRequested.FromReleaseEvent(releaseEvent));
        }

        static void HandleScroll(ScrollEvent scrollEvent)
        {
            ThreadAffinity.AssertUiThread();
            if (StateManager.UiScroll(scrollEvent))
            {
                StateManager.UiInvalidate();
                return;
            }
            if (HUDManager.Scroll(scrollEvent)) return;
            Mailboxes.PublishSimCommand(WorldScrollRequested.FromScrollEvent(scrollEvent));
        }

        static void HandleKeyboardSnapshot(KeyboardSnapshot snapshot)
        {
            ThreadAffinity.AssertUiThread();
            UiKeyboardStateCache.Update(snapshot);
            Mailboxes.PublishSimCommand(snapshot);
        }

        static void HandleKeyBindSnapshot(KeyBindSnapshot snapshot)
        {
            ThreadAffinity.AssertUiThread();
            UiKeyBindStateCache.Update(snapshot);
            Mailboxes.PublishSimCommand(snapshot);
            if (!UiTextInputManager.IsActive)
            {
                Mailboxes.PublishSimCommand(new PlayerMovementRequested(
                    UiKeyBindStateCache.GetHold(KeyBindManager.KeyListner.MoveCharacterLeft),
                    UiKeyBindStateCache.GetHold(KeyBindManager.KeyListner.MoveCharacterRight),
                    UiKeyBindStateCache.GetHold(KeyBindManager.KeyListner.MoveCharacterUp),
                    UiKeyBindStateCache.GetHold(KeyBindManager.KeyListner.MoveCharacterDown)));
            }
        }

        static void HandleMouseSnapshot(MouseSnapshot snapshot)
        {
            ThreadAffinity.AssertUiThread();
            bool changed = UiMouseStateCache.Update(snapshot);
            Mailboxes.PublishSimCommand(snapshot);
            if (!changed) return;
            // TODO: Ponder whether this is the right approach.
            // Current (invalidate on mouse move/scroll):
            // + Ensures hover/drag visuals stay responsive.
            // + Simple and predictable (no extra state).
            // - Redraws every frame while mouse is moving.
            // Suggested (gate by hover/drag/hit-test or pixel threshold):
            // + Reduces redraws during continuous movement.
            // + Could skip work when no UI can visually change.
            // - Requires extra state/caching and careful hit-test tracking.
            if (StateManager.CurrentState == StateManager.States.Game || StateManager.CurrentState == StateManager.States.MoveHUD)
            {
                HUDManager.InvalidateUi();
                return;
            }
            StateManager.UiInvalidate();
        }

        static void HandleEscapePressed(EscapePressed pressed)
        {
            ThreadAffinity.AssertUiThread();
            if (StateManager.UiEscapePressed())
            {
                StateManager.UiInvalidate();
                return;
            }
            Mailboxes.PublishSimCommand(pressed);
        }
    }
}
