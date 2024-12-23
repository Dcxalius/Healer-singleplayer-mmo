﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.GameObjects.Spells;
using Project_1.Input;
using Project_1.Textures;
using Project_1.UI.UIElements.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.SpellBook
{
    internal class HeldSpell
    {
        Spell heldSpell;
        Vector2 grabOffset;
        UITexture xdd;

        public HeldSpell()
        {
            
        }

        public void HoldMe(Spell aSpell, Vector2 aGrabOffset)
        {
            heldSpell = aSpell;
            grabOffset = aGrabOffset;

            xdd = new UITexture(heldSpell.GfxPath, Color.Gray);
        }


        public void ReleaseMe()
        {
            grabOffset = Vector2.Zero;
            heldSpell = null;

            xdd = new UITexture(GfxPath.NullPath, Color.White);
        }

        public void Draw(SpriteBatch aBatch)
        {
            if (heldSpell == null) return;
            Rectangle pos = new Rectangle((InputManager.GetMousePosAbsolute().ToVector2() - grabOffset).ToPoint(), new Point(32));

            Color transparent = new Color(80, 80, 80, 80);
            xdd.Draw(aBatch, pos, transparent);
        }
            
    }
}
