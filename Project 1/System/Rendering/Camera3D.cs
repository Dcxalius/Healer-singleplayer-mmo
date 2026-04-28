using Microsoft.Xna.Framework;
using Project_1.Camera;
using System;

namespace Project_1.Rendering
{
    internal sealed class Camera3D : Camera
    {
        public WorldSpace3D Position => position;
        WorldSpace3D position;

        public WorldSpace3D Target => target;
        WorldSpace3D target;

        public WorldSpace3D Up => up;
        WorldSpace3D up;

        public float FieldOfViewRadians => fieldOfViewRadians;
        float fieldOfViewRadians;

        public float NearPlane => nearPlane;
        float nearPlane;

        public float FarPlane => farPlane;
        float farPlane;

        public WorldSpace3D Forward
        {
            get
            {
                Vector3 forward = target.ToVector3() - position.ToVector3();
                if (forward.LengthSquared() <= float.Epsilon) return new WorldSpace3D(0f, 0f, -1f);
                forward.Normalize();
                return new WorldSpace3D(forward);
            }
        }

        public Camera3D(AbsoluteScreenPosition aViewportSize)
            : this(aViewportSize, new WorldSpace3D(0f, 0f, 10f), WorldSpace3D.Zero)
        {
        }

        public Camera3D(
            AbsoluteScreenPosition aViewportSize,
            WorldSpace3D aPosition,
            WorldSpace3D aTarget,
            WorldSpace3D? aUp = null,
            float aFieldOfViewRadians = MathHelper.PiOver4,
            float aNearPlane = 0.1f,
            float aFarPlane = 2048f)
            : base(aViewportSize)
        {
            position = aPosition;
            target = aTarget;
            up = aUp ?? new WorldSpace3D(Vector3.Up);
            fieldOfViewRadians = aFieldOfViewRadians;
            nearPlane = Math.Max(0.001f, aNearPlane);
            farPlane = Math.Max(nearPlane + 0.001f, aFarPlane);
            Refresh();
        }

        public void MoveTo(WorldSpace3D aPosition)
        {
            position = aPosition;
            Refresh();
        }

        public void LookAt(WorldSpace3D aTarget)
        {
            target = aTarget;
            Refresh();
        }

        public void SetUp(WorldSpace3D aUp)
        {
            up = aUp == WorldSpace3D.Zero ? new WorldSpace3D(Vector3.Up) : WorldSpace3D.Normalize(aUp);
            Refresh();
        }

        public void SetLens(float aFieldOfViewRadians, float aNearPlane, float aFarPlane)
        {
            fieldOfViewRadians = aFieldOfViewRadians;
            nearPlane = Math.Max(0.001f, aNearPlane);
            farPlane = Math.Max(nearPlane + 0.001f, aFarPlane);
            Refresh();
        }

        public AbsoluteScreenPosition WorldToScreen(WorldSpace3D aWorldPosition)
        {
            Vector4 clip = Vector4.Transform(new Vector4(aWorldPosition.ToVector3(), 1f), ViewProjection);
            return ClipToScreen(clip);
        }

        public WorldSpace3D ScreenToWorld(AbsoluteScreenPosition aScreenPosition, float aDepth)
        {
            return new WorldSpace3D(Unproject(aScreenPosition, aDepth));
        }

        public Ray ScreenPointToRay(AbsoluteScreenPosition aScreenPosition)
        {
            Vector3 nearPoint = Unproject(aScreenPosition, 0f);
            Vector3 farPoint = Unproject(aScreenPosition, 1f);
            Vector3 direction = farPoint - nearPoint;
            if (direction.LengthSquared() <= float.Epsilon) direction = Forward.ToVector3();
            direction.Normalize();
            return new Ray(nearPoint, direction);
        }

        protected override Matrix BuildView()
        {
            Vector3 viewPosition = position.ToVector3();
            Vector3 viewTarget = target.ToVector3();
            if ((viewTarget - viewPosition).LengthSquared() <= float.Epsilon)
            {
                viewTarget = viewPosition + new Vector3(0f, 0f, -1f);
            }

            Vector3 viewUp = up == WorldSpace3D.Zero ? Vector3.Up : up.ToVector3();
            if (viewUp.LengthSquared() <= float.Epsilon) viewUp = Vector3.Up;
            viewUp.Normalize();

            return Matrix.CreateLookAt(viewPosition, viewTarget, viewUp);
        }

        protected override Matrix BuildProjection()
        {
            float safeFieldOfView = MathHelper.Clamp(fieldOfViewRadians, 0.01f, MathHelper.Pi - 0.01f);
            return Matrix.CreatePerspectiveFieldOfView(safeFieldOfView, AspectRatio, nearPlane, farPlane);
        }
    }
}
