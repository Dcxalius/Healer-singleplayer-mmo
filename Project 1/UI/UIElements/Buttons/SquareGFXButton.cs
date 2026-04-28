using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Project_1.UI.UIElements.Buttons
{
    internal class SquareGFXButton : GFXButton
    {
        enum SquareSize
        {
            FromX,
            FromY
        }

        readonly SquareSize sizeFrom;

        public SquareGFXButton(UIElement aParent, List<Action> aActions, GfxPath aPath, RelativeScreenPosition aPos, RelativeScreenPosition aSize, Color aColor, string aText = null, Color? aTextColor = null)
            : base(aParent, aActions, aPath, aPos, aSize, aColor, aText, aTextColor)
        {
            Debug.Assert(aSize.X == 0 || aSize.Y == 0, "Only give one dimension");
            sizeFrom = aSize.X != 0 ? SquareSize.FromX : SquareSize.FromY;
            Resize(aSize.X + aSize.Y);
        }

        public void Resize(float aNewSize)
        {
            if (sizeFrom == SquareSize.FromX)
            {
                Resize(RelativeScreenPosition.GetSquareFromX(aNewSize, ParentSize));
                return;
            }

            Resize(RelativeScreenPosition.GetSquareFromY(aNewSize, ParentSize));
        }

        public override void Resize(RelativeScreenPosition aSize)
        {
            RelativeScreenPosition newSize = sizeFrom == SquareSize.FromX
                ? RelativeScreenPosition.GetSquareFromX(aSize.X, ParentSize)
                : RelativeScreenPosition.GetSquareFromY(aSize.Y, ParentSize);

            base.Resize(newSize);
        }
    }
}
