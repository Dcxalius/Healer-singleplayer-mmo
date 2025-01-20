using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Input;
using Project_1.Managers.States;
using Project_1.Textures;
using Project_1.UI.HUD;
using Project_1.UI.OptionMenu;
using Project_1.UI.PauseMenu;
using Project_1.UI.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Project_1.UI
{
    internal static class UIManager
    {

        public static void Init()
        {
        }


        public static bool Scroll(ScrollEvent aScrollEvent)
        {
            switch (StateManager.currentState)
            {
                case StateManager.State.Game:
                case StateManager.State.Pause:
                    return false;
                case StateManager.State.StartMenu:
                    return false;
                case StateManager.State.Options:
                    return false;
                default:
                    return false;
            }
        }

        public static void Rescale()
        {
            OptionManager.Rescale();
            pauseBox.Rescale();
        }
    }
}
