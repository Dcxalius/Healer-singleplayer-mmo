using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Input;
using Project_1.Managers;
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
        static PauseBox pauseBox;

        public static void Init()
        {
            HUDManager.Init();
            InitPauseMenuUI();
            OptionManager.Init();
        }

        

        static void InitPauseMenuUI()
        {

            pauseBox = new PauseBox();

        }

        public static void Update() 
        {
            switch (StateManager.currentState)
            {
                case StateManager.State.Game:
                    HUDManager.Update();
                    break;
                case StateManager.State.Pause:
                    pauseBox.Update(null);
                    break;
                case StateManager.State.StartMenu:
                    break;
                case StateManager.State.Options:
                    OptionManager.Update();
                    break;
                default:
                    break;
            }
        }

        static void StateUpdate(List<UIElement> aListOfUIElements)
        {
            for (int i = 0; i < aListOfUIElements.Count; i++)
            {
                aListOfUIElements[i].Update(null);
            }

        }

        public static void Draw(SpriteBatch aBatch)
        {
            switch (StateManager.currentState)
            {
                case StateManager.State.Game:
                    break;
                case StateManager.State.Pause:
                    pauseBox.Draw(aBatch);
                    break;
                case StateManager.State.StartMenu:
                    break;
                case StateManager.State.Options:
                    OptionManager.Draw(aBatch);
                    break;
                default:
                    break;
            }
        }



        static void StateDraw(List<UIElement> aListToDraw, SpriteBatch aBatch)
        {
            for (int i = 0; i < aListToDraw.Count; i++)
            {
                aListToDraw[i].Draw(aBatch);
            }

        }

        public static bool Click(ClickEvent aClickEvent)
        {

            switch (StateManager.currentState)
            {
                case StateManager.State.Game:
                    return HUDManager.Click(aClickEvent); 
                case StateManager.State.Pause:
                    return pauseBox.ClickedOn(aClickEvent);
                case StateManager.State.StartMenu:
                    return true;
                case StateManager.State.Options:
                    return OptionManager.SearchForHit(aClickEvent);
                default:
                    break;
            }
            return false;
        }

        public static bool Release(ReleaseEvent aReleaseEvent)
        {
            switch (StateManager.currentState)
            {
                case StateManager.State.Game:
                    return HUDManager.Release(aReleaseEvent);
                case StateManager.State.Pause:
                    //return pauseBox.ClickedOn(aClickEvent);
                    return false;
                case StateManager.State.StartMenu:
                    return false;
                case StateManager.State.Options:
                    //return OptionManager.SearchForHit(aClickEvent);
                    return false;
                default:
                    return false;
            }
        }

        public static void Rescale()
        {
            if (pauseBox == null)
            {
                return;
            }
            OptionManager.Rescale();
            pauseBox.Rescale();
        }
    }
}
