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

namespace Project_1.Managers.States
{

    internal static class StateManager
    {
        public enum States
        {
            StartMenu,
            Game,
            PausedGame,
            PauseMenu,
            OptionMenu
        }

        static State currentState;

        static StartMenu startMenu;
        static Game game;
        static PausedGame pausedGame;
        static PauseMenu pauseMenu;
        static OptionMenu optionMenu;

        static SpriteBatch finalBatch;

        public static RenderTarget2D FinalGameFrame { get => finalGameFrame; set => finalGameFrame = value; }
        static RenderTarget2D finalGameFrame;
        public static Rectangle RenderTargetPosition { set => renderTargetPosition = value; }
        static Rectangle renderTargetPosition;

        public static void Init(ContentManager aContentManager)
        {
            ObjectFactory.Init(aContentManager);



            GraphicsManager.Init();
            finalBatch = GraphicsManager.CreateSpriteBatch();




            startMenu = new StartMenu();
            game = new Game();
            pausedGame = new PausedGame();
            pauseMenu = new PauseMenu();
            optionMenu = new OptionMenu();

            currentState = game;
        }

        public static void Update()
        {
            currentState.Update();
        }

        public static void SetState(States aState)
        {
            currentState.OnLeave();
            switch (aState)
            {
                case States.StartMenu:
                    currentState = startMenu;
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
                case States.PausedGame:
                    currentState = pausedGame;
                    break;
                default:
                    throw new NotImplementedException();
            }
            currentState.OnEnter();
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
            startMenu.Rescale();
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
