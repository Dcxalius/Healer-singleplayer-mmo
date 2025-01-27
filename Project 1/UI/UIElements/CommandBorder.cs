using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Managers;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements
{
    internal class CommandBorder : UIElement
    {
        public new bool Visible { get => visible; set => visible = value; }

        bool visible = true;
        public CommandBorder(Color aColor, RelativeScreenPosition aPos, RelativeScreenPosition aSizeOfBoxToBorder) : base(null, aPos, aSizeOfBoxToBorder)
        //public Border(Color aColor, Vector2 aPos, Vector2 aSize) : base(new UITexture("GrayWhiteBorder", aColor), aPos, aSize)
        {
            //This probably should be broken out and called on rescale or be completly reworked and handled through shaders
            //AbsoluteScreenPosition size = aSizeOfBoxToBorder.ToAbsoluteScreenPos();
            //Texture2D text = Managers.GraphicsManager.CreateNewTexture(size.ToPoint());

            //float borderWidth = 0.10f;

            //int xBorder = (int)(size.X * borderWidth);
            //Color c = Color.DarkGreen;
            //Color[] data = new Color[size.X * size.Y];
            //int borderSize = Math.Min(size.X, size.Y);
            //for (int i = 0; i < size.X; i++)
            //{
            //    for (int j = 0; j < size.Y; j++)
            //    {
            //        if (i <= borderSize * borderWidth || j <= borderSize * borderWidth || i >= size.X - borderSize * borderWidth || j >= size.Y - borderSize * borderWidth)
            //        {
            //            int minX = Math.Min(size.X - i, i);
            //            int minY = Math.Min(size.Y - j, j);
            //            //float colorMultiplier;
            //            //if (minX < minY)
            //            //{
            //            //    colorMultiplier = borderWidth - minX / borderWidth;
            //            //}
            //            //else
            //            //{

            //            //    colorMultiplier =  borderWidth - minY / borderWidth;
            //            //}
            //            //c.A = (byte) (255 - 255 * Math.Min(minX, minY) / (borderSize * borderWidth));
            //            //c.A = (byte) (125 - 125 * Math.Min(minX, minY) / (borderSize * borderWidth));
            //            int min = (Math.Min(minX, minY) + 1);
            //            c.A = (byte)(255 / min);
            //            //if (RandomManager.RollDouble() + 0.2 > Math.Min(minX, minY) / (borderSize * borderWidth)) //TODO: Change this
            //            //{
            //            //    data[i + j * size.X] = c;

            //            //}
            //            //else
            //            {
            //                data[i + j * size.X] = c;

            //            }

            //        }
            //        else
            //        {
            //            data[i + j * size.X] = Color.Transparent;
            //        }
            //    }
            //}

            //text.SetData(0, 0, null, data, 0, size.X * size.Y);

            //gfx = new Textures.UITexture(text, size.ToPoint());
        }

        public override void Draw(SpriteBatch aBatch, float aLayer)
        {
            if (!visible) return;
            base.Draw(aBatch, aLayer);
        }
    }
}
