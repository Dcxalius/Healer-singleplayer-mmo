using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Content.Input;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI
{
    internal class Button : UIElement
    {
        UITexture pressedGfx;

        bool pressed = false;

        public Button(Vector2 aPos, Vector2 aSize, Color aColor) : base(new UITexture(new GfxPath(GfxType.UI, "WhiteBackground"), aColor), aPos, aSize)
        {
            
            pressedGfx = new UITexture(new GfxPath(GfxType.UI, "GrayBackground"), aColor);
        }

        public override void Update()
        {
            base.Update();
        }

        public override void ClickedOnMe(ClickEvent aClick)
        {
            pressed = true;
            base.ClickedOnMe(aClick);
        }
        
        public override void HoldReleaseOnMe()
        {
            pressed = false;
            base.HoldReleaseOnMe();

        }

        public override void Draw(SpriteBatch aBatch)
        {
            if (!pressed)
            {
                base.Draw(aBatch);
                return;
            }

            pressedGfx.Draw(aBatch, pos);
        }
    }
}
