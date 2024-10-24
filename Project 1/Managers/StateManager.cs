using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Project_1.GameObjects;
using Project_1.Input;
using Project_1.UI;

namespace Project_1.Managers
{
    enum State
    {
        Game,
        Pause,
        StartMenu,
        Options
    }
    internal static class StateManager
    {
        public static State currentState = State.Game;

        public static void Update()
        {
            switch (currentState)
            {
                case State.Game:
                    GameUpdate();
                    break;
                case State.Pause:
                    PauseUpdate();
                    break;
                case State.StartMenu:
                    StartMenuUpdate();
                    break;
                case State.Options:
                    OptionsMenuUpdate();
                    break;
                default:
                    //Assert();
                    break;
            }
        }

        public static void Draw()
        {
            switch (currentState)
            {
                case State.Game:
                    GameDraw();
                    break;
                case State.Pause:
                    PauseDraw();
                    break;
                case State.StartMenu:
                    StartMenuDraw();
                    break;
                case State.Options:
                    OptionsMenuDraw();
                    break;
                default:
                    break;
            }

            Camera.DrawRenderTarget();
        }

        public static void SetState(State aState)
        {
            currentState = aState;

        }

        static void GameUpdate()
        {
            if (InputManager.GetPress(Microsoft.Xna.Framework.Input.Keys.Escape))
            {
                PauseGame(State.Pause);
            }
            Camera.Update();
            ObjectManager.Update();
            UIManager.Update();
        }

        static void PauseGame(State aStateToPauseTo)
        {
            currentState = aStateToPauseTo;
            TimeManager.StartPause();
        }

        static void GameDraw()
        {
            Camera.GameDraw();
            Camera.DrawGameToCamera();
        }

        static void PauseUpdate()
        {
            if (InputManager.GetPress(Microsoft.Xna.Framework.Input.Keys.Escape))
            {
                UnpauseGame();
            }

            UIManager.Update();
        }

        public static void UnpauseGame()
        {
            currentState = State.Game;
            TimeManager.StopPause();
        }

        static void PauseDraw()
        {
            GraphicsManager.ClearScreen(Color.Purple);
            Camera.PauseDraw();
        }

        static void StartMenuUpdate()
        {

        }

        static void StartMenuDraw()
        {
            GraphicsManager.ClearScreen(Color.White);

        }

        static void OptionsMenuUpdate()
        {

            UIManager.Update();

        }

        static void OptionsMenuDraw()
        {
            GraphicsManager.ClearScreen(Color.LightGray);
            Camera.OptionDraw();
        }
    }
}
