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

       
        public PlateBox(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new UITexture("GrayBackground", Color.White), aPos, aSize)
        {
            RelativeScreenPosition levelCircleSize = RelativeScreenPosition.GetSquareFromX(0.05f, Size);
            //TODO: Make sure the segmentsizes add up to aSize
            levelCircle = new LevelCircle(new RelativeScreenPosition(1 - levelCircleSize.X, 0), levelCircleSize);
        }

        public abstract void Refresh(Entity aEntity);

        //public override void Resize(AbsoluteScreenPosition aSize) => Resize(aSize.ToRelativeScreenPosition());

        public override void Resize(RelativeScreenPosition aSize)
        {
            RelativeScreenPosition childRatio = aSize / RelativeSize;
            base.Resize(aSize);
            Action<UIElement> action = (child) => child.Resize(child.RelativeSize * childRatio);
            ForAllChildren(action);
        }


        protected void AddSegmentsToChildren()
        {
            AddChildren(leftVerticalSegments);
            AddChildren(rightVerticalSegments);
            AddChildren(horizontalSegments);
            AddChild(levelCircle);
        }
    }
}
