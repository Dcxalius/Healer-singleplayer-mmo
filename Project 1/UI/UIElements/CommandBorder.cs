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
        
        public CommandBorder(Color aColor, RelativeScreenPosition aPos, RelativeScreenPosition aSizeOfBoxToBorder, UIElement aParent = null) : base(aParent, null, aPos, aSizeOfBoxToBorder)
        //public Border(Color aColor, Vector2 aPos, Vector2 aSize) : base(new UITexture("GrayWhiteBorder", aColor), aPos, aSize)
        {
            //TODO: This should probably be a shader
        }
    }
}
