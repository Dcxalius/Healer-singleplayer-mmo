using Microsoft.Xna.Framework;
using Project_1.Camera;
using System;

namespace Project_1.Rendering
{
    internal sealed class Camera2D : Camera
    {
        public WorldSpace Position => position;
        WorldSpace position;

        public float RotationRadians => rotationRadians;
        float rotationRadians;

        public float Zoom => zoom;
        float zoom;

        public float NearPlane => nearPlane;
        float nearPlane;

        public float FarPlane => farPlane;
        float farPlane;

        public Camera2D(AbsoluteScreenPosition aViewportSize)
            : this(aViewportSize, WorldSpace.Zero, 1f, 0f)
        {
        }

        public Camera2D(AbsoluteScreenPosition aViewportSize, WorldSpace aPosition, float aZoom = 1f, float aRotationRadians = 0f, float aNearPlane = -1f, float aFarPlane = 1f)
            : base(aViewportSize)
        {
            position = aPosition;
            rotationRadians = aRotationRadians;
            zoom = Math.Max(0.0001f, aZoom);
            nearPlane = aNearPlane;
            farPlane = aFarPlane;
            Refresh();
        }

        public void MoveTo(WorldSpace aPosition)
        {
            position = aPosition;
            Refresh();
        }

        public void RotateTo(float aRotationRadians)
        {
            rotationRadians = aRotationRadians;
            Refresh();
        }

        public void SetZoom(float aZoom)
        {
            zoom = Math.Max(0.0001f, aZoom);
            Refresh();
        }

        public void SetDepthRange(float aNearPlane, float aFarPlane)
        {
            nearPlane = aNearPlane;
            farPlane = aFarPlane;
            Refresh();
        }

        public AbsoluteScreenPosition WorldToScreen(WorldSpace aWorldPosition)
        {
            Vector4 clip = Vector4.Transform(new Vector4(aWorldPosition.X, aWorldPosition.Y, 0f, 1f), ViewProjection);
            return ClipToScreen(clip);
        }

        public WorldSpace ScreenToWorld(AbsoluteScreenPosition aScreenPosition)
        {
            Vector3 world = Unproject(aScreenPosition, 0f);
            return new WorldSpace(world.X, world.Y);
        }

        protected override Matrix BuildView()
        {
            return
                Matrix.CreateTranslation(-position.X, -position.Y, 0f) *
                Matrix.CreateRotationZ(-rotationRadians) *
                Matrix.CreateScale(zoom, zoom, 1f);
        }

        protected override Matrix BuildProjection()
        {
            float halfWidth = ViewportSize.X / 2f;
            float halfHeight = ViewportSize.Y / 2f;
            return Matrix.CreateOrthographicOffCenter(-halfWidth, halfWidth, halfHeight, -halfHeight, nearPlane, farPlane);
        }
    }
}
