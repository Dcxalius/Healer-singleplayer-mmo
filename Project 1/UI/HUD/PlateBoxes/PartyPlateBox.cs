using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Entities.Temp;
using Project_1.Input;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.PlateBoxes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Project_1.UI.HUD.PlateBoxes
{
    internal class PartyPlateBox : PlateBox
    {
        public bool VisibleBorder { get => border.Visible; set => border.Visible = value; }
        static Walker walker;

        static PlateBoxNameSegment name;
        static PlateBoxHealthSegment health;
        static PlateBoxResourceSegment resource;

        CommandBorder border;

        public PartyPlateBox(Walker aWalker, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aPos, aSize)
        {
            walker = aWalker;

            name = new PlateBoxNameSegment(walker.Name, walker.RelationColor, new RelativeScreenPosition(0, 0), new RelativeScreenPosition(aSize.X, aSize.Y / 2));
            health = new PlateBoxHealthSegment(walker, new RelativeScreenPosition(0, aSize.Y / 2), new RelativeScreenPosition(aSize.X, aSize.Y / 4));
            resource = new PlateBoxResourceSegment(walker, new RelativeScreenPosition(0, aSize.Y / 4 * 3), new RelativeScreenPosition(aSize.X, aSize.Y / 4));

            //health = new PlateBoxHealthSegment(walker, new Vector2(0, 0), new Vector2(aSize.X, aSize.Y / 2));


            leftVerticalSegments = new PlateBoxSegment[] { };
            rightVerticalSegments = new PlateBoxSegment[] { };
            horizontalSegments = new PlateBoxSegment[] { name, health, resource };

            AddSegmentsToChildren();

            border = new CommandBorder(Color.YellowGreen, RelativeScreenPosition.Zero, aSize);
            children.Add(border);
            VisibleBorder = false;
        }


        public bool BelongsTo(Walker aWalker)
        {
            return aWalker == walker;

        }


        public override void HoldReleaseOnMe()
        {
            base.HoldReleaseOnMe();

            ObjectManager.Player.SetTarget(walker);
        }

        protected override bool ClickedOnChildren(ClickEvent aClick)
        {
            return false;
        }

        protected override void ClickedOnMe(ClickEvent aClick)
        {
            if (aClick.Modifier(InputManager.HoldModifier.Shift))
            {
                ObjectManager.Player.AddToCommand(walker);
                return;
            }
            if (aClick.Modifier(InputManager.HoldModifier.Ctrl))
            {
                ObjectManager.Player.NeedyAddToCommand(walker);
                return;
            }
            base.ClickedOnMe(aClick);
        }

        public override void Refresh(Entity aEntity)
        {
            name.Name = aEntity.Name;
            health.Refresh(aEntity);
            resource.Refresh(aEntity);
            levelCircle.Refresh(aEntity);
        }
    }
}
