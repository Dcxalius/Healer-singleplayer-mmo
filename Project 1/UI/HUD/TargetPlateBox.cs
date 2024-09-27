using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.GameObjects;
using Project_1.UI.UIElements.PlateBoxes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Project_1.UI.HUD
{
    internal class TargetPlateBox : PlateBox
    {
        Entity targetEntity;
        
        static PlateBoxNameSegment nameSegment;
        static PlateBoxHealthSegment healthSegment;
        public TargetPlateBox(Vector2 aPos, Vector2 aSize) : base(aPos, aSize)
        {
            nameSegment = new PlateBoxNameSegment(null, Color.White, Vector2.Zero, new Vector2(aSize.X, aSize.Y / 2));
            healthSegment = new PlateBoxHealthSegment(null, new Vector2(0, aSize.Y / 2), new Vector2(aSize.X, aSize.Y / 4));


            leftVerticalSegments = new PlateBoxSegment[] { };
            rightVerticalSegments = new PlateBoxSegment[] { };
            horizontalSegments = new PlateBoxSegment[] { nameSegment, healthSegment };

            AddSegmentsToChildren();
        }

        public void SetEntity(Entity aEntity)
        {
            if (aEntity == null)
            {
                targetEntity = null;
                nameSegment.Name = null;
                return;
            }
            targetEntity = aEntity;
            nameSegment.Name = targetEntity.Data.Name;
            nameSegment.BackgroundColor = aEntity.RelationColor;
            healthSegment.SetTarget(targetEntity);
        }

        public override void Draw(SpriteBatch aBatch)
        {
            if (targetEntity == null)
            {
                return;
            }

            base.Draw(aBatch);
        }

    }
}
