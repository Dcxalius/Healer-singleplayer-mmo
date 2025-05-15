using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Input;
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
            }
        }
        static bool changesMade;

        static List<UIElement>[] optionElements = new List<UIElement>[(int)OptionScreen.Count];
        static List<UIElement> optionScreenPermanents = new List<UIElement>();

        static OptionScreen currentScreen = OptionScreen.Video;

        static ExitOptionsButton exitOptionsButton;
        static SaveChangesButton saveChangesButton;

        public static void Init()
        {
            changesMade = false;
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
        }

        static void InitVideo()
        {
            optionElements[(int)OptionScreen.Video] = new List<UIElement>
            {
                new ScreenSizeSelect(new RelativeScreenPosition(0.1f, 0.1f), new RelativeScreenPosition(0.3f, 0.1f)),
                new CameraStyleSelect(new RelativeScreenPosition(0.1f, 0.22f), new RelativeScreenPosition(0.3f, 0.1f)),
            };
        }


        static void InitKeybindings()
        {
            optionElements[(int)OptionScreen.Keybindings] = new List<UIElement>();
            optionElements[(int)OptionScreen.Keybindings].Add(new KeybindingsList(new RelativeScreenPosition(0.3f, 0.1f), new RelativeScreenPosition(0.4f, 0.8f)));

        }

        public static void SetScreen(OptionScreen aNewScreen)
        {
            if (aNewScreen == currentScreen) return;

            currentScreen = aNewScreen;
            CloseAllOptionMenuStuff();
        }

        public static void Update()
        {
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
    }
}
