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
using System.Windows.Forms.Design;
using System.Xml.Serialization;

namespace Project_1.UI.UIElements
{
    internal abstract class UIElement
    {
        #region Interactibility
        public virtual bool Visible
        {
            get => visible;

            set
            {
                visible = value;
                for (int i = 0; i < children.Count; i++)
                {
                    children[i].Visible = value;
                }
            }
        }

        bool visible;

        protected KeyBindManager.KeyListner? visibleKey;

        bool Hovered => AbsolutePos.Contains(InputManager.GetMousePosAbsolute().ToPoint());
        protected bool wasHovered;

        public bool CapturesClick { get => capturesClick; set => capturesClick = value; }
        protected bool capturesClick;
        public bool CapturesScroll { get => capturesScroll; set => capturesScroll = value; }
        protected bool capturesScroll;
        public bool CapturesRelease { get => capturesRelease; set => capturesRelease = value; }
        protected bool capturesRelease;

        public bool AlwaysOnScreen
        {
            get => alwaysOnScreen;
            set
            {
                alwaysOnScreen = value;
                if (value)
                {
                    alwaysFullyOnScreen = !value;
                }
            }
        }

        protected bool alwaysOnScreen;
        readonly Point alwaysOnScreenAmount = new Point(10, 10);

        public bool AlwaysFullyOnScreen //TOOD: Change to enum?
        {
            get => alwaysFullyOnScreen;
            protected set
            {
                alwaysFullyOnScreen = value;

                if (value)
                {
                    alwaysOnScreen = !value;
                }
            }
        }

        bool alwaysFullyOnScreen;

        public bool Dragable
        {
            get => dragable;
            protected set => dragable = value;
        }
        bool dragable;

        RelativeScreenPosition oldPosition;
        protected bool hudMoving;
        public bool HudMoveable => hudMoveable;
        protected bool hudMoveable;
        static UITexture movableGFX => new UITexture("MovableHUD", Color.White);
        Text nameText;

        readonly TimeSpan timeBeforeDragRegisters = TimeSpan.FromSeconds(0.2);

        public HoldEvent heldEvents;
        #endregion

        #region Position
        public RelativeScreenPosition RelativePos => relativePos;
        public RelativeScreenPosition RelativeSize => relativeSize;

        public RelativeScreenPosition RelativePositionOnScreen => relativePos + ParentPos.ToRelativeScreenPosition();

        public Rectangle AbsolutePos
        {
            get
            {
                Rectangle tempRec = Rectangle.Empty;
                tempRec.Location = pos.Location + ParentPos;
                tempRec.Size = Size.ToPoint();
                return tempRec;
            }
        }

        public AbsoluteScreenPosition Location => AbsolutePosition;

        AbsoluteScreenPosition AbsolutePosition => new AbsoluteScreenPosition(pos.Location) + ParentPos;
        
        RelativeScreenPosition relativePos; //TODO: Change this so 1, 1 refers to parents bottom right instead of screen bottom right + parent top left
        RelativeScreenPosition relativeSize; 
        public AbsoluteScreenPosition Size => new AbsoluteScreenPosition(pos.Size);
        Rectangle pos;
        #endregion

        #region Graphics
        public UITexture Gfx => gfx;
        protected UITexture gfx;
        public virtual Color Color { get => gfx.Color; set => gfx.Color = value; } 
        #endregion

        #region Parentage
        protected UIElement parent;
        protected AbsoluteScreenPosition ParentPos => parent == null ? AbsoluteScreenPosition.Zero : parent.AbsolutePosition;

        List<UIElement> children = new List<UIElement>();
        protected int ChildCount => children.Count;

        protected virtual void KillAllChildren() => children.Clear();
        protected virtual void KillChild(int aIndex) => children.RemoveAt(aIndex);
        protected virtual void KillChild(UIElement aChild) => children.Remove(aChild);
        protected UIElement GetChild(int aIndex) => children[aIndex];
        protected int GetChildID(UIElement aChild) => children.IndexOf(aChild);
        protected void SetParent(UIElement aParent) => parent = aParent;

        protected virtual void AddChild(UIElement aUIElement)
        {
            aUIElement.Visible = Visible;
            aUIElement.parent = this;
            children.Add(aUIElement);
        }

        protected void AddChildren(UIElement[] aUIElement)
        {
            for (int i = 0; i < aUIElement.Length; i++)
            {
                aUIElement[i].parent = this;
                AddChild(aUIElement[i]);
            }
        }

        protected void AddChildren<T>(List<T> aUIElement) where T : UIElement
        {
            for (int i = 0; i < aUIElement.Count; i++)
            {
                aUIElement[i].parent = this;
                AddChild(aUIElement[i]);
            }
        }

        #endregion

        public (string, RelativeScreenPosition) Save => (GetType().Name, RelativePos);

