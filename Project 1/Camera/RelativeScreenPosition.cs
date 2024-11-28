using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Camera
{
    internal struct RelativeScreenPosition
    {
        Vector2 position;

        static RelativeScreenPosition Zero { get { return new RelativeScreenPosition(); } }

        public float X { get { return position.X; } }
        public float Y { get { return position.Y; } }

        public RelativeScreenPosition()
        {
            position = Vector2.Zero;
        }

        public RelativeScreenPosition(Vector2 aPosition)
        {
            position = aPosition;
        }

        public RelativeScreenPosition(float aX)
        {
            position = new Vector2(aX);
        }

        public RelativeScreenPosition(float aX, float aY)
        {
            position = new Vector2(aX, aY);
        }



        public static RelativeScreenPosition operator +(RelativeScreenPosition aScreenPosition) => aScreenPosition;

        public static RelativeScreenPosition operator +(RelativeScreenPosition aScreenPosition, RelativeScreenPosition bScreenPosition)
        {
            return new RelativeScreenPosition(aScreenPosition.position + bScreenPosition.position);
        }

        public static RelativeScreenPosition operator -(RelativeScreenPosition aScreenPosition, RelativeScreenPosition bScreenPosition)
        {
            return new RelativeScreenPosition(aScreenPosition.position - bScreenPosition.position);
        }

        public static RelativeScreenPosition operator *(RelativeScreenPosition aScreenPosition, RelativeScreenPosition bScreenPosition)
        {
            return new RelativeScreenPosition(aScreenPosition.position * bScreenPosition.position);
        }
        public static RelativeScreenPosition operator *(RelativeScreenPosition aScreenPosition, float aMultiplier)
        { 
            return new RelativeScreenPosition(aScreenPosition.position * aMultiplier);
        }

        public static RelativeScreenPosition operator /(RelativeScreenPosition aScreenPosition, RelativeScreenPosition bScreenPosition)
        {
            return new RelativeScreenPosition(aScreenPosition.position / bScreenPosition.position);
        }
        public static RelativeScreenPosition operator /(RelativeScreenPosition aScreenPosition, float aDivisor)
        {
            return new RelativeScreenPosition(aScreenPosition.position / aDivisor);
        }

        public static bool operator ==(RelativeScreenPosition aLhs, RelativeScreenPosition aRhs)
        {
            return aLhs.position == aRhs.position;
        }

        public static bool operator !=(RelativeScreenPosition aLhs, RelativeScreenPosition aRhs)
        {
            return aLhs.position != aRhs.position;
        }

        public override int GetHashCode()
        {
            return position.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is RelativeScreenPosition)
            {
                return Equals((RelativeScreenPosition)obj);
            }
            return false;
        }

        public bool Equals(RelativeScreenPosition obj)
        {
            return (position == obj.position);
        }

        public Point ToPoint()
        {
            return position.ToPoint();
        }

        public Vector2 ToVector2()
        {
            return position;
        }
    }
}
