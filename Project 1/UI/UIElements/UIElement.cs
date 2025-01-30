using Microsoft.Xna.Framework;
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
        #region Interactibility
        protected bool Visible
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

        protected bool capturesClick;
        protected bool capturesScroll;
        protected bool capturesRelease;

        #endregion

        #region Position
        public RelativeScreenPosition RelativePos => relativePos;
        public RelativeScreenPosition RelativeSize => relativeSize;

        public RelativeScreenPosition RelativePositionOnScreen => relativePos + parentPos.ToRelativeScreenPosition();

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

        public AbsoluteScreenPosition Location => absolutePos;

        AbsoluteScreenPosition absolutePos;
        
        RelativeScreenPosition relativePos; //TODO: Change this so 1, 1 refers to parents bottom right instead of screen bottom right + parent top left
        RelativeScreenPosition relativeSize; 
        public AbsoluteScreenPosition Size => new AbsoluteScreenPosition(pos.Size);
        Rectangle pos;
        #endregion

        #region Graphics
        public UITexture Gfx => gfx;
        protected UITexture gfx;
        public Color Color { get => gfx.Color; set => gfx.Color = value; }
        
        public float Layer
        {
            get => layer;
            set
            {
                layer = (float)value;
                for (int i = 0; i < children.Count; i++)
                {
                    children[i].layer = value + 0.02f;
                }
            }

        }
        float layer;
        #endregion

        public HoldEvent heldEvents;



        #region Parentage
        protected AbsoluteScreenPosition parentPos;
        List<UIElement> children = new List<UIElement>();
        protected int ChildCount => children.Count;

        protected void KillAllChildren() => children.Clear();
        protected void KillChild(int aIndex) => children.RemoveAt(aIndex);

        #endregion

        protected UIElement(UITexture aGfx, RelativeScreenPosition aPos, RelativeScreenPosition aSize) //aPos and aSize should be between 0 and 1
        {
            visible = true;
            gfx = aGfx;

            relativePos = aPos;
            relativeSize = aSize;

            pos = RelativeScreenPosition.TransformToAbsoluteRect(aPos, aSize);
            absolutePos = new AbsoluteScreenPosition(pos.Location);
            capturesClick = true;
            capturesScroll = false;
            capturesRelease = true;
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

        protected void AddChild(UIElement aUIElement)
        {
            if (aUIElement.gfx != null)
            {
                aUIElement.layer = layer + 0.02f;

            }
            aUIElement.Visible = Visible;
            children.Add(aUIElement);
        }

        protected void AddChildren(UIElement[] aUIElement)
        {
            for (int i = 0; i < aUIElement.Length; i++)
            {
                AddChild(aUIElement[i]);
            }
        }

        protected void AddChildren<T>(List<T> aUIElement) where T : UIElement
        {
            for (int i = 0; i < aUIElement.Count; i++)
            {
                AddChild(aUIElement[i]);
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

        #region Change
        void GetVisibiltyPress()
        {
            if (!visibleKey.HasValue) return;
            if (KeyBindManager.GetPress(visibleKey.Value))
            {
                ToggleVisibilty();
            }
        }

        public virtual void ToggleVisibilty()
        {
            visible = !visible;
            foreach (UIElement child in children)
            {
                child.Visible = visible;
            }
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

        public virtual void Close()
        {
            for (int i = 0; i < children.Count; i++)
            {
                children[i].Close();
            }
        }

        public virtual void PauseMenuActivated()
        {
            for (int i = 0; i < children.Count; i++)
            {
                children[i].PauseMenuActivated();
            }
            heldEvents = null;
        }

        #endregion

        static protected Point TransformFromRelativeToPoint(Vector2 aValue) //TODO: ????
        { 
            Point size = new Point((int)(Camera.Camera.ScreenSize.X * aValue.X), (int)(Camera.Camera.ScreenSize.Y * aValue.Y));
            return size;
        }


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

        public virtual void HoldReleaseOnMe() => heldEvents = null;
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
            if (!visible) return false;

            if (!AbsolutePos.Contains(aClick.AbsolutePos)) return false;


            if (ClickedOnChildren(aClick)) return true;

            ClickedOnMe(aClick);
            return capturesClick;
        }

        protected virtual bool ClickedOnChildren(ClickEvent aClick)
        {
            if (!visible) return false;

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
            if (!visible) return;
            heldEvents = new HoldEvent(TimeManager.TotalFrameTime, aClick, this);

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

        public virtual void Draw(SpriteBatch aBatch)
        {
            if (!visible) return;

            if (gfx != null)
            {

                gfx.Draw(aBatch, AbsolutePos);
            }

            GraphicsManager.CaptureScissor(this, AbsolutePos);
            foreach (UIElement child in children)
            {
                child.Draw(aBatch);
            }
            GraphicsManager.ReleaseScissor(this);
        }
    }
}
