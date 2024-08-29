using Microsoft.Xna.Framework;
using Project_1.Content.Input;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI
{
    internal class GFXButton : Button
    {
        Texture gfx;

        public GFXButton(GfxPath aGfxPath, Vector2 aPos, Vector2 aSize, Color aColorOfBorder) : base(aPos, aSize, aColorOfBorder)
        {
            gfx = new Texture(aGfxPath);
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch aBatch)
        {
            base.Draw(aBatch);

            if (!Pressed)
            {
                gfx.Draw(aBatch, new Rectangle(pos.Location.X + pos.Size.X / 10, pos.Location.Y + pos.Size.Y / 10, pos.Size.X / 10 * 9, pos.Size.Y / 10 * 9));
            }
            else
            {
                gfx.Draw(aBatch, new Rectangle(pos.Location.X + pos.Size.X / 10, pos.Location.Y + pos.Size.Y / 10, pos.Size.X / 10 * 9, pos.Size.Y / 10 * 9), Color.DarkGray);

            }
        }

        public override void ClickedOnMe(ClickEvent aClick)
        {
            base.ClickedOnMe(aClick);
        }
    }
}
