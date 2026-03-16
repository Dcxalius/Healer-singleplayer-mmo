using Microsoft.Xna.Framework.Graphics;
using Project_1.Managers;

namespace Project_1.UI.UIElements
{
    internal abstract partial class UIElement
    {
        public void HudMovableDraw(SpriteBatch aBatch)
        {
            ThreadAffinity.AssertMainThread();
            if (!hudMoveable) return;

            if (gfx != null)
            {
                gfx.Draw(aBatch, AbsolutePos);
            }

            MovableGfx.Draw(aBatch, AbsolutePos);
            nameText.CentredDraw(aBatch, Location + Size / 2);
        }

        public virtual void Draw(SpriteBatch aBatch)
        {
            ThreadAffinity.AssertMainThread();
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
