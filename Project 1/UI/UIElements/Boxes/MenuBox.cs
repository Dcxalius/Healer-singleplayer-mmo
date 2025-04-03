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
            float spacing = 0.05f;
            buttonSize = new RelativeScreenPosition(4f / 5f, 1f / 12f);
            buttonStartingPos = new RelativeScreenPosition(1f / 2f - buttonSize.X / 2, spacing);
            buttonStartingFromBottomPos = new RelativeScreenPosition(1f / 2f - buttonSize.X / 2, 1f - spacing - buttonSize.Y);
            buttonOffset = new RelativeScreenPosition(0, buttonSize.Y * 1.5f);
        }
    }
}
