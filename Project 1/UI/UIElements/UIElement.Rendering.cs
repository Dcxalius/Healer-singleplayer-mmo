using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Managers;
using System;
using System.Diagnostics;

namespace Project_1.UI.UIElements
{
    internal abstract partial class UIElement
    {
        protected bool ForceVolatileRender { get; set; }

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

            DrawSelf(aBatch);
            DrawChildren(aBatch);
        }

        internal void MarkRenderStale()
        {
            TouchInteraction();
        }

        protected virtual void DrawSelf(SpriteBatch aBatch)
        {
            if (gfx == null) return;
            gfx.Draw(aBatch, AbsolutePos);
        }

        protected void DrawChildren(SpriteBatch aBatch)
        {
            ThreadAffinity.AssertMainThread();
            if (children.Count == 0 && clipChildren.Count == 0) return;

            if (children.Count > 0)
            {
                GraphicsManager.CaptureScissor(this, AbsolutePos);
                try
                {
                    DrawChildList(aBatch, children);
                }
                finally
                {
                    GraphicsManager.ReleaseScissor(this);
                }
            }

            if (clipChildren.Count > 0)
            {
                DrawChildList(aBatch, clipChildren);
            }
        }

        protected virtual void DrawChildrenToCache(SpriteBatch aBatch)
        {
            DrawChildren(aBatch);
        }

        void DrawChildList(SpriteBatch batch, global::System.Collections.Generic.List<UIElement> childList)
        {
            for (int i = 0; i < childList.Count; i++)
            {
                UIElement child = childList[i];
                try
                {
                    child.Draw(batch);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"UI child draw failed '{child?.GetType().Name ?? "<null>"}': {ex.GetType().Name}: {ex.Message}");
                    Debug.WriteLine($"UI child draw failed '{child?.GetType().Name ?? "<null>"}' and was skipped: {ex}");
                }
            }
        }

        AbsoluteScreenPosition GetClipParentLocation(ClipAttachment attachment)
        {
            Microsoft.Xna.Framework.Rectangle ownerBounds = AbsolutePos;
            int start = (int)MathF.Round(attachment.Start);
            switch (attachment.Side)
            {
                case ClipSide.Left:
                    return new AbsoluteScreenPosition(0, ownerBounds.Y + start);
                case ClipSide.Right:
                    return new AbsoluteScreenPosition(ownerBounds.Right, ownerBounds.Y + start);
                case ClipSide.Top:
                    return new AbsoluteScreenPosition(ownerBounds.X + start, 0);
                case ClipSide.Bottom:
                    return new AbsoluteScreenPosition(ownerBounds.X + start, ownerBounds.Bottom);
                default:
                    return Location;
            }
        }

        AbsoluteScreenPosition GetClipParentSize(ClipAttachment attachment)
        {
            Microsoft.Xna.Framework.Rectangle ownerBounds = AbsolutePos;
            Microsoft.Xna.Framework.Point screen = Camera.Camera.WindowSize;
            int start = (int)MathF.Round(attachment.Start);
            switch (attachment.Side)
            {
                case ClipSide.Left:
                    return new AbsoluteScreenPosition(Math.Max(0, ownerBounds.Left), Math.Max(0, ownerBounds.Height - start));
                case ClipSide.Right:
                    return new AbsoluteScreenPosition(Math.Max(0, screen.X - ownerBounds.Right), Math.Max(0, ownerBounds.Height - start));
                case ClipSide.Top:
                    return new AbsoluteScreenPosition(Math.Max(0, ownerBounds.Width - start), Math.Max(0, ownerBounds.Top));
                case ClipSide.Bottom:
                    return new AbsoluteScreenPosition(Math.Max(0, ownerBounds.Width - start), Math.Max(0, screen.Y - ownerBounds.Bottom));
                default:
                    return Size;
            }
        }
    }
}
