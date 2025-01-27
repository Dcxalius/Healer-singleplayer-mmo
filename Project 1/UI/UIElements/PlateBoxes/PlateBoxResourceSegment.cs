using Microsoft.Xna.Framework;
using Project_1.Camera;
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
        static Color backgroundColor = new Color(255, 211, 211, 120);
        public PlateBoxResourceSegment(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(null, aPos, aSize)
        {
            resourceBar = new ResourceBar(new BarTexture(BarTexture.FillingDirection.Right, Color.White), new UITexture("WhiteBackground", backgroundColor), RelativeScreenPosition.Zero, aSize);
            
            AddChild(resourceBar);
        }

        public override void Refresh(Entity aEntity)
        {
            resourceBar.Value = aEntity.CurrentResource;
            resourceBar.MaxValue = aEntity.MaxResource;
        }

        public void SetTarget(Entity aEntity)
        {
            resourceBar.MaxValue = aEntity.MaxResource;
            resourceBar.Value = aEntity.CurrentResource;
            resourceBar.Color = aEntity.ResourceColor;
        }

    }
}
