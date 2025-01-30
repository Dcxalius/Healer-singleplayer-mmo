using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Textures;
using Project_1.UI.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.SelectBoxes
{
    internal abstract class SelectBoxValue : UIElement
    {
        public enum SelectBoxValueTypes
        {
            Int,
            String,
            CameraSetting,
            ScreenRez
        }

        public string DisplayText { get => text.Value; }
        public SelectBoxValueTypes Type { get => type; }



        //public 

        protected Text text;

        SelectBoxValueTypes type;

        protected SelectBoxValue(SelectBoxValueTypes aType, UITexture aGfx, string aStartText, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aGfx, aPos, aSize)
        {
            type = aType;
            text = new Text("Gloryse", aStartText, Color.Teal);
        }

        public override void Rescale()
        {
            base.Rescale();
            
            text.Rescale();
        }

        public override void Draw(SpriteBatch aBatch)
        {
            base.Draw(aBatch);

            text.CentredDraw(aBatch, new AbsoluteScreenPosition(AbsolutePos.Center));
        }
    }
}
