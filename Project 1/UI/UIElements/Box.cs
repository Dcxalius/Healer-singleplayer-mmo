using Microsoft.Xna.Framework;
using Project_1.Input;
using Project_1.Managers;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements
{
    internal class Box : UIElement
    {

        public Box(in Rectangle? aParentPos, UITexture aGfx, Vector2 aPos, Vector2 aSize) : base(aParentPos, aGfx, aPos, aSize)
        {

        }


        public override void Update()
        {
            base.Update();
        }
        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch aBatch)
        {
            base.Draw(aBatch);
        }

        public override void HoldReleaseOnMe()
        {
            base.HoldReleaseOnMe();
        }

        public override void ClickedOnMe(ClickEvent click)
        {
            base.ClickedOnMe(click);
        }
    }
}
