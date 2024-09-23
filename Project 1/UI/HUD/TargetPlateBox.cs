using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.GameObjects;
using Project_1.UI.UIElements.PlateBoxes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.HUD
{
    internal class TargetPlateBox : PlateBox
    {
        Entity targetEntity;
        
        static PlateBoxNameSegment nameSegment;

        public TargetPlateBox(Vector2 aPos, Vector2 aSize) : base(aPos, aSize)
        {
            nameSegment = new PlateBoxNameSegment(null, aPos, new Vector2(aSize.X, aSize.Y / 2));
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
            nameSegment.Name = targetEntity.Name;
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
