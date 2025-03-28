using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects.Spells;
using Project_1.Managers;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.Bars
{
    internal class CastBar : Bar
    {
        Spell castSpell;
        UITexture spellTexture;
        Text remainingCast;
        public override float Value
        {
            set
            {
                remainingCast.Value = Math.Round((castSpell.CastTime - castSpell.CastTime * value) / 1000, 1).ToString();

                base.Value = value;
            }
        }
        public CastBar(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new BarTexture(BarTexture.FillingDirection.Right, Color.White), new UITexture(new GfxPath(GfxType.UI, "WhiteBackground"), Color.Black), aPos, aSize)
        {
            remainingCast = new Text("Gloryse", Color.Wheat);
        }

        public void CancelCast()
        {
            castSpell = null; //TODO: Do something fancy
            spellTexture = null;
        }

        public void FinishCast()
        {
            castSpell = null; //TODO: Do something fancier
            spellTexture = null;
        }

        public void CastSpell(Spell aSpell)
        {
            spellTexture = new UITexture(aSpell.GfxPath, Color.White);
            castSpell = aSpell;
        }

        public override void Draw(SpriteBatch aBatch)
        {
            if (castSpell == null) return;
            base.Draw(aBatch);
            spellTexture.Draw(aBatch, new Rectangle(AbsolutePos.Location, new Point(AbsolutePos.Size.Y)));
            remainingCast.CentreRightDraw(aBatch, new AbsoluteScreenPosition(AbsolutePos.Right - 5, AbsolutePos.Center.Y));
        }
    }
}
