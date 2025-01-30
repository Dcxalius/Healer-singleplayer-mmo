using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Input;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.Inventory
{
    internal class HeldItem
    {
        Item heldItem;
        AbsoluteScreenPosition grabOffset;
        AbsoluteScreenPosition size = new AbsoluteScreenPosition(32);

        public bool HoldItem(Item aItem, AbsoluteScreenPosition aGrabOffset)
        {
            heldItem = aItem;
            heldItem.HoldMe();
            grabOffset = aGrabOffset;
            if (grabOffset.X > size.ToVector2().X) grabOffset.X = size.X;
            if (grabOffset.Y > size.ToVector2().Y) grabOffset.Y = size.Y;
            return true;
        }

        public bool ReleaseMe()
        {
            if (heldItem == null) return false;
            heldItem.ReleaseMe();
            grabOffset = AbsoluteScreenPosition.Zero;
            heldItem = null;
            return true;
        }

        public void Draw(SpriteBatch aBatch)
        {
            if (heldItem == null)
            {
                return;
            }

            Rectangle pos = new Rectangle((InputManager.GetMousePosAbsolute() - grabOffset), size);
            Color transparent = new Color(80, 80, 80, 80);
            heldItem.Gfx.Draw(aBatch, pos, transparent);
            heldItem.GfxOnButton.Draw(aBatch, pos, transparent);
        }
    }
}
