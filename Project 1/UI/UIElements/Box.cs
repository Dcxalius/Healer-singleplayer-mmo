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
        protected bool visible = true;
        protected KeyBindManager.KeyListner? visibleKey = null;

        public Box(UITexture aGfx, Vector2 aPos, Vector2 aSize) : base(aGfx, aPos, aSize)
        {

        }

        public override bool ClickedOn(ClickEvent aClick)
        {
            if (!visible) return false;
            return base.ClickedOn(aClick);

        }

        public override void Update(in UIElement aParent)
        {
            base.Update(aParent);

            if (!visibleKey.HasValue) return;
            if (KeyBindManager.GetPress(visibleKey.Value))
            {
                visible = !visible;
            }
        }
        public override void Draw(SpriteBatch aBatch)
        { 
            if (!visible) return;

            base.Draw(aBatch);
        }
    }
}