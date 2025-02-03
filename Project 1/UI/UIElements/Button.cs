using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
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
    internal class Button : UIElement
    {

        protected string ButtonText //TODO: Bring this out to a class called TextButton
        {
            get => text.Value;
            set { text.Value = value; }
        }

        protected bool Pressed
        {
            get => pressed;
            set => pressed = value;
        }

        UITexture hoverGfx;
        UITexture pressedGfx;
        bool pressed;

        Text text;

        protected bool usesPressedGfx;

        List<Action> actions;

        public Button(List<Action> aActions, RelativeScreenPosition aPos, RelativeScreenPosition aSize, Color aColor, string aText = null, Color? aTextColor = null) : this(aPos, aSize, aColor, aText, aTextColor) 
        {
            actions = aActions ?? new List<Action>();
        }

        public Button(RelativeScreenPosition aPos, RelativeScreenPosition aSize, Color aColor, string aText = null, Color? aTextColor = null) : base(new UITexture("WhiteBackground", aColor), aPos, aSize)
        {
            Color denullifiedTextColor = aTextColor ?? Color.White;
            text = new Text("Gloryse", aText, denullifiedTextColor);
            pressedGfx = new UITexture("GrayBackground", aColor);
            hoverGfx = new UITexture("ButtonHover", Color.Yellow);
            usesPressedGfx = true;
            actions = new List<Action>();

        }

        public void AddAction(Action aAction) => actions.Add(aAction);

        protected override void ClickedOnMe(ClickEvent aClick)
        {
            pressed = true;
            base.ClickedOnMe(aClick);
        }

        public override void HoldReleaseOnMe()
        {
            pressed = false;

            if (actions.Count != 0)
            {
                for (int i = 0; i < actions.Count; i++)
                {
                    if (actions[i] == null) continue;

                    actions[i].Invoke();
                }
            }

            base.HoldReleaseOnMe();

        }

        protected override void HoldReleaseAwayFromMe()
        {
            pressed = false;
            base.HoldReleaseAwayFromMe();
        }


        public override void Draw(SpriteBatch aBatch)
        {
            if (!pressed || !usesPressedGfx)
            {
                base.Draw(aBatch);
            }
            else
            {
                pressedGfx.Draw(aBatch, AbsolutePos);
            }

            if (text.Value == null)
            {
                return;
            }
            text.CentredDraw(aBatch, new AbsoluteScreenPosition(AbsolutePos.Center));

            if (wasHovered)
            {
                //hoverGfx.Draw(aBatch, AbsolutePos);
            }
        }
    }
}
