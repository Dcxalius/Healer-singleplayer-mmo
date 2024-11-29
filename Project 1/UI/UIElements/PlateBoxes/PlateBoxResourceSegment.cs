using Microsoft.Xna.Framework;
using Project_1.GameObjects.Entities;
using Project_1.Textures;
using Project_1.UI.UIElements.Bars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.PlateBoxes
{
    internal class PlateBoxResourceSegment : PlateBoxSegment
    {
        ResourceBar resourceBar;
        Entity entity;
        static Color backgroundColor = new Color(255, 211, 211, 120);
        public PlateBoxResourceSegment(Entity aEntity, Vector2 aPos, Vector2 aSize) : base(null, aPos, aSize)
        {
            entity = aEntity;
            if (entity != null)
            {
                resourceBar = new ResourceBar(new BarTexture(BarTexture.FillingDirection.Right, entity.ResourceColor), new UITexture("WhiteBackground", backgroundColor), Vector2.Zero, aSize);
                resourceBar.MaxValue = aEntity.MaxResource;
            }
            else
            {
                resourceBar = new ResourceBar(new BarTexture(BarTexture.FillingDirection.Right, Color.White), new UITexture("WhiteBackground", backgroundColor), Vector2.Zero, aSize);
            }
            children.Add(resourceBar);
        }

        public override void Update(in UIElement aParent)
        {
            base.Update(aParent);
            if (entity == null) { return; }
            resourceBar.Value = entity.CurrentResource;

        }

        public void SetTarget(Entity aEntity)
        {
            entity = aEntity;
            resourceBar.MaxValue = aEntity.MaxResource;
            resourceBar.Value = entity.CurrentResource;
            resourceBar.Color = entity.ResourceColor;
        }

    }
}
