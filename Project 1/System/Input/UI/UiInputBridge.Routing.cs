using Project_1.Managers;
using Project_1.Managers.States;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.UI.HUD.Managers;
using System;

namespace Project_1.Input
{
    internal static partial class UiInputBridge
    {
        static void HandleClick(ClickEvent clickEvent)
        {
            ThreadAffinity.AssertUiThread();
            UiTextInputManager.Clear();
            RoutePointerEvent(
                clickEvent,
                StateManager.UiClick,
                HUDManager.Click,
                static e => Mailboxes.PublishSimCommand(WorldClickRequested.FromClickEvent(e)));
        }

        static void HandleRelease(ReleaseEvent releaseEvent)
        {
            ThreadAffinity.AssertUiThread();
            RoutePointerEvent(
                releaseEvent,
                StateManager.UiRelease,
                HUDManager.Release,
                static e => Mailboxes.PublishSimCommand(WorldReleaseRequested.FromReleaseEvent(e)));
        }

        static void HandleScroll(ScrollEvent scrollEvent)
        {
            ThreadAffinity.AssertUiThread();
            RoutePointerEvent(
                scrollEvent,
                StateManager.UiScroll,
                HUDManager.Scroll,
                static e => Mailboxes.PublishSimCommand(WorldScrollRequested.FromScrollEvent(e)));
        }

        static void HandleEscapePressed(EscapePressed _)
        {
            ThreadAffinity.AssertUiThread();
            if (StateManager.UiEscapePressed())
            {
                StateManager.UiInvalidate();
                return;
            }

            Mailboxes.PublishSimCommand(new EscapeRequested());
        }

        static void RoutePointerEvent<TEvent>(
            TEvent inputEvent,
            Func<TEvent, bool> stateHandler,
            Func<TEvent, bool> hudHandler,
            Action<TEvent> forwardToSim)
        {
            if (stateHandler(inputEvent))
            {
                StateManager.UiInvalidate();
                return;
            }

            if (hudHandler(inputEvent))
            {
                HUDManager.InvalidateUi();
                return;
            }

            forwardToSim(inputEvent);
        }
    }
}
