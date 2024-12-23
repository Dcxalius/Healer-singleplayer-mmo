﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        Vector2 grabOffset;
        public HeldItem()
        {
        }

        public bool HoldItem(Item aItem, Vector2 aGrabOffset)
        {
            heldItem = aItem;
            heldItem.HoldMe();
            grabOffset = aGrabOffset;
            return true;
        }

        public bool ReleaseMe()
        {
            heldItem.ReleaseMe();
            grabOffset = Vector2.Zero;
            heldItem = null;
            return true;
        }

        public void Draw(SpriteBatch aBatch)
        {
            if (heldItem == null)
            {
                return;
            }

            Rectangle pos = new Rectangle((InputManager.GetMousePosAbsolute().ToVector2() - grabOffset).ToPoint(), new Point(32));
            Color transparent = new Color(80, 80, 80, 80);
            heldItem.Gfx.Draw(aBatch, pos, transparent);
            heldItem.GfxOnButton.Draw(aBatch, pos, transparent);
        }
    }
}
