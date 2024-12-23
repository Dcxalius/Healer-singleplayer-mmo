﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Input;
using Project_1.Managers;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Project_1.UI.UIElements
{
    internal abstract class UIElement
    {
        protected bool Visible { get => visible; }
        bool visible;
        protected KeyBindManager.KeyListner? visibleKey = null;
        public RelativeScreenPosition RelativePos { get => relativePos; }
        public RelativeScreenPosition RelativeSize { get => relativeSize; }

        public Rectangle AbsolutePos
        {
            get
            {
                Rectangle tempRec = Rectangle.Empty;
                tempRec.Location = absolutePos.ToPoint();
                tempRec.Size = Size.ToPoint();
                return tempRec;
            }
        }


        public AbsoluteScreenPosition Size { get => new AbsoluteScreenPosition(pos.Size); }

        public UITexture Gfx { get => gfx; }
        public Color Color { get => gfx.Color; set => gfx.Color = value; }

        bool Hovered { get => AbsolutePos.Contains(InputManager.GetMousePosAbsolute().ToPoint()); }
        protected bool wasHovered = false;

        protected UITexture gfx;
        AbsoluteScreenPosition absolutePos;
        Rectangle pos;
        RelativeScreenPosition relativePos; //TODO: Change this so 1, 1 refers to parents bottom right instead of screen bottom right + parent top left
        RelativeScreenPosition relativeSize;

        public HoldEvent heldEvents; //TODO: This should prob be cleansed on state change



        //protected UIElement? parent;
        protected AbsoluteScreenPosition parentPos = AbsoluteScreenPosition.Zero;
        protected List<UIElement> children = new List<UIElement>();

        protected UIElement(UITexture aGfx, RelativeScreenPosition aPos, RelativeScreenPosition aSize) //aPos and aSize should be between 0 and 1
        {
            //Debug.Assert(aPos > 0 && aPos < 0);
            visible = true;
            gfx = aGfx;

            relativePos = aPos;
            relativeSize = aSize;

            pos = RelativeScreenPosition.TransformToAbsoluteRect(aPos, aSize);
            absolutePos = new AbsoluteScreenPosition(pos.Location);
        }

        public virtual void Update(in UIElement aParent)
        {
            HoldUpdate();

            if (aParent != null) //TODO: Make this less ugly
            {
                parentPos = aParent.absolutePos;
                absolutePos = aParent.absolutePos + new AbsoluteScreenPosition(pos.Location);
            }
            else
            {
                absolutePos = new AbsoluteScreenPosition(pos.Location);
            }
            foreach (UIElement child in children)
            {
               
                child.Update(this);
            }



            HoverUpdate();
            GetVisibiltyPress();
        }

        void HoverUpdate()
        {
            if (!visible) return;

            if (!wasHovered && Hovered)
            {
                wasHovered = true;
                OnHover();
            }

            if (wasHovered && !Hovered)
            {
                wasHovered = false;
                OnDeHover();
            }
        }

        void GetVisibiltyPress()
        {
            if (!visibleKey.HasValue) return;
            if (KeyBindManager.GetPress(visibleKey.Value))
            {
                ToggleVisibilty();
            }
        }

        public void ToggleVisibilty()
        {
            visible = !visible;
            foreach (UIElement child in children)
            {
                child.ToggleVisibilty();
            }
        }


        static protected Point TransformFromRelativeToPoint(Vector2 aValue)
        {
            Point size = new Point((int)(Camera.Camera.ScreenSize.X * aValue.X), (int)(Camera.Camera.ScreenSize.Y * aValue.Y));
            return size;
        }

        public virtual void Close()
        {
            for (int i = 0; i < children.Count; i++)
            {
                children[i].Close();
            }
        }

        public virtual void HoldUpdate()
        {
            if (heldEvents == null)
            {
                return;
            }

            if (!heldEvents.IsStillHeld())
            {
                if (wasHovered)
                {
                    HoldReleaseOnMe();
                }
                else
                {
                    HoldReleaseAwayFromMe();
                }
            }
        }

        public virtual bool ReleasedOn(ReleaseEvent aRelease)
        {
            if (!visible) return false;
            if (AbsolutePos.Contains(aRelease.AbsolutePos))
            {
                bool clickedOnChild = ReleasedOnChildren(aRelease);
                if (clickedOnChild == false)
                {
                    ReleaseOnMe(aRelease);
                }
                return true;
            }
            return false;
        }

        public bool ReleasedOnChildren(ReleaseEvent aRelease)
        {
            if (!visible) return false;
            for (int i = 0; i < children.Count; i++)
            {

                bool releasedOn = children[i].ReleasedOn(aRelease);
                if (releasedOn)
                {
                    ReleasedOnChild(aRelease);
                    return true;

                }
            }

            return false;
        }

        public virtual void ReleasedOnChild(ReleaseEvent aRelease)
        {

            if (!visible) return;
        }

        public virtual void ReleaseOnMe(ReleaseEvent aRelease)
        {
            if (!visible) return;

        }

        public virtual void HoldReleaseOnMe()
        {
            heldEvents = null;
        }

        protected virtual void HoldReleaseAwayFromMe()
        {
            heldEvents = null;
        }

        protected virtual void OnHover()
        {
            if (!visible) return;
            //DebugManager.Print(GetType(), "Hovered on");
        }

        protected virtual void OnDeHover()
        {
            if (!visible) return;
        }

        public virtual bool ClickedOn(ClickEvent aClick)
        {
            if (!visible) return false;

            if (AbsolutePos.Contains(aClick.AbsolutePos))
            {
                bool clickedOnChild = ClickedOnChildren(aClick);
                if (clickedOnChild == false)
                {
                    ClickedOnMe(aClick);
                }
                return true;
            }
            return false;
        }

        protected virtual bool ClickedOnChildren(ClickEvent aClick)
        {
            if (!visible) return false;

            for (int i = 0; i < children.Count; i++)
            {
                
                bool clickedOn = children[i].ClickedOn(aClick);
                if (clickedOn)
                {
                    ClickedOnChild(aClick);
                    return true;

                }
            }

            return false;
        }

        protected virtual void ClickedOnChild(ClickEvent aClick)
        {
            if (!visible) return;

        }

        protected virtual void ClickedOnMe(ClickEvent aClick)
        {
            if (!visible) return;
            heldEvents = new HoldEvent(TimeManager.TotalFrameTime, aClick, this);

            //DebugManager.Print(GetType(), "Clicked on " + pos);
        }

        public virtual void Rescale()
        {
            pos = RelativeScreenPosition.TransformToAbsoluteRect(relativePos, relativeSize);
            absolutePos = parentPos + new AbsoluteScreenPosition(pos.Location);

            for (int i = 0; i < children.Count; i++)
            {
                children[i].Rescale();
            }
        }

        public void Move(RelativeScreenPosition aNewPos)
        {
            if (aNewPos.X == float.NaN || aNewPos.Y == float.NaN) throw new ArgumentException("Invalid move.");
            relativePos = aNewPos;
            pos.Location = TransformFromRelativeToPoint(aNewPos);
            absolutePos = new AbsoluteScreenPosition(pos.Location) + parentPos;
        }

        protected void Resize(RelativeScreenPosition aSize)
        {
            relativeSize = aSize;
            pos.Size = TransformFromRelativeToPoint(aSize);
        }

        public virtual void Draw(SpriteBatch aBatch)
        {
            if (!visible) return;

            if (gfx != null)
            {

                gfx.Draw(aBatch, AbsolutePos);
            }

            GraphicsManager.CaptureScissor(this, pos);
            foreach (UIElement child in children)
            {
                child.Draw(aBatch);
            }
            GraphicsManager.ReleaseScissor(this);
        }
    }
}
