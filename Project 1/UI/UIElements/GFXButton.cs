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
        Texture gfx;
        Rectangle gfxRectangle;

        public GFXButton(GfxPath aGfxPath, Vector2 aPos, Vector2 aSize, Color aColorOfBorder) : base(aPos, aSize, aColorOfBorder)
        {
            gfx = new Texture(aGfxPath);
            gfxRectangle = TransformFromRelativeToValues(new Vector2(RelativePos.X + RelativeSize.X / 10, RelativePos.Y + RelativeSize.Y / 10), new Vector2(RelativeSize.X * 0.8f, RelativeSize.Y * 0.8f));
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch aBatch)
        {
            base.Draw(aBatch);

            if (!Pressed)
            {
                gfx.Draw(aBatch, gfxRectangle);
            }
            else
            {
                gfx.Draw(aBatch, gfxRectangle, Color.DarkGray);

            }
        }

        public override void Update(in UIElement aParent)
        {
            base.Update(aParent);

            //DebugManager.Print(GetType(), "Pos: " + pos);
            //DebugManager.Print(GetType(), "MousePos: " + InputManager.GetMousePosAbsolute());
        }

        public override void Rescale()
        {
            base.Rescale();

            gfxRectangle = TransformFromRelativeToValues(new Vector2(RelativePos.X + RelativeSize.X / 10, RelativePos.Y + RelativeSize.Y / 10), new Vector2(RelativeSize.X * 0.8f, RelativeSize.Y * 0.8f));
            //DebugManager.Print(GetType(), "Pos: " + pos);
            //DebugManager.Print(GetType(), "gfxRec: " + gfxRectangle);
        }

        public override void ClickedOnMe(ClickEvent aClick)
        {
            base.ClickedOnMe(aClick);
        }
    }
}
