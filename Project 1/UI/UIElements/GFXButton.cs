using Microsoft.Xna.Framework;
using Project_1.Camera;
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
        public UITexture GfxOnButton { get => gfxOnButton; }

        protected UITexture gfxOnButton;
        //Rectangle gfxRectangle;

        public GFXButton(GfxPath aPath, RelativeScreenPosition aPos, RelativeScreenPosition aSize, Color aColorOfBorder) : base(aPos, aSize, aColorOfBorder)
        {
            gfxOnButton = new UITexture(aPath, Color.White);
        }

        
        Rectangle ConstructGfxRect() //TODO: Make this not calculate every update
        {
            Point pos = new Vector2(AbsolutePos.X + AbsolutePos.Size.X / 10, AbsolutePos.Y + AbsolutePos.Size.Y / 10).ToPoint();
            Point size = new Vector2(AbsolutePos.Size.X * 0.8f, AbsolutePos.Size.Y * 0.8f).ToPoint();

            return new Rectangle(pos, size);

        }


        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch aBatch, float aLayer)
        {
            base.Draw(aBatch, aLayer);

            if (gfxOnButton == null) return;

            if (!Pressed)
            {
                gfxOnButton.Draw(aBatch, ConstructGfxRect(), aLayer + 0.1f);
            }
            else
            {
                gfxOnButton.Draw(aBatch, ConstructGfxRect(), Color.DarkGray, aLayer + 0.1f);

            }
        }

    }
}
