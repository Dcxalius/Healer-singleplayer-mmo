using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.GameObjects;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Project_1.UI.UIElements.Bars
{
    internal class Bar : Box
    {
        public virtual float Value
        {
            set { barComponent.UpdateBar(value); }
        }

        BarComponent barComponent;
        

        public Bar(float aMaxValue, BarTexture aBarGfx, UITexture aBackgroundGfx, Vector2 aPos, Vector2 aSize) : base(aBackgroundGfx, aPos, aSize)
        {
            barComponent = new BarComponent(aBarGfx, Vector2.Zero, aSize);
            children.Add(barComponent);
        }
        
        public override void Draw(SpriteBatch aBatch)
        {
            base.Draw(aBatch);
        }
    }
}
