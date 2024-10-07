using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Input;
using Project_1.Managers;
using Project_1.Textures;
using Project_1.UI.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements
{
    internal abstract class Box : UIElement
    {
        public Box(UITexture aGfx, Vector2 aPos, Vector2 aSize) : base(aGfx, aPos, aSize)
        {

        }


        public override void Update(in UIElement aParent)
        {
            base.Update(aParent);
        }
        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch aBatch)
        {
            base.Draw(aBatch);
        }
    }
}
//internal abstract class Box : UIElement
//{

//    public Box(UITexture aGfx, Vector2 aPos, Vector2 aSize) : base(aGfx, aPos, aSize)
//    {

//    }


//    public override void Update(in UIElement aParent)
//    {
//        base.Update(aParent);
//    }
//    public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch aBatch)
//    {
//        base.Draw(aBatch);
//    }
//}