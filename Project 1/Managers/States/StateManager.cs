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
using Project_1.Particles;
using Project_1.UI;
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
        public static Rectangle RenderTargetPosition { set => renderTargetPosition = value; }
        static Rectangle renderTargetPosition;

        public static States PreviousState => previousState;
        static States previousState;

        static StateManager()
        {
            finalBatch = GraphicsManager.CreateSpriteBatch();

            startScreen = new StartScreen();
            game = new Game();
            moveHUD = new MoveHUD();
            pauseMenu = new PauseMenu();
            optionMenu = new OptionMenu();
            loadingMenu = new LoadingMenu();
            newGame = new NewGame();


            currentState = startScreen;

            //if (DebugManager.Mode(DebugMode.InstantlyLoadSave1)) TODO: implement
            //{
            //    SaveManager.LoadData();
            //    SetState(States.Game);
            //}
        }

        public static void Update()
        {
            currentState.Update();
        }

        public static void PopUp(DialogueBox aDialogueBox) => currentState.PopUp(aDialogueBox);
        public static void RemovePopUp(DialogueBox aDialogueBox) => currentState.RemovePopUp(aDialogueBox);

        public static void SetState(States aState)
        {
            currentState.OnLeave();
            previousState = currentState.GetStateEnum;
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
            currentState.OnEnter();
        }

        public static void RedrawGame()
        {
            finalGameFrame = game.Draw();
        }


        public static bool Click(ClickEvent aClick) => currentState.Click(aClick);
        public static bool Release(ReleaseEvent aRelease) => currentState.Release(aRelease);

        public static bool Scroll(ScrollEvent aScroll)
        {
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
            RenderTarget2D target = currentState.Draw();

            finalBatch.Begin();
            finalBatch.Draw(target, renderTargetPosition, Color.White);
            finalBatch.End();
        }
    }
}
