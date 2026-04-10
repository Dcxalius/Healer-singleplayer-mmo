using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Messaging.Events;
using Project_1.Textures;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Boxes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.HUD.PlateBoxes
{
    internal abstract class PlateBox : Box
    {
        protected PlateBoxSegment[] leftVerticalSegments;
        protected PlateBoxSegment[] rightVerticalSegments;
        protected PlateBoxSegment[] horizontalSegments;

        protected LevelCircle levelCircle;


        public PlateBox(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(null, new UITexture("GrayBackground", Color.White), aPos, aSize)
        {
            RelativeScreenPosition levelCircleSize = RelativeScreenPosition.GetSquareFromX(0.05f, Size);
            //TODO: Make sure the segmentsizes add up to 1, handle pixel rounding by checking the total of the segment and then adding the missing pixels to the biggest segment or something like that

            levelCircle = new LevelCircle(this, new RelativeScreenPosition(1 - levelCircleSize.X, 0), levelCircleSize);
        }

        public abstract void Refresh(in EntityUiSnapshot snapshot);

        protected void AddSegmentsToChildren()
        {
            //AddChildren(leftVerticalSegments);
            //AddChildren(rightVerticalSegments);
            //AddChildren(horizontalSegments);
        }

    }
}
