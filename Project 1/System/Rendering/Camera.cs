using Microsoft.Xna.Framework;
using Project_1.Camera;
using System;

namespace Project_1.Rendering
{
    internal abstract class Camera
    {
        public AbsoluteScreenPosition ViewportSize => viewportSize;
        AbsoluteScreenPosition viewportSize;

        public Matrix View => view;
        Matrix view;

        public Matrix Projection => projection;
        Matrix projection;

        public Matrix ViewProjection => view * projection;

        public float AspectRatio => viewportSize.Y <= 0 ? 1f : viewportSize.X / (float)viewportSize.Y;

        protected Camera(AbsoluteScreenPosition aViewportSize)
        {
            viewportSize = SanitizeViewport(aViewportSize);
        }

        public void Resize(AbsoluteScreenPosition aViewportSize)
        {
            viewportSize = SanitizeViewport(aViewportSize);
            RebuildMatrices();
        }

        public void Refresh()
        {
            RebuildMatrices();
        }

        protected abstract Matrix BuildView();

        protected abstract Matrix BuildProjection();

        protected AbsoluteScreenPosition ClipToScreen(Vector4 aClipPosition)
        {
            float safeW = Math.Abs(aClipPosition.W) < float.Epsilon ? 1f : aClipPosition.W;
            float ndcX = aClipPosition.X / safeW;
            float ndcY = aClipPosition.Y / safeW;

            int screenX = (int)Math.Round((ndcX + 1f) * 0.5f * viewportSize.X);
            int screenY = (int)Math.Round((1f - ndcY) * 0.5f * viewportSize.Y);
            return new AbsoluteScreenPosition(screenX, screenY);
        }

        protected Vector3 Unproject(AbsoluteScreenPosition aScreenPosition, float aDepth)
        {
            float depth = MathHelper.Clamp(aDepth, 0f, 1f);
            float width = Math.Max(1f, viewportSize.X);
            float height = Math.Max(1f, viewportSize.Y);

            Vector3 clipSpace = new Vector3(
                aScreenPosition.X / width * 2f - 1f,
                1f - aScreenPosition.Y / height * 2f,
                depth);

            Matrix inverseViewProjection = Matrix.Invert(ViewProjection);
            Vector4 world = Vector4.Transform(new Vector4(clipSpace, 1f), inverseViewProjection);
            float safeW = Math.Abs(world.W) < float.Epsilon ? 1f : world.W;
            return new Vector3(world.X / safeW, world.Y / safeW, world.Z / safeW);
        }

        void RebuildMatrices()
        {
            view = BuildView();
            projection = BuildProjection();
        }

        static AbsoluteScreenPosition SanitizeViewport(AbsoluteScreenPosition aViewportSize)
        {
            int safeX = Math.Max(1, aViewportSize.X);
            int safeY = Math.Max(1, aViewportSize.Y);
            return new AbsoluteScreenPosition(safeX, safeY);
        }
    }
}
