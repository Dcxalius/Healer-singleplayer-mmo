﻿using Microsoft.Xna.Framework;
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

namespace Project_1.UI.UIElements.Buttons
{
    internal class Button : UIElement
    {

        public string ButtonText //TODO: Bring this out to a class called TextButton
        {
            get => label.Text;
            set { label.Text = value; }
        }

        protected bool Pressed
        {
            get => pressed;
            set => pressed = value;
        }

        UITexture hoverGfx;
        UITexture pressedGfx;
        bool pressed;

        Label label;

        protected bool usesPressedGfx;

        public virtual List<Action> Actions => actions;
        List<Action> actions;

        public Button(List<Action> aActions, RelativeScreenPosition aPos, RelativeScreenPosition aSize, Color aColor, string aText = null, Color? aTextColor = null) : this(aPos, aSize, aColor, aText, aTextColor)
        {
            actions = aActions ?? new List<Action>();
        }

        public Button(RelativeScreenPosition aPos, RelativeScreenPosition aSize, Color aColor, string aText = null, Color? aTextColor = null) : base(new UITexture("WhiteBackground", aColor), aPos, aSize)
        {
            Color denullifiedTextColor = aTextColor ?? Color.White;
            label = new Label(aText, RelativeScreenPosition.Zero, RelativeScreenPosition.One, Label.TextAllignment.Centred, denullifiedTextColor);
            AddChild(label);
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

        public override void ClickedOnAndReleasedOnMe()
        {
            pressed = false;

            if (Actions.Count != 0)
            {
                for (int i = 0; i < Actions.Count; i++)
                {
                    if (Actions[i] == null) continue;

                    Actions[i].Invoke();
                }
            }

            base.ClickedOnAndReleasedOnMe();

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

            //if (text.Value == null)
            //{
            //    return;
            //}
            //text.CentredDraw(aBatch, new AbsoluteScreenPosition(AbsolutePos.Center));

            if (isHovered)
            {
                //hoverGfx.Draw(aBatch, AbsolutePos);
            }
        }
    }
}
