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

        public GFXButton(string aGfxPath, Vector2 aPos, Vector2 aSize, Color aColorOfBorder) : base(aPos, aSize, aColorOfBorder)
        {
            gfxOnButton = new UITexture(aGfxPath, Color.White);
            gfxRectangle = TransformFromRelativeToValues(new Vector2(RelativePos.X + RelativeSize.X / 10, RelativePos.Y + RelativeSize.Y / 10), new Vector2(RelativeSize.X * 0.8f, RelativeSize.Y * 0.8f));
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch aBatch)
        {
            base.Draw(aBatch);

            if (!Pressed)
            {
                gfxOnButton.Draw(aBatch, gfxRectangle);
            }
            else
            {
                gfxOnButton.Draw(aBatch, gfxRectangle, Color.DarkGray);

            }
        }

        public override void Rescale()
        {
            base.Rescale();

            gfxRectangle = TransformFromRelativeToValues(new Vector2(RelativePos.X + RelativeSize.X / 10, RelativePos.Y + RelativeSize.Y / 10), new Vector2(RelativeSize.X * 0.8f, RelativeSize.Y * 0.8f));
            //DebugManager.Print(GetType(), "Pos: " + pos);
            //DebugManager.Print(GetType(), "gfxRec: " + gfxRectangle);
        }
    }
}
