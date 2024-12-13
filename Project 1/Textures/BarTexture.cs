using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Textures
{
    internal class BarTexture : UITexture
    {
        public enum FillingDirection
        {
            Right,
            Left,
            Up,
            Down
        }
        public float Filled
        {
            set
            {
                Debug.Assert(value >= 0 || value <= 1f, "Tried to set wrong value to bar.");
                switch (fillingDirection)
                {
                    case FillingDirection.Right:
                    case FillingDirection.Up:
                        Visible = new Rectangle(Point.Zero, new Point((int)(defaultVisible.X * value), defaultVisible.Y));
                        break;
                    case FillingDirection.Down:
                    case FillingDirection.Left:
                        Visible = new Rectangle(new Point((int)(defaultVisible.X - defaultVisible.X * value), 0), defaultVisible);
                        break;
                    default:
                        break;
                }
            }
        }

        Point defaultVisible;
        FillingDirection fillingDirection;

        public BarTexture(FillingDirection aDir, Color aColor, string aPath = "WhiteGrayBasedBar") : base(aPath, aColor)
        {
            defaultVisible = new Point(64,8);
            fillingDirection = aDir;
            if (fillingDirection == FillingDirection.Up || fillingDirection == FillingDirection.Down)
            {
                Rotation = 0.25f;
            }
            Visible = new Rectangle(Point.Zero, defaultVisible);
        }

    }
}
