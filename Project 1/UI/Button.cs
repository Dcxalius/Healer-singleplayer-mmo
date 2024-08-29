using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Content.Input;
using Project_1.Managers;
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

        protected string ButtonText 
        { 
            get => buttonText;
            set
            {
                buttonText = value;
                textSize = GraphicsManager.buttonFont.MeasureString(buttonText);
            }
        }

        protected bool Pressed
        { 
            get => pressed; 
        }

        UITexture pressedGfx;
        bool pressed = false;

        string buttonText;
        Vector2 textSize;

        public Button(Vector2 aPos, Vector2 aSize, Color aColor) : base(new UITexture(new GfxPath(GfxType.UI, "WhiteBackground"), aColor), aPos, aSize)
        {
            
            pressedGfx = new UITexture(new GfxPath(GfxType.UI, "GrayBackground"), aColor);
        }

        public override void Update()
        {
            if (pressed) { DebugManager.Print(this.GetType(), "meow"); }
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
            }
            else
            {
                pressedGfx.Draw(aBatch, pos);

            }

            if (buttonText != null)
            {

                aBatch.DrawString(GraphicsManager.buttonFont, buttonText, new Vector2(pos.X + pos.Size.X / 2 - textSize.X / 2, pos.Y + pos.Size.Y / 2 - textSize.Y / 2), Color.White);
            }
        }
    }
}
