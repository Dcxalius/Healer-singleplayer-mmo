using Microsoft.Xna.Framework;
using Project_1.GameObjects;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.PlateBoxes
{
    internal class PlateBox : Box
    {
        Entity targetEntity;

        PlateBoxSegment[] horizontalSegments; 
        PlateBoxSegment[] verticalSegments;

        public PlateBox(ref Rectangle aParentPos, PlateBoxSegment[] aSetOfHorizonalSegments, PlateBoxSegment[] aSetOfVerticalSegments, Vector2 aPos, Vector2 aSize) : base(ref aParentPos, new UITexture("GreyBackground", Color.White), aPos, aSize)
        {
            //TODO: Make sure the segmentsizes doesnt add up to > aSize


        }

        
    }
}
