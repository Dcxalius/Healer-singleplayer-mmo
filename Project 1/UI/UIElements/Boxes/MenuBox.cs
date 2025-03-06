using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.Boxes
{
    internal class MenuBox : Box
    {
        

        RelativeScreenPosition buttonSize;
        RelativeScreenPosition buttonStartingPos;
        RelativeScreenPosition buttonStartingFromBottomPos;
        RelativeScreenPosition buttonOffset;
        int buttonIndex = 0;
        int buttonIndexFromBottom = 0;


        protected RelativeScreenPosition GetStartPositionFromTop
        {
            get
            {
                return buttonStartingPos + buttonOffset * buttonIndex++;
                
            }
        }

        protected RelativeScreenPosition GetStartPositionFromBottom
        {
            get
            {
                return buttonStartingFromBottomPos - buttonOffset * buttonIndexFromBottom++;
            }
        }

        protected RelativeScreenPosition ButtonSize => buttonSize;

        public MenuBox(UITexture aGfx, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aGfx, aPos, aSize)
        {
            //pauseSize = new RelativeScreenPosition(0.2f, 0.5f);
            //pausePos = new RelativeScreenPosition(0.5f - (pauseSize.X / 2), 0.5f - pauseSize.Y / 2);
            UITexture staticGfx = new UITexture("WhiteBackground", Color.DarkGray);
            buttonSize = new RelativeScreenPosition(aSize.X / 5 * 4, aSize.Y / 12);
            buttonStartingPos = new RelativeScreenPosition(aSize.X / 2 - buttonSize.X / 2, 0.05f);
            buttonStartingFromBottomPos = new RelativeScreenPosition(aSize.X / 2 - buttonSize.X / 2, aSize.Y - 0.05f - buttonSize.Y);
            buttonOffset = new RelativeScreenPosition(0, buttonSize.Y * 1.5f);
        }
    }
}
