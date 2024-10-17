using Microsoft.Xna.Framework;
using Project_1.Input;
using Project_1.UI.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.OptionMenu
{
    internal class KeybindingButton : Button
    {
        bool firstButtons;
        KeyBindManager.KeyListner keyListner;
        
        
        bool waitingForPress = false;
        public KeybindingButton(bool aFirstButtons, KeyBindManager.KeyListner aKeyListner, Vector2 aPos, Vector2 aSize, Color aColor) : base(aPos, aSize, aColor, KeyBindManager.GetKey(aFistButtons, aKeyListner).ToString(), Color.Gray)
        {
            firstButtons = aFirstButtons; 
            keyListner = aKeyListner;
        }

        public override void Update(in UIElement aParent)
        {
            base.Update(aParent);

            if (waitingForPress == true)
            {
                if (InputManager.GetPress(Microsoft.Xna.Framework.Input.Keys.Escape))
                {
                    waitingForPress = false;
                    KeyBindManager.SetKey(firstButtons, keyListner, null);
                }

                //TODO: Get Any press
            }
        }

        public override void HoldReleaseOnMe()
        {
            base.HoldReleaseOnMe();

            waitingForPress = true;
        }
    }
}
