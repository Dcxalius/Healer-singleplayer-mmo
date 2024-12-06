using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Project_1.Camera;
using Project_1.Input;
using Project_1.UI.OptionMenu;
using Project_1.UI.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements
{
    internal class KeybindingButton : Button
    {
        bool firstButtons;
        KeyBindManager.KeyListner keyListner;
        
        
        bool waitingForPress = false;
        public KeybindingButton(bool aFirstButtons, KeyBindManager.KeyListner aKeyListner, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aPos, aSize, Color.White, KeyBindManager.GetKey(aFirstButtons, aKeyListner).ToString(), Color.Black)
        {
            firstButtons = aFirstButtons; 
            keyListner = aKeyListner;
        }

        public override void Update(in UIElement aParent)
        {
            base.Update(aParent);

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
                KeyBindManager.SetKey(firstButtons, keyListner, Keys.None);
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
            if (newKey.Value == Keys.Escape)
            {
                return;
            }

            if (!KeyBindManager.CheckForNoDupeKeys(newKey.Value))
            {
                ButtonText = "Dupe key, try another key.";
                return;
            }

            ButtonText = newKey.Value.ToString();
            KeyBindManager.SetKey(firstButtons, keyListner, newKey.Value);
            waitingForPress = false;

            OptionManager.AddActionToDoAtExitOfOptionMenu(KeyBindManager.SaveBindings);
        }

        public override void Close()
        {
            base.Close();
            ButtonText = KeyBindManager.GetKey(firstButtons, keyListner).ToString();
            waitingForPress = false;
        }

        public override void HoldReleaseOnMe()
        {
            base.HoldReleaseOnMe();

            OptionManager.CloseAllOptionMenuStuff();
            ButtonText = "Waiting for key press.";
            waitingForPress = true;
        }
    }
}
