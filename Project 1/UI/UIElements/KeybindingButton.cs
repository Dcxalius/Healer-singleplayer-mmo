using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Project_1.Camera;
using Project_1.Input;
using Project_1.UI.OptionMenu;
using Project_1.UI.UIElements.Buttons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements
{
    internal class KeybindingButton : Button
    {
        bool firstButton;
        KeyBindManager.KeyListner keyListner;
        
        
        bool waitingForPress = false;
        public KeybindingButton(bool aFirstButtons, KeyBindManager.KeyListner aKeyListner, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aPos, aSize, Color.White, KeyBindManager.GetKey(aFirstButtons, aKeyListner).ToString(), Color.Black)
        {
            firstButton = aFirstButtons; 
            keyListner = aKeyListner;
        }

        public override void Update()
        {
            base.Update();

            WaitingForPress();
        }

        void WaitingForPress()
        {
            if (waitingForPress == true)
            {
                CheckForEscape();
                SetKey();
            }
        }

        void CheckForEscape()
        {
            if (InputManager.GetPress(Keys.Escape))
            {
                KeyBindManager.SetKey(firstButton, keyListner, Keys.None);
                waitingForPress = false;
                ButtonText = "None";
                OptionManager.AddActionToDoAtExitOfOptionMenu(KeyBindManager.SaveBindings);

            }
        }

        void SetKey()
        {
            Keys? newKey = InputManager.GetAnyKey;

            if (!newKey.HasValue)
            {
                return;
            }
            if (newKey.Value == Keys.Escape || InputManager.IsModifier(newKey.Value))
            {
                return;
            }

            if (!KeyBindManager.CheckForNoDupeKeys(newKey.Value, InputManager.CheckHoldModifiers()))
            {
                ButtonText = "Dupe key, try another key.";
                return;
            }

            KeyBindManager.SetKey(firstButton, keyListner, newKey.Value);
            ButtonText = KeyBindManager.GetKey(firstButton, keyListner).ToString();
            waitingForPress = false;

            OptionManager.AddActionToDoAtExitOfOptionMenu(KeyBindManager.SaveBindings);
        }

        public override void Close()
        {
            base.Close();
            ButtonText = KeyBindManager.GetKey(firstButton, keyListner).ToString();
            waitingForPress = false;
        }

        public override void ClickedOnAndReleasedOnMe()
        {
            base.ClickedOnAndReleasedOnMe();

            OptionManager.CloseAllOptionMenuStuff();
            ButtonText = "Waiting for key press.";
            waitingForPress = true;
        }
    }
}
