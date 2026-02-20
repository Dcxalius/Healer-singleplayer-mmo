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
using Project_1.GameObjects.Entities.Friendlies.Players;
using Project_1.Managers;

namespace Project_1.UI.HUD.SpellBook
{
    internal class HeldSpell
    {
        GfxPath heldSpellGfxPath = GfxPath.NullPath;
        bool hasHeldSpell;
        AbsoluteScreenPosition grabOffset;
        AbsoluteScreenPosition size = new AbsoluteScreenPosition(32);
        UITexture gfx;

        public void HoldMe(GfxPath aSpellGfxPath, AbsoluteScreenPosition aGrabOffset)
        {
            heldSpellGfxPath = aSpellGfxPath;
            hasHeldSpell = true;
            grabOffset = aGrabOffset;
            gfx = new UITexture(heldSpellGfxPath, Color.Gray);
        }


        public void ReleaseMe()
        {
            if (!hasHeldSpell) return;
            heldSpellGfxPath = GfxPath.NullPath;
            hasHeldSpell = false;
            grabOffset = AbsoluteScreenPosition.Zero;

            gfx = new UITexture(GfxPath.NullPath, Color.White);
        }

        public void Draw(SpriteBatch aBatch)
        {
            ThreadAffinity.AssertMainThread();
            if (!hasHeldSpell) return;
            Rectangle pos = new Rectangle(UiMouseStateCache.Absolute - grabOffset, size);

            Color transparent = new Color(80, 80, 80, 80);
            gfx.Draw(aBatch, pos, transparent);
        }

    }
}
