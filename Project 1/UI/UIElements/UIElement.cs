using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

namespace Project_1.UI.UIElements
{
    internal abstract class UIElement
    {
        public Vector2 RelativePos
        {
            get => relativePos;
        }
        public Vector2 RelativeSize
        {
            get => relativeSize;
        }

        public Rectangle AbsolutePos
        {
            get
            {
                Rectangle tempRec = Rectangle.Empty;
                tempRec.Location = absolutePos;
                tempRec.Size = Size;
                return tempRec;
            }
        }

        public Point Size { get => pos.Size; }

        UITexture gfx;
        Point absolutePos;
        protected Rectangle pos;
        Vector2 relativePos;
        Vector2 relativeSize;

        public HoldEvent heldEvents; //TODO: This should prob be cleansed on state change

        //protected UIElement? parent;
        protected Point parentPos = Point.Zero;
        protected List<UIElement> children = new List<UIElement>();

        protected UIElement(UITexture aGfx, Vector2 aPos, Vector2 aSize) //aPos and aSize should be between 0 and 1
        {
            //Debug.Assert(aPos > 0 && aPos < 0);

            gfx = aGfx;

            relativePos = aPos;
            relativeSize = aSize;

            pos = TransformFromRelativeToValues(aPos, aSize);
            absolutePos = pos.Location;
        }

        static protected Rectangle TransformFromRelativeToValues(Vector2 aPos, Vector2 aSize)
        {
            Point pos = new Point((int)(Camera.ScreenSize.X * aPos.X), (int)(Camera.ScreenSize.Y * aPos.Y));
            Point size = new Point((int)(Camera.ScreenSize.X * aSize.X), (int)(Camera.ScreenSize.Y * aSize.Y));
            return new Rectangle(pos, size);
        }

        static protected Point TransformFromRelativeToPoint(Vector2 aValue)
        {
            Point size = new Point((int)(Camera.ScreenSize.X * aValue.X), (int)(Camera.ScreenSize.Y * aValue.Y));
            return size;
        }

        public virtual void Close()
        {

        }

        public virtual void HoldUpdate()
        {
            if (heldEvents == null)
            {
                return;
            }

            if (!heldEvents.IsStillHeld())
            {
                HoldReleaseOnMe();
            }
        }

        public virtual void Update(in UIElement aParent)
        {
            HoldUpdate();

            if (aParent != null)
            {
                absolutePos = aParent.absolutePos + pos.Location;
            }
            else
            {
                absolutePos = pos.Location;
            }
            foreach (UIElement child in children)
            {
                child.Update(this);
            }


            if (AbsolutePos.Contains(InputManager.GetMousePosAbsolute()))
            {
                OnHover();
            }
        }


        public bool ClickedOn(ClickEvent aClick)
        {
            if (AbsolutePos.Contains(aClick.ClickPoint))
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

        public virtual void HoldReleaseOnMe()
        {
            heldEvents = null;
        }

        public virtual void OnHover()
        {
            //DebugManager.Print(GetType(), "Hovered on");
        }

        public bool ClickedOnChildren(ClickEvent aClick)
        {

            for (int i = 0; i < children.Count; i++)
            {
                bool clickedOn = children[i].ClickedOn(aClick);
                if (clickedOn)
                {
                    return true;

                }
            }

            return false;
        }

        public virtual void ClickedOnMe(ClickEvent aClick)
        {
            heldEvents = new HoldEvent(TimeManager.gt.TotalGameTime, aClick, this);

            //DebugManager.Print(GetType(), "Clicked on " + pos);
        }

        public virtual void Rescale()
        {
            pos = TransformFromRelativeToValues(relativePos, relativeSize);

            for (int i = 0; i < children.Count; i++)
            {
                children[i].Rescale();
            }
        }

        public virtual void Draw(SpriteBatch aBatch)
        {
            if (gfx != null)
            {

                gfx.Draw(aBatch, AbsolutePos);
            }

            foreach (UIElement child in children)
            {
                child.Draw(aBatch);
            }
        }
    }
}
