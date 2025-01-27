using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects.Entities;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.PlateBoxes
{
    internal abstract class PlateBoxSegment : UIElement
    {
        protected string Text { get => text.Value; set => text.Value = value; }

        Text text;

        public PlateBoxSegment(UITexture aGfx, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aGfx, aPos, aSize)
        {
            text = new Text("Gloryse", Color.Black);
            
        }


        public abstract void Refresh(Entity aEntity);

        public override void Draw(SpriteBatch aBatch, float aLayer)
        {
            base.Draw(aBatch, aLayer);

            text.CentredDraw(aBatch, new AbsoluteScreenPosition(AbsolutePos.Center), aLayer + 0.01f);
        }
    }
}
