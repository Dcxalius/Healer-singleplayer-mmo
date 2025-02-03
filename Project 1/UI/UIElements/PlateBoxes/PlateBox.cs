using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Unit;
using Project_1.Textures;
using Project_1.UI.UIElements.Boxes;
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

        protected LevelCircle levelCircle;

        readonly RelativeScreenPosition levelCircleSize = RelativeScreenPosition.GetSquareFromX(0.015f);
        public PlateBox(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new UITexture("GrayBackground", Color.White), aPos, aSize)
        {

            //TODO: Make sure the segmentsizes add up to aSize
            levelCircle = new LevelCircle(new RelativeScreenPosition(aSize.X - levelCircleSize.X, 0), levelCircleSize);
        }

        public abstract void Refresh(Entity aEntity);

        

        protected void AddSegmentsToChildren()
        {
            AddChildren(leftVerticalSegments);
            AddChildren(rightVerticalSegments);
            AddChildren(horizontalSegments);
            AddChild(levelCircle);
        }
    }
}
