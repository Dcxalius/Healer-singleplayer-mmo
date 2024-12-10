using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements
{
    internal class Label : UIElement
    {
        Text underlyingText;
        static readonly Color defaultC = Color.White;
        public Label(string aText, RelativeScreenPosition aPos, RelativeScreenPosition aSize, Color? aColor = null, string aFontname = "Gloryse") : base(null, aPos, aSize)
        {
            if (aColor.HasValue)
            {
                underlyingText = new Text(aFontname, aText, aColor.Value);
            }
            else
            {
                underlyingText = new Text(aFontname, aText);

            }
        }

        public override void Draw(SpriteBatch aBatch)
        {
            underlyingText.LeftAllignedDraw(aBatch, new AbsoluteScreenPosition( AbsolutePos.Location + new Vector2(0, AbsolutePos.Size.Y / 2).ToPoint()));

            base.Draw(aBatch);


        }
    }
}
