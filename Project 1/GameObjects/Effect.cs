using Microsoft.Xna.Framework;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects
{
    internal class Effect : GameObject
    {
        Rectangle hitBox;

        public Effect(string aGfxName, Vector2 aStartingPos) : base(new Texture(new GfxPath(GfxType.Effect, aGfxName)), aStartingPos)
        {
            hitBox = new Rectangle();
        }

        bool circlevscirclecollision(Rectangle aCircleThatsARect, Rectangle aCollidesAgainst)
        {
            Point a = aCircleThatsARect.Location + (aCircleThatsARect.Size.ToVector2() / 2).ToPoint();
            Point b = aCollidesAgainst.Location + (aCollidesAgainst.Size.ToVector2() / 2).ToPoint(); ;

            Vector2 bFromA = (a - b).ToVector2();
            Vector2 normal = Vector2.Normalize(bFromA);

            Vector2 xdd = new Vector2(1, 2);
            float isthisit = (normal / xdd).Length();

                        double ellipsedegrees2 = Math.Atan2(norm.Y, norm.X);

            double a = ellipseCentre.X - ellipsePos.Location.X;
            double b = ellipseCentre.Y - ellipsePos.Location.Y;

            

            double x2 = (a * b * Math.Cos(ellipsedegrees2) / Math.Sqrt(Math.Pow(b * Math.Cos(ellipsedegrees2), 2) + Math.Pow(a * Math.Sin(ellipsedegrees2), 2)));
            double y2 = (a * b * Math.Sin(ellipsedegrees2) / Math.Sqrt(Math.Pow(b * Math.Cos(ellipsedegrees2), 2) + Math.Pow(a * Math.Sin(ellipsedegrees2), 2)));
        }
    }
}
