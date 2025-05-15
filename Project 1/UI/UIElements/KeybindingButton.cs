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
        bool buttonLevel;
        KeyBindManager.KeyListner keyListner;
        
        
        bool waitingForPress = false;
        public KeybindingButton(bool aButtonLevel, KeyBindManager.KeyListner aKeyListner, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aPos, aSize, Color.White, KeyBindManager.GetKey(aButtonLevel, aKeyListner).ToString(), Color.Black)
        {
            buttonLevel = aButtonLevel; 
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
                ActualKey();
            }
        }

        void CheckForEscape()
        {
            if (InputManager.GetPress(Keys.Escape))
            {
                KeySet keySet = KeyBindManager.GetKey(buttonLevel, keyListner);

                waitingForPress = false;
                ButtonText = "None";
                OptionManager.AddActionToDoAtExitOfOptionMenu(() => SetKey(buttonLevel, keyListner, keySet), () => SetKey(buttonLevel, keyListner, Keys.None));
                OptionManager.AddFinalActions(KeyBindManager.SaveBindings);

            }
        }

        void SetKey(bool aButtonLevel, KeyBindManager.KeyListner aListner, KeySet aKey)
        {
            KeyBindManager.SetKey(aButtonLevel, aListner, aKey);

            ButtonText = KeyBindManager.GetKey(buttonLevel, keyListner).ToString();
        }

        void ActualKey()
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

            KeySet keySet = new KeySet(newKey.Value, InputManager.CheckHoldModifiers());

            if (!KeyBindManager.CheckForNoDupeKeys(keySet))
            {
                ButtonText = "Dupe key, try another key.";
                return;
            }

            KeySet oldKeySet = KeyBindManager.GetKey(buttonLevel, keyListner);
            KeyBindManager.SetKey(buttonLevel, keyListner, keySet);
            ButtonText = KeyBindManager.GetKey(buttonLevel, keyListner).ToString();
            waitingForPress = false;

            OptionManager.AddActionToDoAtExitOfOptionMenu(() => SetKey(buttonLevel, keyListner, oldKeySet), () => SetKey(buttonLevel, keyListner, keySet));
            OptionManager.AddFinalActions(KeyBindManager.SaveBindings);
        }

        public override void Close()
        {
            base.Close();
            ButtonText = KeyBindManager.GetKey(buttonLevel, keyListner).ToString();
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
