using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Managers;
using Project_1.UI.UIElements;
using System.Collections.Generic;

namespace Project_1.UI.HUD.Managers
{
    internal static partial class HUDManager
    {
        public static void SetSizeChanger(UIElement aUIElement)
        {
            AssertUiThreadOrMainFallback();
            if (!sizeChanger.Active) return;
            sizeChanger.SetElement(aUIElement);
        }

        public static void DisableHudMoveable() => SetHudMoveable(false);

        public static void SetHudMoveable(bool aSet)
        {
            AssertUiThreadOrMainFallback();
            plateBoxHandler.SetHudMovable(aSet);

            for (int i = 0; i < hudElements.Count; i++)
            {
                hudElements[i].SetHudMoveable(aSet);
            }

            for (int i = 0; i < dialogueBoxes.Count; i++)
            {
                dialogueBoxes[i].SetHudMoveable(aSet);
            }
        }

        public static void ResetHudMoveable()
        {
            AssertUiThreadOrMainFallback();
            plateBoxHandler.ResetHudMovable();

            for (int i = 0; i < hudElements.Count; i++)
            {
                hudElements[i].ResetHudMoveable();
            }

            for (int i = 0; i < dialogueBoxes.Count; i++)
            {
                dialogueBoxes[i].ResetHudMoveable();
            }
        }

        public static void HudMoveableDraw(SpriteBatch aBatch)
        {
            ThreadAffinity.AssertMainThread();
            plateBoxHandler.HudMovableDraw(aBatch);

            for (int i = 0; i < hudElements.Count; i++)
            {
                hudElements[i].HudMovableDraw(aBatch);
            }

            for (int i = 0; i < dialogueBoxes.Count; i++)
            {
                dialogueBoxes[i].HudMovableDraw(aBatch);
            }

            sizeChanger.Draw(aBatch);
        }

        public static void HudMovableUpdate()
        {
            AssertUiThreadOrMainFallback();
            plateBoxHandler.HudMovableUpdate();

            for (int i = 0; i < hudElements.Count; i++)
            {
                hudElements[i].Update();
            }

            for (int i = 0; i < dialogueBoxes.Count; i++)
            {
                dialogueBoxes[i].Update();
            }

            sizeChanger.Update();
        }

        public static void ChangeSizes()
        {
            AssertUiThreadOrMainFallback();
            sizeChanger.Active = true;
        }

        public static void DisableSizeChanges()
        {
            AssertUiThreadOrMainFallback();
            sizeChanger.Active = false;
        }

        public static void Save()
        {
            AssertUiThreadOrMainFallback();
            List<(string, RelativeScreenPosition, RelativeScreenPosition)> saveables = new List<(string, RelativeScreenPosition, RelativeScreenPosition)>();
            plateBoxHandler.Save(ref saveables);

            for (int i = 0; i < hudElements.Count; i++)
            {
                if (!hudElements[i].HudMoveable) continue;
                saveables.Add(hudElements[i].Save);
            }

            for (int i = 0; i < dialogueBoxes.Count; i++)
            {
                if (!dialogueBoxes[i].HudMoveable) continue;
                saveables.Add(dialogueBoxes[i].Save);
            }

            SaveManager.ExportData(SaveManager.HudSettings, saveables);
        }
    }
}
