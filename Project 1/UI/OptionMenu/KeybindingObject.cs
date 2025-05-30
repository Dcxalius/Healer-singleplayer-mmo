﻿using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Input;
using Project_1.Textures;
using Project_1.UI.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.OptionMenu
{
    internal class KeybindingObject : UIElement
    {
        Label nameOfButton;
        KeybindingButton firstKeybindingButton;
        KeybindingButton secondKeybindingButton;

        const float spacingX = 0.03f;

        public KeybindingObject(KeyBindManager.KeyListner aListner, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new UITexture("WhiteBackground", Color.Wheat), aPos, aSize)
        {
            nameOfButton = new Label(aListner.ToString(), RelativeScreenPosition.Zero, new RelativeScreenPosition(1f / 3, 1f), Label.TextAllignment.Centred, Color.Black);
            firstKeybindingButton = new KeybindingButton(true, aListner, new RelativeScreenPosition(1f/3, 0), new RelativeScreenPosition(1f / 3, 1f));
            secondKeybindingButton = new KeybindingButton(false, aListner, new RelativeScreenPosition(1f * 2 / 3 + spacingX, 0), new RelativeScreenPosition(1f / 3, 1f));

            AddChild(nameOfButton);
            AddChild(firstKeybindingButton);
            AddChild(secondKeybindingButton);
        }
    }
}
