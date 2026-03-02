using Project_1.Input;

namespace Project_1.UI.HUD.Managers
{
    internal static partial class HUDManager
    {
        public static bool Click(ClickEvent aClickEvent)
        {
            AssertUiThreadOrMainFallback();
            if (sizeChanger.ClickedOn(aClickEvent)) return true;

            for (int i = 0; i < dialogueBoxes.Count; i++)
            {
                if (dialogueBoxes[i].ClickedOn(aClickEvent)) return true;
            }

            for (int i = hudElements.Count - 1; i >= 0; i--)
            {
                if (!hudElements[i].ClickedOn(aClickEvent)) continue;

                UI.UIElements.UIElement temp = hudElements[i];
                hudElements.RemoveAt(i);
                hudElements.Add(temp);
                InvalidateUi();
                return true;
            }

            if (plateBoxHandler.Click(aClickEvent)) return true;

            return false;
        }

        public static bool Release(ReleaseEvent aReleaseEvent)
        {
            AssertUiThreadOrMainFallback();
            for (int i = hudElements.Count - 1; i >= 0; i--)
            {
                if (hudElements[i].ReleasedOn(aReleaseEvent)) return true;
            }

            InvalidateUi();
            return false;
        }

        internal static bool Scroll(ScrollEvent aScrollEvent)
        {
            AssertUiThreadOrMainFallback();
            for (int i = 0; i < dialogueBoxes.Count; i++)
            {
                if (dialogueBoxes[i].ScrolledOn(aScrollEvent)) return true;
            }

            for (int i = 0; i < hudElements.Count; i++)
            {
                if (hudElements[i].ScrolledOn(aScrollEvent)) return true;
            }

            InvalidateUi();
            return false;
        }

        public static void LeavingGameState()
        {
            AssertUiThreadOrMainFallback();
            for (int i = 0; i < hudElements.Count; i++)
            {
                hudElements[i].LeavingGameState();
            }

            heldItem.ReleaseMe();
            heldSpell.ReleaseMe();
        }
    }
}
