using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements
{
    internal class Border : UIElement
    {
        public bool Visible { get => visible; set => visible = value; }

        bool visible = true;
        public Border(Color aColor, Vector2 aPos, Vector2 aSize) : base(null, aPos, aSize)
        //public Border(Color aColor, Vector2 aPos, Vector2 aSize) : base(new UITexture("GrayWhiteBorder", aColor), aPos, aSize)
        {
            Point size = Camera.TransformRelativeToAbsoluteScreenSpace(aPos);
            Texture2D text = Managers.GraphicsManager.CreateNewTexture(size);

            float borderWidth = 0.05f;

            int xBorder = (int)(size.X * borderWidth);
            Color c = Color.Green;
            Color[] data = new Color[size.X * size.Y];
            for (int i = 0; i < size.X; i++)
            {
                for (int j = 0; j < size.Y; j++)
                {
                    if (i <= size.X * borderWidth || j <= size.Y * borderWidth || i >= size.X - size.X * borderWidth || j >= size.Y - size.Y * borderWidth)
                    {
                        int minX = Math.Min(size.X - i, i);
                        int minY = Math.Min(size.Y - j, j);
                        float colorMultiplier;
                        if (minX < minY)
                        {
                            colorMultiplier = size.X * borderWidth - minX / size.X * borderWidth;
                        }
                        else
                        {

                            colorMultiplier = size.Y * borderWidth - minY / size.Y * borderWidth;
                        }
                        c.A = (byte) (255 - 255 * Math.Min(minX, minY) * borderWidth);
                        data[i + j* size.X] = c;
                    }
                    else
                    {
                        data[i + j * size.X] = Color.Transparent;
                    }
                }
            }

            text.SetData(0, 0, null, data, 0, size.X * size.Y);

            gfx = new Textures.UITexture(text, size);
        }

        public override void Draw(SpriteBatch aBatch)
        {
            if (!visible) return;
            base.Draw(aBatch);
        }
    }
}
