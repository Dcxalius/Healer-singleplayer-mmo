using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Content.Input;
using Project_1.Managers;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI
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


        UITexture gfx;
        protected Rectangle pos;
        Vector2 relativePos;
        Vector2 relativeSize;

        public HoldEvent heldEvents; //TODO: This should prob be cleanse on state change
        

        protected List<UIElement> children = new List<UIElement>();

        protected UIElement(UITexture aGfx, Vector2 aPos, Vector2 aSize) //aPos and aSize should be between 0 and 1
        {
            //Debug.Assert(aPos > 0 && aPos < 0);
            gfx = aGfx;

            relativePos = aPos;
            relativeSize = aSize;

            pos = TransformFromRelativeToValues(aPos, aSize);
        }

        static protected Rectangle TransformFromRelativeToValues(Vector2 aPos, Vector2 aSize)
        {
            return new Rectangle((Camera.ScreenRectangle.Size.ToVector2() * aPos).ToPoint(), (Camera.ScreenRectangle.Size.ToVector2() * aSize).ToPoint());
        }

        static protected Point TransformFromRelativeToPoint(Vector2 aValue)
        {
            return (Camera.ScreenRectangle.Size.ToVector2() * aValue).ToPoint();
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

        public virtual void Update()
        {
            HoldUpdate();

            foreach (UIElement child in children)
            {
                child.Update();
            }

        }

        public virtual void Draw(SpriteBatch aBatch)
        {
            gfx.Draw(aBatch, pos);

            foreach (UIElement child in children)
            {
                child.Draw(aBatch);
            }
        }

        public bool ClickedOn(ClickEvent aClick)
        {
            if (pos.Contains(aClick.ClickPos))
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
            heldEvents = new HoldEvent(TimeManager.gt.TotalGameTime ,aClick, this);

            DebugManager.Print(GetType(), "Clicked on " + pos);
        }
    }
}
