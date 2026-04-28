using Microsoft.Xna.Framework;
using Project_1.Camera;
using System;

namespace Project_1.System.Models
{
    internal class Joint3D : Joint
    {
        public WorldSpace3D Anchor
        {
            get
            {
                if (Parent == null) return anchor;
                return Parent.Anchor + Parent.DirectionVector * distanceUpParent;
            }
        }
        WorldSpace3D anchor;

        float distanceUpParent;
        float length;
        WorldSpace3D directionVector = (WorldSpace3D)Vector3.UnitX;

        public float Length => length;

        public WorldSpace3D DirectionVector => directionVector;

        /// <summary>
        /// In radians around the vertical axis, where 0 points along +X.
        /// </summary>
        public float Yaw => (float)Math.Atan2(directionVector.Z, directionVector.X);

        /// <summary>
        /// In radians above the XZ plane, where positive points upward.
        /// </summary>
        public float Pitch
        {
            get
            {
                float planarLength = (float)Math.Sqrt(directionVector.X * directionVector.X + directionVector.Z * directionVector.Z);
                return (float)Math.Atan2(directionVector.Y, planarLength);
            }
        }

        public new Joint3D Parent => (Joint3D)base.Parent;

        public Joint3D(Joint3D aParent, float aDistanceUpParent, float aLength) : base(aParent)
        {
            distanceUpParent = aDistanceUpParent;
            length = aLength;
        }

        public Joint3D(WorldSpace3D aAnchor, float aLength) : base(null)
        {
            anchor = aAnchor;
            length = aLength;
        }

        protected void StretchTo(WorldSpace3D aWorldPosition)
        {
            PointAt(aWorldPosition);
            length = aWorldPosition.DistanceTo(Anchor);
        }

        protected void PointAt(WorldSpace3D aWorldPosition)
        {
            WorldSpace3D delta = aWorldPosition - Anchor;
            if (delta.ToVector3().LengthSquared() <= float.Epsilon)
            {
                return;
            }

            delta.Normalize();
            directionVector = delta;
        }

        public WorldSpace3D TransformLocalDirection(WorldSpace3D aLocalDirection)
        {
            Vector3 forward = DirectionVector.ToVector3();
            if (forward.LengthSquared() <= float.Epsilon)
            {
                return aLocalDirection;
            }

            forward.Normalize();

            Vector3 worldUp = Vector3.Up;
            if (Math.Abs(Vector3.Dot(forward, worldUp)) > 0.999f)
            {
                worldUp = Vector3.Forward;
            }

            Vector3 right = Vector3.Cross(worldUp, forward);
            if (right.LengthSquared() <= float.Epsilon)
            {
                right = Vector3.Right;
            }
            right.Normalize();

            Vector3 up = Vector3.Cross(forward, right);
            if (up.LengthSquared() <= float.Epsilon)
            {
                up = Vector3.Up;
            }
            up.Normalize();

            Vector3 local = aLocalDirection.ToVector3();
            Vector3 transformed = right * local.X + up * local.Y + forward * local.Z;
            return (WorldSpace3D)transformed;
        }

        public WorldSpace3D TransformLocalPoint(WorldSpace3D aLocalOffset)
        {
            return Anchor + TransformLocalDirection(aLocalOffset);
        }
    }
}
