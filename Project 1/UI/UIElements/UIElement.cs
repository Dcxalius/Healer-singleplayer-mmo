using Microsoft.Win32.SafeHandles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Input;
using Project_1.Managers;
using Project_1.Textures;
using Project_1.UI.HUD;
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
        protected bool hudMoving; //Todo: Change name on these, this means the hud is in movable state, maybe even move this out
        public bool HudMoveable => hudMoveable;
        protected bool hudMoveable; //Todo: Change name on these, this means the element is movable
        static UITexture movableGFX => new UITexture("MovableHUD", Color.White);
        Text nameText;

        readonly TimeSpan timeBeforeDragRegisters = TimeSpan.FromSeconds(0.2);

        public HoldEvent heldEvents;
        #endregion

        #region Position
        public RelativeScreenPosition RelativePos => relativePos;
        RelativeScreenPosition relativePos; //TODO: Change this so 1, 1 refers to parents bottom right instead of screen bottom right + parent top left
        public RelativeScreenPosition RelativeSize => relativeSize;
        RelativeScreenPosition relativeSize;

        public RelativeScreenPosition RelativePositionOnScreen => relativePos + ParentPos.ToRelativeScreenPosition();

        public Rectangle AbsolutePos
        {
            get
            {
                Rectangle tempRec = Rectangle.Empty;
                tempRec.Location = Location;
                tempRec.Size = Size;
                return tempRec;
            }
        }

        public AbsoluteScreenPosition Location => (ParentPos) + (RelativePos * ParentRelativeSize).ToAbsoluteScreenPos();
        public AbsoluteScreenPosition Size => (RelativeSize * ParentRelativeSize).ToAbsoluteScreenPos(); //TODO: This is wrong, it needs to include grandparents
        #endregion

        #region Graphics
        public UITexture Gfx => gfx;
        protected UITexture gfx;
        public virtual Color Color { get => gfx.Color; set => gfx.Color = value; } 
        #endregion

        #region Parentage
        protected UIElement parent;
        protected AbsoluteScreenPosition ParentPos => parent == null ? AbsoluteScreenPosition.Zero : parent.Location;
        protected RelativeScreenPosition ParentRelativePos => parent == null ? RelativeScreenPosition.Zero : parent.RelativePos;
        protected AbsoluteScreenPosition ParentSize => parent == null ? new AbsoluteScreenPosition(1) : parent.Size;
        protected RelativeScreenPosition ParentRelativeSize => parent == null ? new RelativeScreenPosition(1) : parent.Size.ToRelativeScreenPosition();

        List<UIElement> children = new List<UIElement>();
        protected int ChildCount => children.Count;

        protected virtual void KillAllChildren() => children.Clear();
        protected virtual void KillChild(int aIndex) => children.RemoveAt(aIndex);
        protected virtual void KillChild(UIElement aChild) => children.Remove(aChild);
        protected UIElement GetChild(int aIndex) => children[aIndex];
        protected int GetChildID(UIElement aChild) => children.IndexOf(aChild);
        protected void ForAllChildren(Action<UIElement> aAction)
        {
            for (int i = 0; i < children.Count; i++)
            {
                aAction(children[i]);
            }
        }

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

        public (string, RelativeScreenPosition, RelativeScreenPosition) Save => (GetType().Name, RelativePos, RelativeSize);

        protected UIElement(UITexture aGfx, RelativeScreenPosition aPos, RelativeScreenPosition aSize) //aPos and aSize should be between 0 and 1
        {
            //TODO: Make it so aPos and aSize is parent relative
            //If no parent 0 0 should refer to top left of game screen and 1 1 should refer to bottom right of game screen
            //If parent 0 0 should refer to top left of parent and 1 1 should refer to bottom right of game screen
            visible = true;
            gfx = aGfx;

            relativePos = aPos;
            relativeSize = aSize;

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
            //pos = RelativeScreenPosition.TransformToAbsoluteRect(relativePos, relativeSize);

            for (int i = 0; i < children.Count; i++)
            {
                children[i].Rescale();
            }

            nameText.Rescale();
        }

        public void Move(RelativeScreenPosition aNewPos)
        {
            if (aNewPos.X == float.NaN || aNewPos.Y == float.NaN) throw new ArgumentException("Invalid move.");
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

            Rectangle overlap = Rectangle.Intersect(Camera.Camera.ScreenRectangle, AbsolutePos);

            if (overlap == AbsolutePos) return;

            (bool, bool) outOfBounds = OutOfBoundsCheck(overlap.Size, alwaysOnScreenAmount);
            bool xOutOfBounds = outOfBounds.Item1;
            bool yOutOfBounds = outOfBounds.Item2;

            if (!xOutOfBounds && !yOutOfBounds) return;


            relativePos = new AbsoluteScreenPosition( GetNewMove(xOutOfBounds, yOutOfBounds, alwaysOnScreenAmount)).ToRelativeScreenPosition();
        }

        void AlwaysFullyOnScreenCheck()
        {
            if (!alwaysFullyOnScreen) return;

            Rectangle overlap = Rectangle.Intersect(Camera.Camera.ScreenRectangle, AbsolutePos);

            if (overlap == AbsolutePos) return;

            (bool, bool) outOfBounds = OutOfBoundsCheck(overlap.Size, Size);
            bool xOutOfBounds = outOfBounds.Item1;
            bool yOutOfBounds = outOfBounds.Item2;

            if (!xOutOfBounds && !yOutOfBounds) return;


            relativePos = new AbsoluteScreenPosition(GetNewMove(xOutOfBounds, yOutOfBounds, Size)).ToRelativeScreenPosition();

            //relativePos = new AbsoluteScreenPosition(Location).ToRelativeScreenPosition();
        }

        Point GetNewMove(bool xOutOfBounds, bool yOutOfBounds, Point alwaysOnScreenAmount)
        {
            Point returnable = Location;

            if (xOutOfBounds)
            {
                if (Location.X < 0)
                {
                    returnable.X = alwaysOnScreenAmount.X - Size.X;
                }
                else
                {
                    returnable.X = Camera.Camera.ScreenRectangle.Width - alwaysOnScreenAmount.X;
                }
            }

            if (yOutOfBounds)
            {
                if (Location.Y < 0)
                {
                    returnable.Y = alwaysOnScreenAmount.Y - Size.Y;
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
                if (Location.X < Size.X - aOnScreenAmount.X || Location.X > Camera.Camera.ScreenRectangle.Width - aOnScreenAmount.X)
                {
                    xOutOfBounds = true;
                }
            }
            if (aOverlapSize.Y < aOnScreenAmount.Y)
            {
                if (Location.Y < Size.Y - aOnScreenAmount.Y || Location.Y > Camera.Camera.ScreenRectangle.Height - aOnScreenAmount.Y)
                {
                    yOutOfBounds = true;
                }
            }
            return (xOutOfBounds, yOutOfBounds);
        }

        public virtual void Resize(RelativeScreenPosition aSize) => relativeSize = aSize;

        public virtual void Resize(AbsoluteScreenPosition aSize) => relativeSize = aSize.ToRelativeScreenPosition();

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

        public virtual void ClickedOnAndReleasedOnMe()
        {
            heldEvents = null;
            if (parent != null || !hudMoveable || !hudMoving) return;
            HUDManager.SetSizeChanger(this);
        }

        protected virtual void HoldReleaseAwayFromMe() => heldEvents = null;
        #endregion

        #region Hover
        void HoverUpdate()
        {
            if (!visible && (!hudMoveable && !hudMoving)) return;

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
            nameText.CentredDraw(aBatch, Location + Size / 2 - new AbsoluteScreenPosition(nameText.Offset.ToPoint()) / 2);

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
