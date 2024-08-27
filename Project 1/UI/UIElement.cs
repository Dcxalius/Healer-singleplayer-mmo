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
        UITexture gfx;
        protected Rectangle pos;
        public HoldEvent heldEvents; //TODO: This should prob be cleanse on state change


        protected List<UIElement> children = new List<UIElement>();

        protected UIElement(UITexture aGfx, Vector2 aPos, Vector2 aSize) //aPos and aSize should be between 0 and 1
        {
            //Debug.Assert(aPos > 0 && aPos < 0);
            gfx = aGfx;

            pos = new Rectangle((Camera.screenBorder.ToVector2() * aPos).ToPoint(), (Camera.screenBorder.ToVector2() * aSize).ToPoint());
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
