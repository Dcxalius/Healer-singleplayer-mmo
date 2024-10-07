using Microsoft.Xna.Framework;
using Project_1.Textures;
using Project_1.UI.UIElements;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.OptionMenu
{
    internal class OptionScreenBox : Box
    {
        VideoOptionsButton videoOptionsButton;
        KeybindingsOptionButton keybindingsOptionButton;

        public OptionScreenBox(int aCountOfButtons, Vector2 aPos, Vector2 aSize) : base(null, aPos, aSize)
        {
            Vector2 buttonSize = new Vector2(aSize.X / aCountOfButtons, aSize.Y);
            videoOptionsButton = new VideoOptionsButton(new Vector2(0), buttonSize);
            keybindingsOptionButton = new KeybindingsOptionButton(new Vector2(buttonSize.X, 0), buttonSize);

            children.Add(videoOptionsButton);  
            children.Add(keybindingsOptionButton);

            Debug.Assert(children.Count == aCountOfButtons, "Missing buttons in optionpagebox");
        }
    }
}