        protected UIElement(UITexture aGfx, RelativeScreenPosition aPos, RelativeScreenPosition aSize) //aPos and aSize should be between 0 and 1
        {
            visible = true;
            gfx = aGfx;

            relativePos = aPos;
            relativeSize = aSize;

            pos = RelativeScreenPosition.TransformToAbsoluteRect(aPos, aSize);
            nameText = new Text("Gloryse", GetType().Name);


            capturesClick = true;
            capturesScroll = false;
            capturesRelease = true;
            alwaysOnScreen = false;
            alwaysFullyOnScreen = false;
            hudMoveable = true;
        }



        #region Update
        public virtual void Update()
        {
            HoldUpdate();
            UpdateChildren();
            HoverUpdate();
            GetVisibiltyPress();
        }

        public void HUDMovableUpdate()
        {
            if (!hudMoveable) return;
            HoldUpdate();
        }

        void UpdateChildren()
        {
            for (int i = 0; i < children.Count; i++)
            {
                children[i].Update();
            }
        }

        protected virtual void HoldUpdate()
        {
            if (heldEvents == null) return;

            if (!heldEvents.IsStillHeld())
            {
                if (wasHovered)
                {
                    ClickedOnAndReleasedOnMe();
                    return;
                }

                HoldReleaseAwayFromMe();
                return;
            }

            if (!Dragable && !hudMoving) return;
            if (heldEvents.DurationHeld < timeBeforeDragRegisters.TotalSeconds) return;
            
            Move(InputManager.GetMousePosRelative() - heldEvents.Offset);
        }
        #endregion
 
        #region Change
        void GetVisibiltyPress()
        {
            if (!visibleKey.HasValue) return;
            if (KeyBindManager.GetPress(visibleKey.Value))
            {
                ToggleVisibilty();
            }
        }

        public void SetHudMoveable(bool aSet)
        {
            hudMoving = aSet;
            if (hudMoving)
            {
                oldPosition = RelativePos;
            }
        }

        public void ResetHudMoveable()
        {
            hudMoving = false;
            Move(oldPosition);
        }

        public virtual void ToggleVisibilty() => Visible = !visible;

        public virtual void Rescale()
        {
            pos = RelativeScreenPosition.TransformToAbsoluteRect(relativePos, relativeSize);

            for (int i = 0; i < children.Count; i++)
            {
                children[i].Rescale();
            }

            nameText.Rescale();
        }

        public void Move(RelativeScreenPosition aNewPos)
        {
            if (aNewPos.X == float.NaN || aNewPos.Y == float.NaN) throw new ArgumentException("Invalid move.");
            pos.Location = aNewPos.ToAbsoluteScreenPos();
            relativePos = aNewPos;
            MoveBoundsCheck();
        }

        void MoveBoundsCheck()
        {
            AlwaysOnScreenCheck();
            AlwaysFullyOnScreenCheck();
        }

        void AlwaysOnScreenCheck()
        {
            if (!alwaysOnScreen) return;

            Rectangle overlap = Rectangle.Intersect(Camera.Camera.ScreenRectangle, pos);

            if (overlap == pos) return;

            (bool, bool) outOfBounds = OutOfBoundsCheck(overlap.Size, alwaysOnScreenAmount);
            bool xOutOfBounds = outOfBounds.Item1;
            bool yOutOfBounds = outOfBounds.Item2;

            if (!xOutOfBounds && !yOutOfBounds) return;


            pos.Location = GetNewMove(xOutOfBounds, yOutOfBounds, alwaysOnScreenAmount);


            relativePos = new AbsoluteScreenPosition(pos.Location).ToRelativeScreenPosition();
        }

        void AlwaysFullyOnScreenCheck()
        {
            if (!alwaysFullyOnScreen) return;

            Rectangle overlap = Rectangle.Intersect(Camera.Camera.ScreenRectangle, pos);

            if (overlap == pos) return;

            (bool, bool) outOfBounds = OutOfBoundsCheck(overlap.Size, Size);
            bool xOutOfBounds = outOfBounds.Item1;
            bool yOutOfBounds = outOfBounds.Item2;

            if (!xOutOfBounds && !yOutOfBounds) return;


            pos.Location = GetNewMove(xOutOfBounds, yOutOfBounds, Size);
            relativePos = new AbsoluteScreenPosition(pos.Location).ToRelativeScreenPosition();
        }

        Point GetNewMove(bool xOutOfBounds, bool yOutOfBounds, Point alwaysOnScreenAmount)
        {
            Point returnable = pos.Location;

            if (xOutOfBounds)
            {
                if (pos.Location.X < 0)
                {
                    returnable.X = alwaysOnScreenAmount.X - pos.Width;
                }
                else
                {
                    returnable.X = Camera.Camera.ScreenRectangle.Width - alwaysOnScreenAmount.X;
                }
            }

            if (yOutOfBounds)
            {
                if (pos.Location.Y < 0)
                {
                    returnable.Y = alwaysOnScreenAmount.Y - pos.Height;
                }
                else
                {
                    returnable.Y = Camera.Camera.ScreenRectangle.Height - alwaysOnScreenAmount.Y;
                }
            }

            return returnable;
        }

