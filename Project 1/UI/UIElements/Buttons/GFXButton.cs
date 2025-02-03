using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Input;
using Project_1.Managers;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.Buttons
{
    internal class GFXButton : Button
    {
        public UITexture GfxOnButton { get => gfxOnButton; }

        protected UITexture gfxOnButton;
        Rectangle gfxRectangle;



        public GFXButton(GfxPath aPath, RelativeScreenPosition aPos, RelativeScreenPosition aSize, Color aColorOfBorder) : base(aPos, aSize, aColorOfBorder)
        {
            gfxOnButton = new UITexture(aPath, Color.White);
        }

        public GFXButton(List<Action> aActions, GfxPath aPath, RelativeScreenPosition aPos, RelativeScreenPosition aSize, Color aColor, string aText = null, Color? aTextColor = null) : base(aActions, aPos, aSize, aColor, aText, aTextColor)
        {
            gfxOnButton = new UITexture(aPath, Color.White);
        }

        public override void Update(in UIElement aParent)
        {
            base.Update(aParent);

            gfxRectangle = ConstructGfxRect();

        }

        Rectangle ConstructGfxRect() 
        {
            Point pos = new Vector2(AbsolutePos.X + AbsolutePos.Size.X / 10, AbsolutePos.Y + AbsolutePos.Size.Y / 10).ToPoint();
            Point size = new Vector2(AbsolutePos.Size.X * 0.8f, AbsolutePos.Size.Y * 0.8f).ToPoint();

            return new Rectangle(pos, size);

        }


        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch aBatch)
        {
            base.Draw(aBatch);

            if (gfxOnButton == null) return;

            if (!Pressed)
            {
                gfxOnButton.Draw(aBatch, gfxRectangle);
            }
            else
            {
                gfxOnButton.Draw(aBatch, gfxRectangle, Color.DarkGray);

            }
        }

    }
}
