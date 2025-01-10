using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Camera
{
    [DebuggerStepThrough]
    internal struct RelativeScreenPosition
    {
        Vector2 position;

        public static RelativeScreenPosition Zero { get { return new RelativeScreenPosition(); } }

        public float X { get { return position.X; } set => position.X = value; }
        public float Y { get { return position.Y; } set => position.Y = value; }

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

        static public RelativeScreenPosition GetSquareFromX(float aSizeInX)
        {
            float a = Camera.ScreenRectangle.Size.X * aSizeInX;
            float b = a / Camera.ScreenRectangle.Size.Y;
            return new(aSizeInX, b);
        }

        static public RelativeScreenPosition GetSquareFromY(float aSizeInY)
        {
            float a = Camera.ScreenRectangle.Size.Y * aSizeInY;
            float b = a / Camera.ScreenRectangle.Size.X;
            return new(b, aSizeInY);
        }

        static public Rectangle TransformToAbsoluteRect(RelativeScreenPosition aPos, RelativeScreenPosition aSize)
        {
            Point pos = new Point((int)(Camera.ScreenSize.X * aPos.X), (int)(Camera.ScreenSize.Y * aPos.Y));
            Point size = new Point((int)(Camera.ScreenSize.X * aSize.X), (int)(Camera.ScreenSize.Y * aSize.Y));
            return new Rectangle(pos, size);
        }

        static public RelativeScreenPosition FromAbsoluteScreenPosition(AbsoluteScreenPosition aAbsoluteScreenPosition)
        {
            RelativeScreenPosition pos = new RelativeScreenPosition(aAbsoluteScreenPosition.X / (float)Camera.ScreenRectangle.Width, aAbsoluteScreenPosition.Y / (float)Camera.ScreenRectangle.Height);
            return pos;
        }

        static public AbsoluteScreenPosition ToAbsoluteScreenPosition(RelativeScreenPosition aAbsoluteScreenPosition)
        {
            return AbsoluteScreenPosition.FromRelativeScreenPosition(aAbsoluteScreenPosition);
        }

        public AbsoluteScreenPosition ToAbsoluteScreenPos()
        {
            return ToAbsoluteScreenPosition(this);
        }

        public static implicit operator Vector2(RelativeScreenPosition rs) => rs.position;
        public static explicit operator RelativeScreenPosition(Vector2 v) => new RelativeScreenPosition(v);

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
