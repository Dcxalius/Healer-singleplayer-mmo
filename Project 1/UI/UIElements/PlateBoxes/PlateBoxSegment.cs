using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        protected string Text //TODO: Bring this out to a class called TextButton
        {
            get => textToDraw;
            set
            {
                textToDraw = value;
                if (value == null) return;
                textSize = font.MeasureString(textToDraw);
            }
        }

        string textToDraw;
        Vector2 textSize;
        SpriteFont font;



        public PlateBoxSegment(UITexture aGfx, Vector2 aPos, Vector2 aSize) : base(aGfx, aPos, aSize)
        {
            font = TextureManager.GetFont("Gloryse");
        }

        public override void Draw(SpriteBatch aBatch)
        {
            base.Draw(aBatch);

            if (textToDraw != null)
            {
                aBatch.DrawString(font, textToDraw, new Vector2(AbsolutePos.X + Size.X / 2 , AbsolutePos.Y + Size.Y / 2 ), Color.White, 0f, textSize / 2, 1f, SpriteEffects.None, 1f);
            }
        }
    }
}
