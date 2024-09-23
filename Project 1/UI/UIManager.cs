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
        static List<UIElement> gameElements = new List<UIElement>();
        static List<UIElement> pauseElements = new List<UIElement>();
        static List<UIElement> optionElements = new List<UIElement>();

        public static void Init()
        {
            InitGameUI();
            InitPauseMenuUI();
            InitOptionsMenuUI();
        }

        public static void InitOptionsMenuUI()
        {
            optionElements.Add(new CameraStyleSelect(new Vector2(0.1f, 0.22f), new Vector2(0.3f, 0.1f)));
            optionElements.Add(new ScreenSizeSelect(new Vector2(0.1f, 0.1f), new Vector2(0.3f, 0.1f)));
            optionElements.Add(new ExitOptionsButton());
        }

        static void InitGameUI()
        {
            HUDManager.Init();
        }

        static void InitPauseMenuUI()
        {

            pauseElements.Add(new PauseBox());

        }

        public static void Update() 
        {
            switch (StateManager.currentState)
            {
                case State.Game:
                    StateUpdate(gameElements);
                    HUDManager.Update();
                    break;
                case State.Pause:
                    StateUpdate(pauseElements);
                    break;
                case State.StartMenu:
                    break;
                case State.Options:
                    StateUpdate(optionElements);
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
                case State.Game:
                    break;
                case State.Pause:
                    StateDraw(pauseElements, aBatch);
                    break;
                case State.StartMenu:
                    break;
                case State.Options:
                    StateDraw(optionElements, aBatch);
                    break;
                default:
                    break;
            }
        }

        public static void CloseAllOptionMenuStuff()
        {
            for (int i = 0; i < optionElements.Count; i++)
            {
                optionElements[i].Close();
            }
        }

        public static void DrawGameUI(SpriteBatch aBatch)
        {
            HUDManager.Draw(aBatch);
            StateDraw(gameElements, aBatch);
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
                case State.Game:
                    return SearchForHit(gameElements, aClickEvent); 
                case State.Pause:
                    return SearchForHit(pauseElements, aClickEvent);
                case State.StartMenu:
                    return true;
                case State.Options:
                    return SearchForHit(optionElements, aClickEvent);
                default:
                    break;
            }
            return false;
        }

        public static bool SearchForHit(List<UIElement> aListToSearch, ClickEvent aClickEvent)
        {
            for (int i = aListToSearch.Count - 1; i >= 0; i--)
            {
                bool clickedOn = false;
                clickedOn = aListToSearch[i].ClickedOn(aClickEvent);
                if (clickedOn == true)
                {
                    return true;
                }
            }
            return false;
        }

        public static void Rescale()
        {
            RescaleList(gameElements);
            RescaleList(optionElements);
            RescaleList(pauseElements);

        }

        public static void RescaleList(List<UIElement> aList)
        {

            foreach (UIElement element in aList)
            {
                element.Rescale();
            }
        }
    }
}
