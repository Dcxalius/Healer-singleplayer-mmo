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
            SweepRight,
            SweepUp
        }

        public double Ratio { set => ratio = value; }
        double ratio;
        CooldownGfxType cdType;

        public CooldownTexture() : base("Cooldown", Color.White)
        {
            Visible = new Rectangle(Point.Zero, size);
        }

        public Point GetReducedSize(Point aSize)
        {
            Point reduceSizeBy;

            switch (cdType)
            {
                case CooldownGfxType.SweepRight:
                    reduceSizeBy = new Point((int)(aSize.X * (1 - ratio)), 0); 
                    break;
                case CooldownGfxType.SweepUp:
                    reduceSizeBy = new Point(0, (int)(aSize.Y * (1 - ratio)));
                    break;
                default:
                    throw new NotImplementedException();
            }

            return aSize - reduceSizeBy;
        }



        public override void Draw(SpriteBatch aBatch, Rectangle aPosition, Color aColor, float aLayer)
        {
            base.Draw(aBatch, new Rectangle(aPosition.Location, GetReducedSize(aPosition.Size)), aColor, layer);
            //aBatch.Draw(gfx, aPosRectangle, visRect, aColor);
        }
    }
}
