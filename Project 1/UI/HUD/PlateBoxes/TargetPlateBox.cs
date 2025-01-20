using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Entities.Players;
using Project_1.GameObjects.Entities.Temp;
using Project_1.GameObjects.Unit;
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
    internal class TargetPlateBox : PlateBox
    {

        Entity targetEntity;

        PlateBoxNameSegment nameSegment;
        PlateBoxHealthSegment healthSegment;
        PlateBoxResourceSegment resourceSegment;


        public TargetPlateBox(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aPos, aSize)
        {
            nameSegment = new PlateBoxNameSegment(null, Color.White, RelativeScreenPosition.Zero, new RelativeScreenPosition(aSize.X, aSize.Y / 2));
            healthSegment = new PlateBoxHealthSegment(null, new RelativeScreenPosition(0, aSize.Y / 2), new RelativeScreenPosition(aSize.X, aSize.Y / 4));
            resourceSegment = new PlateBoxResourceSegment(null, new RelativeScreenPosition(0, aSize.Y / 4 * 3), new RelativeScreenPosition(aSize.X, aSize.Y / 4));

            leftVerticalSegments = new PlateBoxSegment[] { };
            rightVerticalSegments = new PlateBoxSegment[] { };
            horizontalSegments = new PlateBoxSegment[] { nameSegment, healthSegment, resourceSegment };

            AddSegmentsToChildren();
        }

        public override void Update(in UIElement aParent)
        {
            base.Update(aParent);

            if (ObjectManager.Player.Target == null) { return; }
            if (ObjectManager.Player.Target.CurrentHealth <= 0) { SetEntity(); }
        }

        public override void Refresh(Entity aEntity)
        {
            nameSegment.Name = aEntity.Name;
            healthSegment.Refresh(aEntity);
            resourceSegment.Refresh(aEntity);
        }

        public bool BelongsTo(Entity aEntity)
        {
            return targetEntity == aEntity;

        }
        public void SetEntity() 
        {
            Entity target = ObjectManager.Player.Target;//Todo: Generalize this
            if (target == null)
            {
                nameSegment.Name = null;
                return;
            }
            targetEntity = target;
            nameSegment.Name = target.Name;
            nameSegment.BackgroundColor = target.RelationColor;
            healthSegment.SetTarget(target);
            resourceSegment.SetTarget(target);
        }


        public override void Draw(SpriteBatch aBatch)
        {
            if (ObjectManager.Player.Target == null) return;

            base.Draw(aBatch);
        }

    }
}
