using Project_1.Input;
using Project_1.Managers;
using Project_1.Messaging;
using Project_1.Messaging.Events;

namespace Project_1.UI.UIElements
{
    internal abstract partial class UIElement
    {
        public virtual void Update()
        {
            ThreadAffinity.AssertUiThread();
            HoldUpdate();
            UpdateChildren();
            HoverUpdate();
            GetVisibiltyPress();
        }

        public void HUDMovableUpdate()
        {
            ThreadAffinity.AssertUiThread();
            if (!hudMoveable) return;
            HoldUpdate();
        }

        void UpdateChildren()
        {
            for (int i = 0; i < children.Count; i++)
            {
                children[i].Update();
            }

            for (int i = 0; i < clipChildren.Count; i++)
            {
                clipChildren[i].Update();
            }
        }

        protected virtual void HoldUpdate()
        {
            if (heldEvents == null) return;
            if (!Visible) return;

            if (!heldEvents.IsStillHeld())
            {
                Released();
                if (isHovered)
                {
                    ClickedOnAndReleasedOnMe();
                    return;
                }

                HoldReleaseAwayFromMe();
                return;
            }

            if (!Dragable && !hudMoving) return;
            if (heldEvents.DurationHeld < timeBeforeDragRegisters.TotalSeconds) return;
            Move(UiMouseStateCache.Relative - heldEvents.Offset);
        }

        protected virtual void Released()
        {
        }

        public virtual bool ReleasedOn(ReleaseEvent aRelease)
        {
            if (!visible) return false;
            if (!ContainsInputPoint(aRelease.AbsolutePos.ToPoint())) return false;
            if (ReleasedOnChildren(aRelease)) return true;

            ReleaseOnMe(aRelease);
            return capturesRelease;
        }

        public bool ReleasedOnChildren(ReleaseEvent aRelease)
        {
            if (!visible) return false;
            for (int i = 0; i < children.Count; i++)
            {
                if (!children[i].ReleasedOn(aRelease)) continue;

                ReleasedOnChild(aRelease);
                return true;
            }

            for (int i = 0; i < clipChildren.Count; i++)
            {
                if (!clipChildren[i].ReleasedOn(aRelease)) continue;

                ReleasedOnChild(aRelease);
                return true;
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
            TouchInteraction();
            if (parent != null || !hudMoveable || !hudMoving) return;
            MailboxManager.PublishUiEvent(new HudSizeChangerSet(UiElementId));
        }

        protected virtual void HoldReleaseAwayFromMe()
        {
            heldEvents = null;
            TouchInteraction();
        }

        void HoverUpdate()
        {
            if (!Visible && !(hudMoveable && hudMoving)) return;
            if (!isHovered && Hovered)
            {
                isHovered = true;
                OnHover();
                TouchInteraction();
                return;
            }

            if (isHovered && !Hovered)
            {
                isHovered = false;
                OnDeHover();
                TouchInteraction();
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

        public virtual bool ClickedOn(ClickEvent aClick)
        {
            if (!visible && !(hudMoveable && hudMoving)) return false;
            if (!ContainsInputPoint(aClick.AbsolutePos.ToPoint())) return false;
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

            for (int i = 0; i < clipChildren.Count; i++)
            {
                if (!clipChildren[i].ClickedOn(aClick)) continue;

                ClickedOnChild(aClick);
                return clipChildren[i].capturesClick;
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
            TouchInteraction();
        }

        internal virtual bool ScrolledOn(ScrollEvent aScrollEvent)
        {
            if (!visible) return false;
            if (!ContainsInputPoint(aScrollEvent.AbsolutePos.ToPoint())) return false;
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

            for (int i = 0; i < clipChildren.Count; i++)
            {
                if (!clipChildren[i].ScrolledOn(aScrollEvent)) continue;

                ScrolledOnChild(aScrollEvent);
                return clipChildren[i].capturesScroll;
            }

            return false;
        }

        protected virtual void ScrolledOnChild(ScrollEvent aScrollEvent)
        {
        }

        bool ContainsInputPoint(Microsoft.Xna.Framework.Point point)
        {
            if (AbsolutePos.Contains(point)) return true;

            for (int i = 0; i < clipChildren.Count; i++)
            {
                if (clipChildren[i].AbsolutePos.Contains(point)) return true;
            }

            return false;
        }
    }
}
