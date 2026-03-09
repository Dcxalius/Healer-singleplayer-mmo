using Microsoft.Xna.Framework;
using Project_1.Input;
using Project_1.Managers;
using Project_1.Messaging.Events;
using Project_1.UI;
using Project_1.UI.HUD.Managers;
using Project_1.UI.UIElements;

namespace Project_1.Managers.States
{
    internal static partial class StateManager
    {
        static void AssertUiOwnerThread()
        {
            if (UiThread.IsRunning)
            {
                ThreadAffinity.AssertUiThread();
                return;
            }

            ThreadAffinity.AssertMainThread();
        }

        internal static bool UiClick(ClickEvent aClick)
        {
            AssertUiOwnerThread();
            switch (currentStateEnum)
            {
                case States.StartScreen:
                    return startScreen.UiClick(aClick);
                case States.PauseMenu:
                    return pauseMenu.UiClick(aClick);
                case States.OptionMenu:
                    return optionMenu.UiClick(aClick);
                case States.LoadingMenu:
                    return loadingMenu.UiClick(aClick);
                case States.NewGame:
                    return newGame.UiClick(aClick);
                case States.MoveHUD:
                    return moveHUD.UiClick(aClick);
                default:
                    return false;
            }
        }

        internal static bool UiRelease(ReleaseEvent aRelease)
        {
            AssertUiOwnerThread();
            switch (currentStateEnum)
            {
                case States.StartScreen:
                    return startScreen.UiRelease(aRelease);
                case States.PauseMenu:
                    return pauseMenu.UiRelease(aRelease);
                case States.OptionMenu:
                    return optionMenu.UiRelease(aRelease);
                case States.LoadingMenu:
                    return loadingMenu.UiRelease(aRelease);
                case States.NewGame:
                    return newGame.UiRelease(aRelease);
                case States.MoveHUD:
                    return moveHUD.UiRelease(aRelease);
                default:
                    return false;
            }
        }

        internal static bool UiScroll(ScrollEvent aScroll)
        {
            AssertUiOwnerThread();
            switch (currentStateEnum)
            {
                case States.StartScreen:
                    return startScreen.UiScroll(aScroll);
                case States.PauseMenu:
                    return pauseMenu.UiScroll(aScroll);
                case States.OptionMenu:
                    return optionMenu.UiScroll(aScroll);
                case States.LoadingMenu:
                    return loadingMenu.UiScroll(aScroll);
                case States.NewGame:
                    return newGame.UiScroll(aScroll);
                case States.MoveHUD:
                    return moveHUD.UiScroll(aScroll);
                default:
                    return false;
            }
        }

        internal static bool UiEscapePressed()
        {
            AssertUiOwnerThread();
            switch (currentStateEnum)
            {
                case States.StartScreen:
                    return startScreen.UiEscapePressed();
                case States.PauseMenu:
                    return pauseMenu.UiEscapePressed();
                case States.OptionMenu:
                    return optionMenu.UiEscapePressed();
                case States.LoadingMenu:
                    return loadingMenu.UiEscapePressed();
                case States.NewGame:
                    return newGame.UiEscapePressed();
                case States.MoveHUD:
                    return moveHUD.UiEscapePressed();
                default:
                    return false;
            }
        }

        internal static void UiInvalidate()
        {
            AssertUiOwnerThread();
            currentState?.MarkUiDirty();
        }

        internal static void UiUpdate()
        {
            AssertUiOwnerThread();
            long interactionVersionBefore = UIElement.InteractionVersion;
            switch (currentStateEnum)
            {
                case States.StartScreen:
                    startScreen.UiUpdate();
                    break;
                case States.PauseMenu:
                    pauseMenu.UiUpdate();
                    break;
                case States.OptionMenu:
                    optionMenu.UiUpdate();
                    break;
                case States.LoadingMenu:
                    loadingMenu.UiUpdate();
                    break;
                case States.NewGame:
                    newGame.UiUpdate();
                    break;
                case States.MoveHUD:
                    moveHUD.UiUpdate();
                    break;
                default:
                    break;
            }

            if (currentStateEnum != States.Game
                && currentStateEnum != States.MoveHUD
                && UIElement.InteractionVersion != interactionVersionBefore)
            {
                currentState?.MarkUiDirty();
            }
        }

        static void HandleEscapePressed()
        {
            ThreadAffinity.AssertSimThread();
            switch (currentStateEnum)
            {
                case States.Game:
                    if (HasGroundTargetPendingSpell)
                    {
                        CancelGroundTargeting();
                        break;
                    }
                    RequestStateChange(States.PauseMenu);
                    break;
                case States.PauseMenu:
                    pauseMenu.HandleEscapePressed();
                    break;
                default:
                    break;
            }
        }

        internal static void UiOnLeave(States state)
        {
            AssertUiOwnerThread();
            switch (state)
            {
                case States.Game:
                    game.UiOnLeave();
                    break;
                case States.MoveHUD:
                    moveHUD.UiOnLeave();
                    break;
                case States.OptionMenu:
                    optionMenu.UiOnLeave();
                    break;
                case States.LoadingMenu:
                    loadingMenu.UiOnLeave();
                    break;
                case States.NewGame:
                    newGame.UiOnLeave();
                    break;
                default:
                    break;
            }
        }

        internal static void UiOnEnter(States state)
        {
            AssertUiOwnerThread();
            switch (state)
            {
                case States.MoveHUD:
                    moveHUD.UiOnEnter();
                    break;
                case States.OptionMenu:
                    optionMenu.UiOnEnter();
                    break;
                case States.LoadingMenu:
                    loadingMenu.UiOnEnter();
                    break;
                case States.NewGame:
                    newGame.UiOnEnter();
                    break;
                default:
                    break;
            }
        }

        internal static void UiRescale(Point windowSize)
        {
            AssertUiOwnerThread();

            HUDManager.Rescale();
            HUDManager.InvalidateUi();
            HUDManager.InvalidatePlates();
            switch (currentStateEnum)
            {
                case States.StartScreen:
                    startScreen.UiRescale();
                    break;
                case States.PauseMenu:
                    pauseMenu.UiRescale();
                    break;
                case States.OptionMenu:
                    optionMenu.UiRescale();
                    break;
                case States.LoadingMenu:
                    loadingMenu.UiRescale();
                    break;
                case States.NewGame:
                    newGame.UiRescale();
                    break;
                case States.MoveHUD:
                    moveHUD.UiRescale();
                    break;
                default:
                    break;
            }
        }

        static void HandleUiStateChanged(StateChanged e)
        {
            AssertUiOwnerThread();
            UiOnLeave(e.Previous.ToStateManagerState());
            UiOnEnter(e.Current.ToStateManagerState());
        }
    }
}
