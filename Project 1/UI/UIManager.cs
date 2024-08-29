using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Content.Input;
using Project_1.Managers;
using Project_1.Textures;
using Project_1.UI.OptionMenu;
using Project_1.UI.PauseMenu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            optionElements.Add(new ExitOptionsButton());
        }

        static void InitGameUI()
        {
            (Vector2 pos1, Vector2 size1) = Camera.TransformDevSizeToRelativeVectors(new Rectangle(100, 100, 128, 128));
            gameElements.Add(new Box(new UITexture(new GfxPath(GfxType.UI, "WhiteBackground"), Color.AliceBlue), pos1, size1));
            (Vector2 pos2, Vector2 size2) = Camera.TransformDevSizeToRelativeVectors(new Rectangle(500, 100, 256, 128));
            gameElements.Add(new Box(new UITexture(new GfxPath(GfxType.UI, "WhiteBackground"), Color.IndianRed), pos2, size2));

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
                aListOfUIElements[i].Update();
            }

        }

        public static void Draw(SpriteBatch aBatch)
        {
            switch (StateManager.currentState)
            {
                case State.Game:
                    StateDraw(gameElements, aBatch);
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

        public static void DrawGameUI(SpriteBatch aBatch)
        {
            StateDraw(gameElements, aBatch);
        }

        static void StateDraw(List<UIElement> aListToDraw, SpriteBatch aBatch)
        {
            for (int i = 0; i < aListToDraw.Count; i++)
            {
                aListToDraw[i].Draw(aBatch);
            }

        }

        public static void Click(ClickEvent aClickEvent)
        {
            switch (StateManager.currentState)
            {
                case State.Game:
                    SearchForHit(gameElements, aClickEvent);
                    break;
                case State.Pause:
                    SearchForHit(pauseElements, aClickEvent);
                    break;
                case State.StartMenu:
                    break;
                case State.Options:
                    SearchForHit(optionElements, aClickEvent);
                    break;
                default:
                    break;
            }
        }

        public static void SearchForHit(List<UIElement> aListToSearch, ClickEvent aClickEvent)
        {
            for (int i = aListToSearch.Count - 1; i >= 0; i--)
            {
                bool clickedOn = false;
                clickedOn = aListToSearch[i].ClickedOn(aClickEvent);
                if (clickedOn == true)
                {
                    return;
                }
            }
        }

    }
}
