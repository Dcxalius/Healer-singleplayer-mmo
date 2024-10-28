using Microsoft.Xna.Framework;
using Project_1.Input;
using Project_1.Managers;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements
{
    internal abstract class GFXButton : Button
    {
        UITexture gfxOnButton;
        Rectangle gfxRectangle;
        Rectangle xdd;

        public GFXButton(GfxPath aPath, Vector2 aPos, Vector2 aSize, Color aColorOfBorder) : base(aPos, aSize, aColorOfBorder)
        {
            gfxOnButton = new UITexture(aPath, Color.White);
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch aBatch)
        {
            base.Draw(aBatch);

            if (!Pressed)
            {
                gfxOnButton.Draw(aBatch, constructGfxRect());
            }
            else
            {
                gfxOnButton.Draw(aBatch, constructGfxRect(), Color.DarkGray);

            }
        }

        Rectangle constructGfxRect()
        {
            Point pos = new Vector2(AbsolutePos.X + AbsolutePos.Size.X / 10, AbsolutePos.Y + AbsolutePos.Size.Y / 10).ToPoint();
            Point size = new Vector2(AbsolutePos.Size.X * 0.8f, AbsolutePos.Size.Y * 0.8f).ToPoint();

            return new Rectangle(pos, size);

        }

        public override void Rescale()
        {
            base.Rescale();

        }
    }
}
