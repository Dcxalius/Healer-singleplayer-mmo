using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
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
        public new Color Color { get => barComponent.Color; set => barComponent.Color = value; }
        public virtual float Value
        {
            set { barComponent.UpdateBar(value, RelativeSize.X); }
        }

        BarComponent barComponent;


        public Bar(BarTexture aBarGfx, UITexture aBackgroundGfx, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aBackgroundGfx, aPos, aSize)
        {
            barComponent = new BarComponent(aBarGfx, RelativeScreenPosition.Zero, aSize);
            AddChild(barComponent);
        }
        public override void Draw(SpriteBatch aBatch)
        {
            base.Draw(aBatch);
        }
    }
}
