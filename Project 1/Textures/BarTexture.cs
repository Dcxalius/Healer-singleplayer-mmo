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
                        visible = new Rectangle(Point.Zero, new Point(visible.Value.Height, (int)(defaultVisible.Y * value)));
                        break;
                    case FillingDirection.Down:
                    case FillingDirection.Left:
                        visible = new Rectangle(new Point((int)(defaultVisible.X - defaultVisible.X * value), 0), visible.Value.Size);
                        break;
                    default:
                        break;
                }
            }
        }

        Point defaultVisible;
        FillingDirection fillingDirection;

        public BarTexture(FillingDirection aDir, string aPath, Color aColor) : base(aPath, aColor)
        {
            defaultVisible = size;
            fillingDirection = aDir;
            if (fillingDirection == FillingDirection.Up || fillingDirection == FillingDirection.Down)
            {
                rotation = 0.25f;
            }
        }

    }
}
