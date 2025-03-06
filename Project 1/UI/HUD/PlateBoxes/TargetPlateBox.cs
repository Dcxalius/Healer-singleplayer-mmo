using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Entities.Players;
using Project_1.GameObjects.Unit;
using Project_1.Input;
using Project_1.Managers.States;
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
    internal class TargetPlateBox : PlateBox
    {

        Entity targetEntity;

        PlateBoxNameSegment nameSegment;
        PlateBoxHealthSegment healthSegment;
        PlateBoxResourceSegment resourceSegment;


        public TargetPlateBox(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aPos, aSize)
        {
            nameSegment = new PlateBoxNameSegment(null, Color.White, RelativeScreenPosition.Zero, new RelativeScreenPosition(aSize.X, aSize.Y / 2));
            healthSegment = new PlateBoxHealthSegment(new RelativeScreenPosition(0, aSize.Y / 2), new RelativeScreenPosition(aSize.X, aSize.Y / 4));
            resourceSegment = new PlateBoxResourceSegment(new RelativeScreenPosition(0, aSize.Y / 4 * 3), new RelativeScreenPosition(aSize.X, aSize.Y / 4));

            leftVerticalSegments = new PlateBoxSegment[] { };
            rightVerticalSegments = new PlateBoxSegment[] { };
            horizontalSegments = new PlateBoxSegment[] { nameSegment, healthSegment, resourceSegment };

            AddSegmentsToChildren();
        }


        public override void Refresh(Entity aEntity)
        {
            healthSegment.Refresh(aEntity);
            levelCircle.Refresh(aEntity);
            resourceSegment.Refresh(aEntity);
        }

        public bool BelongsTo(Entity aEntity)
        {
            return targetEntity == aEntity;

        }
        public void SetTarget(Entity aTarget) 
        {
            targetEntity = aTarget;
            if (targetEntity == null)
            {
                nameSegment.Name = null;
                return;
            }

            nameSegment.Refresh(targetEntity);
            healthSegment.SetTarget(targetEntity);
            levelCircle.Refresh(targetEntity);
            resourceSegment.SetTarget(targetEntity);
        }


        public override void Draw(SpriteBatch aBatch)
        {
            if (targetEntity == null) return;

            base.Draw(aBatch);
        }

    }
}
