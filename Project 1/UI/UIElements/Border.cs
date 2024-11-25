using Microsoft.Xna.Framework;
using Project_1.Input;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements
{
    internal class Border : Box
    {
        public Border(Vector2 aPos, Vector2 aSize) : base(new UITexture("GrayWhiteBorder", Color.White), aPos, aSize)
        {
        }

        public override bool ClickedOn(ClickEvent aClick)
        {
            return false;
        }

        public override bool ReleasedOn(ReleaseEvent aRelease)
        {
            return false;
        }
    }
}
