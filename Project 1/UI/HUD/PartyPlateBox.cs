using Microsoft.Xna.Framework;
using Project_1.GameObjects;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.PlateBoxes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Project_1.UI.HUD
{
    internal class PartyPlateBox : PlateBox
    {
        public bool VisibleBorder { get => border.Visible; set => border.Visible = value; }
        static Walker walker;

        static PlateBoxNameSegment name;
        static PlateBoxHealthSegment health;

        Border border;

        public PartyPlateBox(Walker aWalker, Vector2 aPos, Vector2 aSize) : base(aPos, aSize)
        {
            walker = aWalker;

            name = new PlateBoxNameSegment(walker.Name, walker.RelationColor, new Vector2(0, 0), new Vector2(aSize.X, aSize.Y / 2));
            health = new PlateBoxHealthSegment(walker, new Vector2(0, aSize.Y / 2), new Vector2(aSize.X, aSize.Y / 4));
            //health = new PlateBoxHealthSegment(walker, new Vector2(0, 0), new Vector2(aSize.X, aSize.Y / 2));


            leftVerticalSegments = new PlateBoxSegment[] { };
            rightVerticalSegments = new PlateBoxSegment[] { };
            horizontalSegments = new PlateBoxSegment[] { name, health };

            AddSegmentsToChildren();

            border = new Border(Color.YellowGreen, Vector2.Zero, aSize);
            children.Add(border);
            VisibleBorder = false;
        }


        public bool BelongsTo(Walker aWalker)
        {
            return aWalker == walker;
            
        }
    }
}
