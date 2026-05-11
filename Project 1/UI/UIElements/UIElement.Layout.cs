using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Input;

namespace Project_1.UI.UIElements
{
    internal abstract partial class UIElement
    {
        void GetVisibiltyPress()
        {
            if (!visibleKey.HasValue) return;
            if (UiKeyBindStateCache.GetPress(visibleKey.Value))
            {
                ToggleVisibilty();
            }
        }

        public void SetHudMoveable(bool aSet)
        {
            if (hudMoving == aSet) return;
            hudMoving = aSet;
            if (!hudMoveable) return;
            if (hudMoving)
            {
                oldPosition = RelativePos;
            }

            TouchInteraction();
            MarkRenderStale();
        }

        public void ResetHudMoveable()
        {
            if (!hudMoving) return;
            hudMoving = false;
            if (!hudMoveable) return;
            Move(oldPosition);
            TouchInteraction();
            MarkRenderStale();
        }

        public virtual void ToggleVisibilty()
        {
            Visible = !visible;
            TouchInteraction();
            MarkRenderStale();
        }

        public virtual void Rescale()
        {
            for (int i = 0; i < children.Count; i++)
            {
                children[i].Rescale();
            }

            nameText.Rescale();
            MarkRenderStale();
        }

        public void Move(RelativeScreenPosition aNewPos)
        {
            aNewPos.Assert();
            RelativeScreenPosition oldPos = relativePos;
            if (oldPos == aNewPos) return;
            relativePos = aNewPos;
            MoveBoundsCheck();
            if (relativePos != oldPos)
            {
                TouchInteraction();
                MarkRenderStale();
            }
        }

        public void Bump(AbsoluteScreenPosition aAmount)
        {
            RelativeScreenPosition relativeAmount = aAmount.ToRelativeScreenPosition();
            Move(RelativePos + relativeAmount);
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

            (bool xOutOfBounds, bool yOutOfBounds) = OutOfBoundsCheck(overlap.Size, alwaysOnScreenAmount);
            if (!xOutOfBounds && !yOutOfBounds) return;

            relativePos = new AbsoluteScreenPosition(GetNewMove(xOutOfBounds, yOutOfBounds, alwaysOnScreenAmount)).ToRelativeScreenPosition();
        }

        void AlwaysFullyOnScreenCheck()
        {
            if (!alwaysFullyOnScreen) return;

            Rectangle overlap = Rectangle.Intersect(Camera.Camera.ScreenRectangle, AbsolutePos);
            if (overlap == AbsolutePos) return;

            (bool xOutOfBounds, bool yOutOfBounds) = OutOfBoundsCheck(overlap.Size, Size);
            if (!xOutOfBounds && !yOutOfBounds) return;

            relativePos = new AbsoluteScreenPosition(GetNewMove(xOutOfBounds, yOutOfBounds, Size)).ToRelativeScreenPosition();
        }

        Point GetNewMove(bool xOutOfBounds, bool yOutOfBounds, Point alwaysOnScreenAmount)
        {
            Point returnable = Location;

            if (xOutOfBounds)
            {
                returnable.X = Location.X < 0
                    ? alwaysOnScreenAmount.X - Size.X
                    : Camera.Camera.ScreenRectangle.Width - alwaysOnScreenAmount.X;
            }

            if (yOutOfBounds)
            {
                returnable.Y = Location.Y < 0
                    ? alwaysOnScreenAmount.Y - Size.Y
                    : Camera.Camera.ScreenRectangle.Height - alwaysOnScreenAmount.Y;
            }

            return returnable;
        }

        (bool, bool) OutOfBoundsCheck(Point aOverlapSize, Point aOnScreenAmount)
        {
            bool xOutOfBounds = false;
            bool yOutOfBounds = false;

            if (aOverlapSize.X < aOnScreenAmount.X &&
                (Location.X < Size.X - aOnScreenAmount.X || Location.X > Camera.Camera.ScreenRectangle.Width - aOnScreenAmount.X))
            {
                xOutOfBounds = true;
            }

            if (aOverlapSize.Y < aOnScreenAmount.Y &&
                (Location.Y < Size.Y - aOnScreenAmount.Y || Location.Y > Camera.Camera.ScreenRectangle.Height - aOnScreenAmount.Y))
            {
                yOutOfBounds = true;
            }

            return (xOutOfBounds, yOutOfBounds);
        }

        public virtual void Resize(RelativeScreenPosition aSize)
        {
            aSize.Assert();
            if (relativeSize == aSize) return;
            relativeSize = aSize;
            TouchInteraction();
            MarkRenderStale();
        }

        public virtual void Resize(AbsoluteScreenPosition aSize) => Resize(aSize.ToRelativeScreenPosition(ParentSize));

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
    }
}
