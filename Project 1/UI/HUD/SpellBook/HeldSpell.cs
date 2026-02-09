using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects.Spells;
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
        Spell heldSpell;
        AbsoluteScreenPosition grabOffset;
        AbsoluteScreenPosition size = new AbsoluteScreenPosition(32);
        UITexture gfx;

        public void HoldMe(Spell aSpell, AbsoluteScreenPosition aGrabOffset)
        {
            heldSpell = aSpell;
            grabOffset = aGrabOffset;

            gfx = new UITexture(heldSpell.GfxPath, Color.Gray);
        }


        public void ReleaseMe()
        {
            if (heldSpell == null) return;
            heldSpell = null;
            grabOffset = AbsoluteScreenPosition.Zero;

            gfx = new UITexture(GfxPath.NullPath, Color.White);
        }

        public void Draw(SpriteBatch aBatch)
        {
            ThreadAffinity.AssertMainThread();
            if (heldSpell == null) return;
            Rectangle pos = new Rectangle(UiMouseStateCache.Absolute - grabOffset, size);

            Color transparent = new Color(80, 80, 80, 80);
            gfx.Draw(aBatch, pos, transparent);
        }

    }
}
