using Microsoft.Xna.Framework;
using Project_1.Camera;
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

        public OptionScreenBox(int aCountOfButtons, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(null, aPos, aSize)
        {
            RelativeScreenPosition buttonSize = new RelativeScreenPosition(aSize.X / aCountOfButtons, aSize.Y);
            videoOptionsButton = new VideoOptionsButton(new RelativeScreenPosition(0), buttonSize);
            keybindingsOptionButton = new KeybindingsOptionButton(new RelativeScreenPosition(buttonSize.X, 0), buttonSize);

            children.Add(videoOptionsButton);  
            children.Add(keybindingsOptionButton);

            Debug.Assert(children.Count == aCountOfButtons, "Missing buttons in optionpagebox");
        }
    }
}
