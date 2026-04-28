using Project_1.Camera;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.System.Models
{
    internal class Joint2D : Joint
    {
        public WorldSpace Anchor
        {
            get
            {
                if (Parent == null) return anchor;
                return new WorldSpace(Parent.Anchor.X + Parent.DirectionVector.X * distanceUpParent, Parent.Anchor.Y + Parent.DirectionVector.Y * distanceUpParent);
            }
        }
        private WorldSpace anchor;

        private float distanceUpParent;
        private float length;

        public float Length => length;

        public WorldSpace DirectionVector => new WorldSpace((float)Math.Cos(rotation), (float)Math.Sin(rotation));

        /// <summary>
        /// In radians, where 0 is pointing to the right, and positive is counter-clockwise.
        /// </summary>
        public float Rotation => rotation;
        float rotation;

        public new Joint2D Parent => (Joint2D)base.Parent;

        public Joint2D(Joint2D aParent, float aDistanceUpParent, float aLength) : base(aParent)
        {
            distanceUpParent = aDistanceUpParent;
            length = aLength;
        }

        public Joint2D(WorldSpace aAnchor, float aLength) : base(null)
        {
            length = aLength;
            this.anchor = aAnchor;
        }

        protected void StrechTo(WorldSpace aWorldSpace)
        {
            PointAt(aWorldSpace);
            length = (float)Math.Sqrt(Math.Pow(aWorldSpace.X - Anchor.X, 2) + Math.Pow(aWorldSpace.Y - Anchor.Y, 2));
            
        }

        protected void PointAt(WorldSpace aWorldSpace)
        {
            rotation = (float)Math.Atan2(aWorldSpace.Y - Anchor.Y, aWorldSpace.X - Anchor.X);
        }

        public WorldSpace TransformLocalDirection(WorldSpace aLocalDirection)
        {
            float cos = (float)Math.Cos(rotation);
            float sin = (float)Math.Sin(rotation);
            return new WorldSpace(
                aLocalDirection.X * cos - aLocalDirection.Y * sin,
                aLocalDirection.X * sin + aLocalDirection.Y * cos);
        }

        public WorldSpace TransformLocalPoint(WorldSpace aLocalOffset)
        {
            return Anchor + TransformLocalDirection(aLocalOffset);
        }
    }
}
