﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Input;
using Project_1.UI.UIElements;
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
        static List<UIElement>[] optionElements = new List<UIElement>[(int)OptionScreen.Count];
        static List<UIElement> optionScreenPermanents = new List<UIElement>();

        static OptionScreen currentScreen = OptionScreen.Video;

        static ExitOptionsButton exitOptionsButton;

        public static void Init()
        {
            InitPermanents();
            InitVideo();
            InitKeybindings();
        }

        static void InitPermanents()
        {
            optionScreenPermanents.Add(new OptionScreenBox((int)OptionScreen.Count, new RelativeScreenPosition(0), new RelativeScreenPosition(0.3f,0.04f)));
            exitOptionsButton = new ExitOptionsButton();
            optionScreenPermanents.Add(exitOptionsButton);
        }
        
        public static void AddActionToDoAtExitOfOptionMenu(Action aAction)
        {
            exitOptionsButton.AddFuncToTriggerOnExit(aAction);
        }

        static void InitVideo()
        {
            optionElements[(int)OptionScreen.Video] = new List<UIElement>
            {
                new CameraStyleSelect(new RelativeScreenPosition(0.1f, 0.22f), new RelativeScreenPosition(0.3f, 0.1f)),
                new ScreenSizeSelect(new RelativeScreenPosition(0.1f, 0.1f), new RelativeScreenPosition(0.3f, 0.1f)),
            };
        }

        static RelativeScreenPosition keybindingStartPos = new RelativeScreenPosition(0.2f, 0.1f);
        static RelativeScreenPosition keybindingSize = new RelativeScreenPosition(0.5f, 0.05f);
        static RelativeScreenPosition keybindingSpacing = new RelativeScreenPosition(0, 0.01f);
        static RelativeScreenPosition GetKeybindingPos
        {
            get
            {
                RelativeScreenPosition r = keybindingStartPos;
                keybindingStartPos.Y += keybindingSize.Y + keybindingSpacing.Y;

                return r;
            }
        }

        static void InitKeybindings()
        {
            optionElements[(int)OptionScreen.Keybindings] = new List<UIElement>();
            for (int i = 0; i < (int)KeyBindManager.KeyListner.Count; i++)
            {
                optionElements[(int)OptionScreen.Keybindings].Add(new KeybindingObject((KeyBindManager.KeyListner)i, GetKeybindingPos, keybindingSize));
            }

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

        public static void Draw(SpriteBatch aBatch)
        {
            for (int i = 0; i < optionScreenPermanents.Count; i++)
            {
                optionScreenPermanents[i].Draw(aBatch);
            }

            for (int i = 0; i < optionElements[(int)currentScreen].Count; i++)
            {
                optionElements[(int)currentScreen][i].Draw(aBatch);
            }
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
                bool clickedOn = optionElements[(int)currentScreen][i].ClickedOn(aClickEvent);
                if (clickedOn == true)
                {
                    return true;
                }
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
    }
}
