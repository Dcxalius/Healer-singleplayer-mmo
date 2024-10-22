using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Input;
using Project_1.Managers;
using Project_1.Textures;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements
{
    internal abstract class Button : UIElement
    {

        protected string ButtonText //TODO: Bring this out to a class called TextButton
        {
            get => text.Value;
            set { text.Value = value; }
        }

        protected bool Pressed
        {
            get => pressed;
        }

        UITexture hoverGfx;
        UITexture pressedGfx;
        bool pressed = false;

        Text text;

        public Button(Vector2 aPos, Vector2 aSize, Color aColor, string aText = null, Color? aTextColor = null) : base(new UITexture("WhiteBackground", aColor), aPos, aSize)
        {
            Color denullifiedTextColor = aTextColor == null ? Color.White : aTextColor.Value;
            text = new Text("Gloryse", aText, denullifiedTextColor);
            pressedGfx = new UITexture("GrayBackground", aColor);
            hoverGfx = new UITexture("ButtonHover", Color.Yellow);
        }

        public override void Update(in UIElement aParent)
        {
            //if (pressed) { DebugManager.Print(GetType(), "meow"); }
            base.Update(aParent);
        }

        protected override void ClickedOnMe(ClickEvent aClick)
        {
            pressed = true;
            base.ClickedOnMe(aClick);
        }

        public override void HoldReleaseOnMe()
        {
            pressed = false;
            base.HoldReleaseOnMe();

        }

        protected override void HoldReleaseAwayFromMe()
        {
            pressed = false;
            base.HoldReleaseAwayFromMe();
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

            if (text.Value == null)
            {
                return;
            }
            text.CentredDraw(aBatch, AbsolutePos.Center.ToVector2());

            if (wasHovered)
            {
                hoverGfx.Draw(aBatch, AbsolutePos);
            }
        }
    }
}
