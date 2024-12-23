﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
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

        public Box(UITexture aGfx, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aGfx, aPos, aSize)
        {

        }

        public override bool ClickedOn(ClickEvent aClick)
        {
            return base.ClickedOn(aClick);

        }

        public override void Update(in UIElement aParent)
        {
            base.Update(aParent);
        }



        public override void Draw(SpriteBatch aBatch)
        { 
            base.Draw(aBatch);
        }
    }
}