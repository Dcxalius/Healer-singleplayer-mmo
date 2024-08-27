using System;
using System.Collections.Generic;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Project_1.Content.Input;
using Project_1.GameObjects;
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

        }

        static void GameUpdate()
        {

            if ( InputManager.GetPress(Microsoft.Xna.Framework.Input.Keys.Escape))
            {
                currentState = State.Pause;
            }


            Camera.Update();
            ObjectManager.Update();
            UIManager.GameUpdate();
        }

        static void GameDraw()
        {
            Camera.RunningDraw();
        }

        static void PauseUpdate()
        {
            if (InputManager.GetPress(Microsoft.Xna.Framework.Input.Keys.Escape))
            {
                currentState = State.Game;
            }

            UIManager.PauseUpdate();
        }

        static void PauseDraw()
        {
            
            Camera.PauseDraw();
        }

        static void StartMenuUpdate()
        {

        }

        static void StartMenuDraw()
        {

        }

        static void OptionsMenuUpdate()
        {

        }

        static void OptionsMenuDraw()
        {

        }
    }
}
