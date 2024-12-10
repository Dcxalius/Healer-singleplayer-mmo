using Microsoft.Xna.Framework;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Camera
{
    internal struct AbsoluteScreenPosition
    {
        Point position;
        public static AbsoluteScreenPosition Zero { get { return new AbsoluteScreenPosition(); } }

        public int X { get { return position.X; } }
        public int Y { get { return position.Y; } }

        public AbsoluteScreenPosition()
        {
            position = Point.Zero;
        }

        public AbsoluteScreenPosition(Point aPosition)
        {
            position = aPosition;
        }

        public AbsoluteScreenPosition(int aX)
        {
            position = new Point(aX);
        }

        public AbsoluteScreenPosition(int aX, int aY)
        {
            position = new Point(aX, aY);
        }



        static public AbsoluteScreenPosition FromRelativeScreenPosition(RelativeScreenPosition aRelativePosition)
        {
            AbsoluteScreenPosition pos = new AbsoluteScreenPosition((int)(Camera.ScreenRectangle.Width * aRelativePosition.X), (int)(Camera.ScreenRectangle.Height * aRelativePosition.Y));
            //DebugManager.Print(typeof(Camera), "Abs pos = " + pos + ", and relative pos = " + aPos);
            return pos;
        }

        static public RelativeScreenPosition ToRelativeScreenPos(AbsoluteScreenPosition aAbsoluteScreenPosition)
        {
            return RelativeScreenPosition.FromAbsoluteScreenPosition(aAbsoluteScreenPosition);
        }

        public RelativeScreenPosition ToRelativeScreenPosition()
        {
            return ToRelativeScreenPos(this);
        }

        public static implicit operator Point(AbsoluteScreenPosition absP) => absP.position;
        public static explicit operator AbsoluteScreenPosition(Point p) => new AbsoluteScreenPosition(p);

        public static AbsoluteScreenPosition operator +(AbsoluteScreenPosition aScreenPosition) => aScreenPosition;

        public static AbsoluteScreenPosition operator +(AbsoluteScreenPosition aScreenPosition, AbsoluteScreenPosition bScreenPosition)
        {
            return new AbsoluteScreenPosition(aScreenPosition.position + bScreenPosition.position);
        }

        public static AbsoluteScreenPosition operator -(AbsoluteScreenPosition aScreenPosition, AbsoluteScreenPosition bScreenPosition)
        {
            return new AbsoluteScreenPosition(aScreenPosition.position - bScreenPosition.position);
        }

        public static AbsoluteScreenPosition operator *(AbsoluteScreenPosition aScreenPosition, AbsoluteScreenPosition bScreenPosition)
        {
            return new AbsoluteScreenPosition(aScreenPosition.position * bScreenPosition.position);
        }
        public static AbsoluteScreenPosition operator *(AbsoluteScreenPosition aScreenPosition, float aMultiplier)
        {
            return new AbsoluteScreenPosition(new Point((int)(aScreenPosition.position.X * aMultiplier), (int)(aScreenPosition.position.Y * aMultiplier)));
        }

        public static AbsoluteScreenPosition operator /(AbsoluteScreenPosition aScreenPosition, AbsoluteScreenPosition bScreenPosition)
        {
            return new AbsoluteScreenPosition(aScreenPosition.position / bScreenPosition.position);
        }
        public static AbsoluteScreenPosition operator /(AbsoluteScreenPosition aScreenPosition, float aDivisor)
        {
            return new AbsoluteScreenPosition(new Point((int)(aScreenPosition.position.X / aDivisor), (int)(aScreenPosition.position.Y / aDivisor)));
        }

        public static bool operator ==(AbsoluteScreenPosition aLhs, AbsoluteScreenPosition aRhs)
        {
            return aLhs.position == aRhs.position;
        }

        public static bool operator !=(AbsoluteScreenPosition aLhs, AbsoluteScreenPosition aRhs)
        {
            return aLhs.position != aRhs.position;
        }

        public override int GetHashCode()
        {
            return position.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is AbsoluteScreenPosition)
            {
                return Equals((AbsoluteScreenPosition)obj);
            }
            return false;
        }

        public bool Equals(AbsoluteScreenPosition obj)
        {
            return (position == obj.position);
        }

        public Point ToPoint()
        {
            return position;
        }

        public Vector2 ToVector2()
        {
            return position.ToVector2();
        }
    }
}
