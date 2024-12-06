using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.PlateBoxes
{
    internal class PlateBoxNameSegment : PlateBoxSegment
    {
        public string Name { get => Text; set => Text = value; }
        public Color BackgroundColor { set => gfx.Color = value; }

        

        public PlateBoxNameSegment(string name, Color aNamePlateColor, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new UITexture("WhiteBackground", aNamePlateColor), aPos, aSize)
        {
            Text = name;
            
        }

        public override void Draw(SpriteBatch aBatch)
        {
            base.Draw(aBatch);
        }
    }
}
