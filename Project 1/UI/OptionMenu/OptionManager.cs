using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Input;
using Project_1.Managers;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Buttons;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.OptionMenu
{
    internal static class OptionManager
    { 
        public enum OptionScreen
        {
            Video,
            Keybindings,
            Count
        }
        public static bool ChangesMade
        {
            set
            {
                if (changesMade == false && value == true)
                {
                    exitOptionsButton.ButtonText = "Revert";
                    saveChangesButton.Visible = true;
                }
                if (changesMade == true && value == false)
                {
                    exitOptionsButton.ButtonText = "Close";
                    saveChangesButton.Visible = false;

                }
                changesMade = value;
                renderDirty = true;
            }
        }
        static bool changesMade;

        static List<UIElement>[] optionElements = new List<UIElement>[(int)OptionScreen.Count];
        static List<UIElement> optionScreenPermanents = new List<UIElement>();

        static OptionScreen currentScreen = OptionScreen.Video;

        static ExitOptionsButton exitOptionsButton;
        static SaveChangesButton saveChangesButton;
        static bool initialized;
        static volatile bool drawListDirty = true;
        static volatile bool renderDirty = true;

        public static void Init()
        {
            ThreadAffinity.AssertMainThread();
            if (initialized) return;
            initialized = true;
            changesMade = false;
            drawListDirty = true;
            renderDirty = true;
            InitPermanents();
            InitVideo();
            InitKeybindings();
        }

        static void InitPermanents()
        {
            optionScreenPermanents.Add(new OptionScreenBox((int)OptionScreen.Count, new RelativeScreenPosition(0), new RelativeScreenPosition(0.3f,0.04f)));

            RelativeScreenPosition buttonSize = new RelativeScreenPosition(0.08f, 0.05f);
            RelativeScreenPosition buttonPos = new RelativeScreenPosition(0.9f);

            exitOptionsButton = new ExitOptionsButton(buttonPos, buttonSize, Color.Beige, Color.Black);
            saveChangesButton = new SaveChangesButton(buttonPos - buttonSize.OnlyX, buttonSize, Color.Beige, Color.Black);
            

            optionScreenPermanents.Add(exitOptionsButton);
            optionScreenPermanents.Add(saveChangesButton);
        }
        
        public static void AddActionToDoAtExitOfOptionMenu(Action aReverseAction, Action aAction)
        {
            //TODO: Right now this holds because the first changes you make gets called last, this is pretty ugly however so a better system should be developed
            ChangesMade = true;
            exitOptionsButton.AddFuncToTriggerOnExit(aReverseAction);
            saveChangesButton.AddAction(aAction);
        }

        public static void AddFinalActions(Action aAction)
        {
            saveChangesButton.AddFinalActions(aAction);
        }

        public static void ClearButtons()
        {
            saveChangesButton.Visible = false;
            ChangesMade = false;
            exitOptionsButton.ClearActions();
            saveChangesButton.ClearActions();
            renderDirty = true;
        }

        static void InitVideo()
        {
            optionElements[(int)OptionScreen.Video] = new List<UIElement>
            {
                new ScreenSizeSelect(new RelativeScreenPosition(0.1f, 0.1f), new RelativeScreenPosition(0.3f, 0.1f)),
                new CameraStyleSelect(new RelativeScreenPosition(0.1f, 0.22f), new RelativeScreenPosition(0.3f, 0.1f)),
                new FullScreenSelect(new RelativeScreenPosition(0.1f, 0.34f), new RelativeScreenPosition(0.3f, 0.1f))
            };
        }


        static void InitKeybindings()
        {
            optionElements[(int)OptionScreen.Keybindings] = new List<UIElement>();
            optionElements[(int)OptionScreen.Keybindings].Add(new KeybindingsList(new RelativeScreenPosition(0.3f, 0.1f), new RelativeScreenPosition(0.4f, 0.8f)));

        }

        public static void SetScreen(OptionScreen aNewScreen)
        {
            ThreadAffinity.AssertUiThread();
            if (aNewScreen == currentScreen) return;

            currentScreen = aNewScreen;
            CloseAllOptionMenuStuff();
            drawListDirty = true;
            renderDirty = true;
        }

        public static void Update()
        {
            ThreadAffinity.AssertUiThread();
            for (int i = 0; i < optionScreenPermanents.Count; i++)
            {
                optionScreenPermanents[i].Update();
            }

            for (int i = 0; i < optionElements[(int)currentScreen].Count ; i++)
            {
                optionElements[(int)currentScreen][i].Update();
            }
        }

        internal static bool Scroll(ScrollEvent aScrollEvent)
        {
            ThreadAffinity.AssertUiThread();
            for (int i = 0; i < optionElements[(int)currentScreen].Count; i++)
            {
                bool clickedOn = optionElements[(int)currentScreen][i].ScrolledOn(aScrollEvent);
                if (clickedOn == true)
                {
                    return true;
                }
            }
            return false;
        }



        public static void CloseAllOptionMenuStuff()
        {
            ThreadAffinity.AssertUiThread();
            for (int i = 0; i < optionElements.Length; i++)
            {
                for (int j = 0; j < optionElements[i].Count; j++)
                {
                    optionElements[i][j].Close();
                }
            }
        }

        public static bool Click(ClickEvent aClickEvent)
        {
            ThreadAffinity.AssertUiThread();
            for (int i = 0; i < optionScreenPermanents.Count; i++)
            {
                bool clickedOn = optionScreenPermanents[i].ClickedOn(aClickEvent);
                if (clickedOn == true)
                {
                    return true;
                }
            }

            for (int i = 0; i < optionElements[(int)currentScreen].Count; i++)
            {
                if (optionElements[(int)currentScreen][i].ClickedOn(aClickEvent)) return true;
            }
            return false;
        }

        public static void Rescale()
        {
            ThreadAffinity.AssertMainThread();
            for (int i = 0; i < optionScreenPermanents.Count; i++)
            {

                optionScreenPermanents[i].Rescale();
            }
            for (int i = 0; i < optionElements.Length; i++)
            {
                for (int j = 0; j < optionElements[i].Count; j++)
                {
                    optionElements[i][j].Rescale();
                }
            }
        }



        public static void Draw(SpriteBatch aBatch)
        {
            for (int i = 0; i < optionScreenPermanents.Count; i++)
            {
                optionScreenPermanents[i].Draw(aBatch);
            }

            for (int i = optionElements[(int)currentScreen].Count - 1; i >= 0; i--)
            {
                optionElements[(int)currentScreen][i].Draw(aBatch);
            }
        }

        public static UIElement[] BuildDrawList()
        {
            ThreadAffinity.AssertUiThread();
            List<UIElement> drawList = new List<UIElement>(optionScreenPermanents.Count + optionElements[(int)currentScreen].Count);
            for (int i = 0; i < optionScreenPermanents.Count; i++)
            {
                drawList.Add(optionScreenPermanents[i]);
            }

            for (int i = optionElements[(int)currentScreen].Count - 1; i >= 0; i--)
            {
                drawList.Add(optionElements[(int)currentScreen][i]);
            }

            return drawList.ToArray();
        }

        public static bool ConsumeDrawListDirty()
        {
            ThreadAffinity.AssertUiThread();
            if (!drawListDirty) return false;
            drawListDirty = false;
            return true;
        }

        public static bool ConsumeRenderDirty()
        {
            ThreadAffinity.AssertUiThread();
            if (!renderDirty) return false;
            renderDirty = false;
            return true;
        }
    }
}
