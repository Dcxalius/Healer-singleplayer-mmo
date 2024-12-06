using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.PlateBoxes
{
    internal abstract class PlateBox : Box
    {
        protected PlateBoxSegment[] leftVerticalSegments;
        protected PlateBoxSegment[] rightVerticalSegments;
        protected PlateBoxSegment[] horizontalSegments;

        public PlateBox(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new UITexture("GrayBackground", Color.White), aPos, aSize)
        {
            //TODO: Make sure the segmentsizes add up to aSize

        }

        protected void AddSegmentsToChildren()
        {

            children.AddRange(leftVerticalSegments);
            children.AddRange(rightVerticalSegments);
            children.AddRange(horizontalSegments);
        }

        public override void Draw(SpriteBatch aBatch)
        {
            base.Draw(aBatch);
        }
    }
}