        (bool, bool) OutOfBoundsCheck(Point aOverlapSize, Point aOnScreenAmount)
        {
            bool xOutOfBounds = false;
            bool yOutOfBounds = false;
            if (aOverlapSize.X < aOnScreenAmount.X)
            {
                if (pos.Location.X < Size.X - aOnScreenAmount.X || pos.Location.X > Camera.Camera.ScreenRectangle.Width - aOnScreenAmount.X)
                {
                    xOutOfBounds = true;
                }
            }
            if (aOverlapSize.Y < aOnScreenAmount.Y)
            {
                if (pos.Location.Y < Size.Y - aOnScreenAmount.Y || pos.Location.Y > Camera.Camera.ScreenRectangle.Height - aOnScreenAmount.Y)
                {
                    yOutOfBounds = true;
                }
            }
            return (xOutOfBounds, yOutOfBounds);
        }

        public virtual void Resize(RelativeScreenPosition aSize)
        {
            relativeSize = aSize;
            pos.Size = aSize.ToAbsoluteScreenPos();
        }

        public virtual void Close()
        {
            for (int i = 0; i < children.Count; i++)
            {
                children[i].Close();
            }
        }

        public virtual void LeavingGameState()
        {
            for (int i = 0; i < children.Count; i++)
            {
                children[i].LeavingGameState();
            }
            heldEvents = null;
        }

        #endregion

        #region Release
        public virtual bool ReleasedOn(ReleaseEvent aRelease)
        {
            if (!visible) return false;
            if (!AbsolutePos.Contains(aRelease.AbsolutePos)) return false;


            if (ReleasedOnChildren(aRelease)) return true;

            ReleaseOnMe(aRelease);
            return capturesRelease;
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

        public virtual void ClickedOnAndReleasedOnMe() => heldEvents = null;
        protected virtual void HoldReleaseAwayFromMe() => heldEvents = null;
        #endregion

        #region Hover
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

        protected virtual void OnHover()
        {
            if (!visible) return;
        }

        protected virtual void OnDeHover()
        {
            if (!visible) return;
        }
        #endregion

        #region Click
        public virtual bool ClickedOn(ClickEvent aClick)
        {
            if (!visible && !(hudMoveable && hudMoving)) return false;

            if (!AbsolutePos.Contains(aClick.AbsolutePos)) return false;


            if (ClickedOnChildren(aClick)) return true;

            ClickedOnMe(aClick);
            return capturesClick;
        }

        protected virtual bool ClickedOnChildren(ClickEvent aClick)
        {
            if (!visible || (hudMoveable && hudMoving)) return false;

            for (int i = 0; i < children.Count; i++)
            {
                if (!children[i].ClickedOn(aClick)) continue;

                ClickedOnChild(aClick);
                return children[i].capturesClick;
            }

            return false;
        }

        protected virtual void ClickedOnChild(ClickEvent aClick)
        {
            if (!visible) return;
        }

        protected virtual void ClickedOnMe(ClickEvent aClick)
        {
            if (!visible && !(hudMoveable && hudMoving)) return;
            heldEvents = new HoldEvent(aClick, this);

            //DebugManager.Print(GetType(), "Clicked on " + pos);
        }
        #endregion

        #region Scroll
        internal virtual bool ScrolledOn(ScrollEvent aScrollEvent)
        {
            if (!visible) return false;

            if (!AbsolutePos.Contains(aScrollEvent.AbsolutePos)) return false;

            if (ScrolledOnChildren(aScrollEvent)) return true;

            ScrolledOnMe(aScrollEvent);
            return capturesScroll;
        }

        protected virtual void ScrolledOnMe(ScrollEvent aScrollEvent)
        {
            if (!visible) return;

        }

        protected virtual bool ScrolledOnChildren(ScrollEvent aScrollEvent)
        {
            if (!visible) return false;
            for (int i = 0; i < children.Count; i++)
            {
                if (!children[i].ScrolledOn(aScrollEvent)) continue;

                ScrolledOnChild(aScrollEvent);
                return children[i].capturesScroll;
            }
            return false;
        }

        protected virtual void ScrolledOnChild(ScrollEvent aScrollEvent)
        {
            
        }
        #endregion

        public void HudMovableDraw(SpriteBatch aBatch)
        {
            if (!hudMoveable) return;

            if (gfx != null)
            {
                gfx.Draw(aBatch, AbsolutePos);
            }

            movableGFX.Draw(aBatch, AbsolutePos);
            nameText.CentredDraw(aBatch, AbsolutePosition + Size / 2 - new AbsoluteScreenPosition(nameText.Offset.ToPoint()) / 2);

        }

        public virtual void Draw(SpriteBatch aBatch)
        {
            if (!visible) return;

            if (gfx != null)
            {

                gfx.Draw(aBatch, AbsolutePos);
            }
            
            if (children.Count == 0) return;

            GraphicsManager.CaptureScissor(this, AbsolutePos);
            foreach (UIElement child in children)
            {
                child.Draw(aBatch);
            }
            GraphicsManager.ReleaseScissor(this);
        }
    }
}
