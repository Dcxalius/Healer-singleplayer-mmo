using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
    internal class Button : UIElement
    {

        protected string ButtonText //TODO: Bring this out to a class called TextButton
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

        public Button(Vector2 aPos, Vector2 aSize, Color aColor) : base(new UITexture("WhiteBackground", aColor), aPos, aSize)
        {

            pressedGfx = new UITexture("GrayBackground", aColor);
        }

        public override void Update(in UIElement aParent)
        {
            //if (pressed) { DebugManager.Print(GetType(), "meow"); }
            base.Update(aParent);
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
                //Rectangle? xdd = new Rectangle(parent.Pos.Location ?? Point.Zero + pos.Location, pos.Size);
                pressedGfx.Draw(aBatch, AbsolutePos);

            }

            if (buttonText != null)
            {

                //aBatch.DrawString(GraphicsManager.buttonFont, buttonText, new Vector2(pos.X + pos.Size.X / 2 , pos.Y + pos.Size.Y / 2 ), Color.Black, 0f, textSize / 2, 1.2f, SpriteEffects.None, 1f);
                aBatch.DrawString(GraphicsManager.buttonFont, buttonText, new Vector2(AbsolutePos.X + AbsolutePos.Size.X / 2 , AbsolutePos.Y + AbsolutePos.Size.Y / 2 ), Color.White, 0f, textSize / 2, 1f, SpriteEffects.None, 1f);
            }
        }
    }
}
