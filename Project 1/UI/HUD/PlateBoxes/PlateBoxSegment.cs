using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Messaging.Events;
using Project_1.Textures;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Bars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.HUD.PlateBoxes
{
    internal abstract class PlateBoxSegment : UIElement
    {
        public virtual Color BackgroundColor { set => gfx.Color = value; }
        protected ResourceBar bar;

        public PlateBoxSegment(UIElement aParent, Color aBackgroundColor, RelativeScreenPosition aPos, RelativeScreenPosition aSize, BarTexture.FillingDirection fillingDirection = BarTexture.FillingDirection.Right) : base(aParent, new UITexture("WhiteBackground", aBackgroundColor), aPos, aSize)
        {
            bar = new ResourceBar(this, new BarTexture(fillingDirection, Color.Transparent), null, RelativeScreenPosition.Zero, RelativeScreenPosition.One);

            bar.Visible = false;
            
            capturesClick = false;
        }

        public abstract void Clear();
        public abstract void Refresh(in EntityUiSnapshot snapshot);
    }
}
