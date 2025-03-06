using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Textures;
using Project_1.UI.UIElements.Boxes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.LoadingMenu
{
    internal class LoadingBox : MenuBox
    {
        public LoadingBox(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new UITexture("GrayBackground", Color.Orange), aPos, aSize)
        {
            RelativeScreenPosition square = RelativeScreenPosition.GetSquareFromY(0.05f);
            AddChild(new ExitButton(new RelativeScreenPosition(0.9f, 0.9f) - square, square));
        }
    }
}
