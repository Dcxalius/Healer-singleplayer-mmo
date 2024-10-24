using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Textures
{
    internal class CooldownTexture : UITexture
    {
        public enum CooldownGfxType
        {
            SweepingRight
        }

        CooldownGfxType cdType;

        public CooldownTexture() : base("Cooldown", Color.White)
        {
            visible = new Rectangle(Point.Zero, size);
        }

        public void UpdateDuration(double aRatio)
        {
            visible = new Rectangle(Point.Zero, new Point(size.X, (int)(size.Y * (1 - aRatio))));
        }

        public override void Draw(SpriteBatch aBatch, Rectangle aPosRectangle, Color aColor)
        {
            //base.Draw(aBatch, aPosRectangle, aColor);
            aBatch.Draw(gfx, aPosRectangle, visible, aColor);
        }
    }
}
