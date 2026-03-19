using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Input;
using Project_1.Managers;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.UI;
using Project_1.UI.UIElements.Boxes;

namespace Project_1.Managers.States
{

    internal static partial class StateManager
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
        static Rectangle renderTargetPosition;

        public static States PreviousState => previousState;
        static States previousState;
        static bool stateChangePending;
        static States pendingState;
        static bool initialized;
        public static void Init()
        {
            ThreadAffinity.AssertMainThread();
            if (initialized) return;
            initialized = true;

            finalBatch = GraphicsManager.CreateSpriteBatch();
            CreateStates();
            GraphicsManager.WindowLayoutChanged += HandleGraphicsLayoutChanged;
            if (GraphicsManager.HasWindowLayout)
            {
                HandleGraphicsLayoutChanged(GraphicsManager.CurrentWindowSize, GraphicsManager.CurrentRenderTargetDestination);
            }
            RegisterStateSubscriptions();
            RegisterUiAndInputSubscriptions();
        }

        static void CreateStates()
        {
            startScreen = new StartScreen();
            game = new Game();
            moveHUD = new MoveHUD();
            pauseMenu = new PauseMenu();
            optionMenu = new OptionMenu();
            loadingMenu = new LoadingMenu();
            newGame = new NewGame();

            currentState = startScreen;
            currentStateEnum = States.StartScreen;
        }

        static void RegisterStateSubscriptions()
        {
            SubscribeSimCommand<StateChangeRequested>(e => SetState(e.State.ToStateManagerState()));
        }

        static void RegisterUiAndInputSubscriptions()
        {
            MailboxManager.Ui.Subscribe<StateChanged>(HandleUiStateChanged);
            MailboxManager.Ui.Subscribe<HudRescaleRequested>(e => UiRescale(e.WindowSize));
            SubscribeSimMailbox<KeyboardSnapshot>(e => KeyboardStateCache.Update(e));
            SubscribeSimMailbox<KeyBindSnapshot>(e => KeyBindStateCache.Update(e));
            SubscribeSimMailbox<MouseSnapshot>(e => MouseStateCache.Update(e));
            SubscribeSimCommand<EscapeRequested>(_ => HandleEscapePressed());
        }

        static void SubscribeSimCommand<T>(Action<T> handler)
        {
            MailboxManager.RegisterSimCommandType<T>();
            SubscribeSimMailbox(handler);
        }

        static void SubscribeSimMailbox<T>(Action<T> handler)
        {
            MailboxManager.Sim.Subscribe(handler);
        }

        public static void Update()
        {
            ThreadAffinity.AssertSimThread();
            KeyboardStateCache.BeginFrame();
            KeyBindStateCache.BeginFrame();
            ApplyPendingStateChange();
            currentState.Update();
        }

        public static void PopUp(DialogueBox aDialogueBox) => currentState.PopUp(aDialogueBox);
        public static void RemovePopUp(DialogueBox aDialogueBox) => currentState.RemovePopUp(aDialogueBox);

        public static void SetState(States aState)
        {
            ThreadAffinity.AssertSimThread();
            pendingState = aState;
            stateChangePending = true;
        }

        public static void RequestStateChange(States aState)
        {
            if (!SimThread.IsRunning || ThreadAffinity.IsSimThread)
            {
                SetState(aState);
                return;
            }

            MailboxManager.PublishSimCommand(new StateChangeRequested(aState.ToStateKind()));
        }

        static void ApplyPendingStateChange()
        {
            if (!stateChangePending) return;
            stateChangePending = false;
            ApplyStateChange(pendingState);
        }

        static void ApplyStateChange(States aState)
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
            if (UiThread.IsRunning || SimThread.IsRunning)
            {
                MailboxManager.PublishUiEvent(new StateChanged(leavingState.ToStateKind(), aState.ToStateKind()));
                return;
            }

            UiOnLeave(leavingState);
            UiOnEnter(aState);
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
            ThreadAffinity.AssertMainThread();
            if (currentState == null) return;
            game.Rescale();
            moveHUD.Rescale();
            optionMenu.Rescale();
            pauseMenu.Rescale();
            startScreen.Rescale();
            loadingMenu.Rescale();
            newGame.Rescale();
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

        static void HandleGraphicsLayoutChanged(Point windowSize, Rectangle renderTargetDestination)
        {
            ThreadAffinity.AssertMainThread();
            renderTargetPosition = renderTargetDestination;
            Rescale();
        }

    }
}
