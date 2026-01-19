using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Project_1.GameObjects;
using Project_1.GameObjects.Spawners;
using Project_1.Input;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.Particles;
using Project_1.UI;
using Project_1.UI.HUD.Managers;
using Project_1.UI.OptionMenu;
using Project_1.UI.PauseMenu;
using Project_1.UI.UIElements.Boxes;

namespace Project_1.Managers.States
{

    internal static class StateManager
    {
        public enum States
        {
            StartScreen,
            Game,
            MoveHUD,
            PauseMenu,
            OptionMenu,
            LoadingMenu,
            NewGame
        }

        static State currentState;
        static States currentStateEnum;

        static StartScreen startScreen;
        static Game game;
        static MoveHUD moveHUD;
        static PauseMenu pauseMenu;
        static OptionMenu optionMenu;
        static LoadingMenu loadingMenu;
        static NewGame newGame;

        static SpriteBatch finalBatch;

        public static RenderTarget2D CleanGameTarget => game.CleanGameDraw();
        public static RenderTarget2D FinalGameFrame { get => finalGameFrame; set => finalGameFrame = value; }
        static RenderTarget2D finalGameFrame;
        static bool pendingRedrawGame;
        public static Rectangle RenderTargetPosition { set => renderTargetPosition = value; }
        static Rectangle renderTargetPosition;

        public static States PreviousState => previousState;
        static States previousState;
        static bool initialized;

        public static void Init()
        {
            ThreadAffinity.AssertMainThread();
            if (initialized) return;
            initialized = true;

            finalBatch = GraphicsManager.CreateSpriteBatch();

            startScreen = new StartScreen();
            game = new Game();
            moveHUD = new MoveHUD();
            pauseMenu = new PauseMenu();
            optionMenu = new OptionMenu();
            loadingMenu = new LoadingMenu();
            newGame = new NewGame();

            currentState = startScreen;
            currentStateEnum = States.StartScreen;

            Mailboxes.Ui.Subscribe<StateChanged>(HandleUiStateChanged);
            Mailboxes.Main.Subscribe<StateChangeRequested>(e => SetState(e.State));
        }

        public static void Update()
        {
            ThreadAffinity.AssertSimThread();
            currentState.Update();
        }

        public static void PopUp(DialogueBox aDialogueBox) => currentState.PopUp(aDialogueBox);
        public static void RemovePopUp(DialogueBox aDialogueBox) => currentState.RemovePopUp(aDialogueBox);

        public static void SetState(States aState)
        {
            ThreadAffinity.AssertSimThread();
            lock (HUDManager.UiLock)
            {
                States leavingState = currentStateEnum;
                currentState.OnLeave();
                previousState = leavingState;
                switch (aState)
                {
                    case States.StartScreen:
                        currentState = startScreen;
                        break;
                    case States.Game:
                        currentState = game;
                        break;
                    case States.PauseMenu:
                        currentState = pauseMenu;
                        break;
                    case States.OptionMenu:
                        currentState = optionMenu;
                        break;
                    case States.MoveHUD:
                        currentState = moveHUD;
                        break;
                    case States.LoadingMenu:
                        currentState = loadingMenu;
                        break;
                    case States.NewGame:
                        currentState = newGame;
                        break;
                    default:
                        throw new NotImplementedException();
                }
                currentStateEnum = aState;
                currentState.OnEnter();
                if (UiThread.IsRunning)
                {
                    Mailboxes.Ui.Publish(new StateChanged(leavingState, aState));
                }
                else
                {
                    UiOnLeave(leavingState);
                    UiOnEnter(aState);
                }
            }
        }

        public static void RequestStateChange(States aState)
        {
            if (!SimThread.IsRunning || ThreadAffinity.IsSimThread)
            {
                SetState(aState);
                return;
            }
            Mailboxes.Main.Publish(new StateChangeRequested(aState));
        }

        public static void RedrawGame()
        {
            if (!ThreadAffinity.IsMainThread)
            {
                pendingRedrawGame = true;
                return;
            }
            finalGameFrame = game.Draw();
        }


        public static bool Click(ClickEvent aClick)
        {
            ThreadAffinity.AssertSimThread();
            return currentState.Click(aClick);
        }

        public static bool Release(ReleaseEvent aRelease)
        {
            ThreadAffinity.AssertSimThread();
            return currentState.Release(aRelease);
        }

        public static bool Scroll(ScrollEvent aScroll)
        {
            ThreadAffinity.AssertSimThread();
            if (currentState.Scroll(aScroll)) return true;
            Camera.Camera.Scroll(aScroll);
            return true;
        }

        public static void Rescale()
        {
            if (currentState == null) return;
            game.Rescale();
            optionMenu.Rescale();
            pauseMenu.Rescale();
            startScreen.Rescale();
        }
        public static void Draw()
        {
            ThreadAffinity.AssertMainThread();
            if (pendingRedrawGame && ThreadAffinity.IsMainThread)
            {
                pendingRedrawGame = false;
                finalGameFrame = game.Draw();
            }
            RenderTarget2D target = currentState.Draw();

            finalBatch.Begin();
            finalBatch.Draw(target, renderTargetPosition, Color.White);
            finalBatch.End();
        }
        public static States CurrentState => currentStateEnum;

        internal static bool UiClick(ClickEvent aClick)
        {
            ThreadAffinity.AssertUiThread();
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
            ThreadAffinity.AssertUiThread();
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
            ThreadAffinity.AssertUiThread();
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

        internal static void UiUpdate()
        {
            ThreadAffinity.AssertUiThread();
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
        }

        internal static void UiOnLeave(States state)
        {
            ThreadAffinity.AssertUiThread();
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
            ThreadAffinity.AssertUiThread();
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

        static void HandleUiStateChanged(StateChanged e)
        {
            UiOnLeave(e.Previous);
            UiOnEnter(e.Current);
        }
    }
}
