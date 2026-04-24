using Project_1.Camera;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.Boxes
{
    internal class Square : Box
    {
        enum SquareSize
        {
            FromX,
            FromY
        }

        SquareSize sizeFrom;
        AbsoluteScreenPosition ContextSize => parent == null ? Camera.Camera.WindowSize : parent.Size;

        public Square(UIElement aParent, UITexture aGfx, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aParent, aGfx, aPos, aSize)
        {
            Debug.Assert(aSize.X == 0 || aSize.Y == 0, "Only give one dimension");
            if (aSize.X != 0)
            {
                sizeFrom = SquareSize.FromX;
            }
            else
            {
                sizeFrom = SquareSize.FromY;
            }
            Resize(aSize.X + aSize.Y);
        }

        public void Resize(float aNewSize)
        {
            if (sizeFrom == SquareSize.FromX)
            {
                Resize(RelativeScreenPosition.GetSquareFromX(aNewSize, ContextSize));
            }
            else
            {
                Resize(RelativeScreenPosition.GetSquareFromY(aNewSize, ContextSize));
            }
        }

        override public void Resize(RelativeScreenPosition aNewSize)
        {
            RelativeScreenPosition newSize = sizeFrom == SquareSize.FromX ? RelativeScreenPosition.GetSquareFromX(aNewSize.X, ContextSize) : RelativeScreenPosition.GetSquareFromY(aNewSize.Y, ContextSize);
            base.Resize(newSize);
        }
    }
}
