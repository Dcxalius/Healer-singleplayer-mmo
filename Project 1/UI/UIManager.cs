using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Content.Input;
using Project_1.Managers;
using Project_1.Textures;
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

        public static void Init()
        {
            (Vector2 pos1, Vector2 size1) = Camera.TransformDevSizeToRelativeVectors(new Rectangle(100, 100, 128, 128));
            gameElements.Add(new Box(new UITexture(new GfxPath(GfxType.UI, "WhiteBackground"), Color.AliceBlue), pos1, size1));
            (Vector2 pos2, Vector2 size2) = Camera.TransformDevSizeToRelativeVectors(new Rectangle(500, 100, 256, 128));
            gameElements.Add(new Box(new UITexture(new GfxPath(GfxType.UI, "WhiteBackground"), Color.IndianRed), pos2, size2));

            pauseElements.Add(new PauseBox());
        }

        public static void Update() 
        {
        
        }

        public static void GameUpdate()
        {
            for (int i = 0; i < gameElements.Count; i++)
            {
                UIElement element = gameElements[i];
                element.Update();
            }

            Update();
        }

        public static void PauseUpdate()
        {
            for (int i = 0; i < pauseElements.Count; i++)
            {
                UIElement element = pauseElements[i];
                element.Update();
            }

            Update();
        }

        public static void GameDraw(SpriteBatch aBatch)
        {
            for (int i = 0; i < gameElements.Count; i++)
            {
                UIElement element = gameElements[i];
                element.Draw(aBatch);
            }
        }

        public static void PauseDraw(SpriteBatch aBatch)
        {
            for (int i = 0; i < pauseElements.Count; i++)
            {
                UIElement element = pauseElements[i];
                element.Draw(aBatch);
            }
        }

        public static void Click(ClickEvent aClickEvent)
        {
            switch (StateManager.currentState)
            {
                case State.Game:
                    GameClick(aClickEvent);
                    break;
                case State.Pause:
                    PauseClick(aClickEvent);
                    break;
                case State.StartMenu:
                    break;
                case State.Options:
                    break;
                default:
                    break;
            }
        }

        public static void GameClick(ClickEvent aClickEvent)
        {
            for (int i = gameElements.Count - 1; i >= 0; i--)
            {
                UIElement element = gameElements[i];
                bool clickedOn = false;
                clickedOn = element.ClickedOn(aClickEvent);
                if (clickedOn == true)
                {
                    return;
                }
            }
        }

        public static void PauseClick(ClickEvent aClickEvent)
        {
            for (int i = pauseElements.Count - 1; i >= 0; i--)
            {
                UIElement element = pauseElements[i];
                bool clickedOn = false;
                clickedOn = element.ClickedOn(aClickEvent);
                if (clickedOn == true)
                {
                    return;
                }
            }
        }

    }
}
