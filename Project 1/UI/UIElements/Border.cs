using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements
{
    internal class Border : UIElement
    {
        public bool Visible { get => visible; set => visible = value; }

        bool visible = true;
        public Border(Color aColor, Vector2 aPos, Vector2 aSize) : base(new UITexture("GrayWhiteBorder", aColor), aPos, aSize)
        {

        }

        public override void Draw(SpriteBatch aBatch)
        {
            if (!visible) return;
            base.Draw(aBatch);
        }
    }
}
