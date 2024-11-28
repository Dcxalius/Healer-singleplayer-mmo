using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Camera
{
    internal struct WorldSpace
    {
        Vector2 position;

        static WorldSpace Zero { get { return new WorldSpace(); } }

        public float X { get { return position.X; } }
        public float Y { get { return position.Y; } }

        public WorldSpace()
        {
            position = Vector2.Zero;
        }

        public WorldSpace(Vector2 aPosition)
        {
            position = aPosition;
        }

        public WorldSpace(float aX)
        {
            position = new Vector2(aX);
        }

        public WorldSpace(float aX, float aY)
        {
            position = new Vector2(aX, aY);
        }



        public static WorldSpace operator +(WorldSpace aScreenPosition) => aScreenPosition;

        public static WorldSpace operator +(WorldSpace aScreenPosition, WorldSpace bScreenPosition)
        {
            return new WorldSpace(aScreenPosition.position + bScreenPosition.position);
        }

        public static WorldSpace operator -(WorldSpace aScreenPosition, WorldSpace bScreenPosition)
        {
            return new WorldSpace(aScreenPosition.position - bScreenPosition.position);
        }

        public static WorldSpace operator *(WorldSpace aScreenPosition, WorldSpace bScreenPosition)
        {
            return new WorldSpace(aScreenPosition.position * bScreenPosition.position);
        }
        public static WorldSpace operator *(WorldSpace aScreenPosition, float aMultiplier)
        {
            return new WorldSpace(aScreenPosition.position * aMultiplier);
        }

        public static WorldSpace operator /(WorldSpace aScreenPosition, WorldSpace bScreenPosition)
        {
            return new WorldSpace(aScreenPosition.position / bScreenPosition.position);
        }
        public static WorldSpace operator /(WorldSpace aScreenPosition, float aDivisor)
        {
            return new WorldSpace(aScreenPosition.position / aDivisor);
        }

        public static bool operator ==(WorldSpace aLhs, WorldSpace aRhs)
        {
            return aLhs.position == aRhs.position;
        }

        public static bool operator !=(WorldSpace aLhs, WorldSpace aRhs)
        {
            return aLhs.position != aRhs.position;
        }

        public override int GetHashCode()
        {
            return position.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is WorldSpace)
            {
                return Equals((WorldSpace)obj);
            }
            return false;
        }

        public bool Equals(WorldSpace obj)
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
