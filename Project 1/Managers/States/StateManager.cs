using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Project_1.GameObjects;
using Project_1.GameObjects.Spawners;
using Project_1.Input;
using Project_1.Particles;
using Project_1.UI;

namespace Project_1.Managers.States
{

    internal static class StateManager
    {
        public enum States
        {
            StartMenu,
            Game,
            Pause,
            OptionMenu
        }

        static State currentState;

        static StartMenu startMenu;
        static Game game;
        static Pause pause;
        static OptionMenu optionMenu;

        

        public static void Init()
        {
            startMenu = new StartMenu();
            game = new Game();
            pause = new Pause();
            optionMenu = new OptionMenu();

            currentState = game;
        }

        public static void Update()
        {
            currentState.Update();
        }

        public static void Draw()
        {
            currentState.Draw();


            Camera.Camera.DrawRenderTarget();
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
                case States.Pause:
                    currentState = pause;
                    break;
                case States.OptionMenu:
                    currentState = optionMenu;
                    break;
                default:
                    throw new NotImplementedException();
            }
            currentState.OnEnter();
        }
    }
}
