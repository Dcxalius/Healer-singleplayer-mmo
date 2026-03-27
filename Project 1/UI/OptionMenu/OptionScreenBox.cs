using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Textures;
using Project_1.UI.UIElements.Boxes;
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
        ChatOptionsButton chatOptionsButton;
        DebugOptionsButton debugOptionsButton;

        public OptionScreenBox(int aCountOfButtons, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(null, aPos, aSize)
        {
            RelativeScreenPosition buttonSize = new RelativeScreenPosition(1f / aCountOfButtons, 1);
            videoOptionsButton = new VideoOptionsButton(new RelativeScreenPosition(0), buttonSize, this);
            keybindingsOptionButton = new KeybindingsOptionButton(new RelativeScreenPosition(buttonSize.X, 0), buttonSize, this);
            chatOptionsButton = new ChatOptionsButton(new RelativeScreenPosition(buttonSize.X * 2, 0), buttonSize, this);
            debugOptionsButton = new DebugOptionsButton(new RelativeScreenPosition(buttonSize.X * 3, 0), buttonSize, this);

            AddChild(videoOptionsButton);  
            AddChild(keybindingsOptionButton);
            AddChild(chatOptionsButton);
            AddChild(debugOptionsButton);

            Debug.Assert(ChildCount == aCountOfButtons, "Missing buttons in optionpagebox");
        }
    }
}
