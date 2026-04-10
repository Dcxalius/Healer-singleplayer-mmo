using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.Textures;
using Project_1.UI.UIElements.Boxes;
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


        public Bar(UIElement aParent, BarTexture aBarGfx, UITexture aBackgroundGfx, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aParent, aBackgroundGfx, aPos, aSize)
        {
            barComponent = new BarComponent(this, aBarGfx, RelativeScreenPosition.Zero, RelativeScreenPosition.One);
        }
    }
}
