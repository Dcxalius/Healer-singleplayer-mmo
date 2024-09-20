using Microsoft.Xna.Framework;
using Project_1.GameObjects;
using Project_1.Textures;
using Project_1.UI.UIElements.PlateBoxes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Project_1.UI.HUD
{
    internal class PlayerPlateBox : PlateBox
    {
        static Player p = ObjectManager.Player;

        

        static PlateBoxNameSegment name;

        public PlayerPlateBox(Vector2 aPos, Vector2 aSize) : base(aPos, aSize)
        { 
            name = new PlateBoxNameSegment(p.Name, new Vector2(0), new Vector2(aSize.X, aSize.Y / 2));



            leftVerticalSegments = new PlateBoxSegment[] { };
            rightVerticalSegments = new PlateBoxSegment[] { };
            horizontalSegments = new PlateBoxSegment[] { name };

            AddSegmentsToChildren();

        }
    }
}
