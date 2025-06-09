using Microsoft.Xna.Framework;
using Project_1.UI.HUD;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Camera
{
    [DebuggerStepThrough]
    internal struct AbsoluteScreenPosition
    {
        public static AbsoluteScreenPosition UpCardinal => new AbsoluteScreenPosition(0, -1);
        public static AbsoluteScreenPosition DownCardinal => new AbsoluteScreenPosition(0, 1);
        public static AbsoluteScreenPosition LeftCardinal => new AbsoluteScreenPosition(-1, 0);
        public static AbsoluteScreenPosition RightCardinal => new AbsoluteScreenPosition(0, 1);


        Point position;
        public static AbsoluteScreenPosition Zero { get { return new AbsoluteScreenPosition(); } }

        public int X
        {
            get => position.X;
            set => position.X = value;
        }

        public int Y
        {
            get => position.Y;
            set => position.Y = value;
        }

        public AbsoluteScreenPosition OnlyX => new AbsoluteScreenPosition(position.X, 0);
        public AbsoluteScreenPosition OnlyY => new AbsoluteScreenPosition(0, position.Y);

        public AbsoluteScreenPosition(int aX) : this(new Point(aX)) { }
        public AbsoluteScreenPosition(int aX, int aY) : this(new Point(aX, aY)) { }
        public AbsoluteScreenPosition(Point aPosition)
        {
            position = aPosition;
        }



        static public AbsoluteScreenPosition FromRelativeScreenPosition(RelativeScreenPosition aRelativePosition, AbsoluteScreenPosition? aContext = null)
        {
            if (!aContext.HasValue) return new AbsoluteScreenPosition((int)(Camera.ScreenRectangle.Width * aRelativePosition.X), (int)(Camera.ScreenRectangle.Height * aRelativePosition.Y));
            return new AbsoluteScreenPosition((int)(aContext.Value.X * aRelativePosition.X), (int)(aContext.Value.Y * aRelativePosition.Y));

            //DebugManager.Print(typeof(Camera), "Abs pos = " + pos + ", and relative pos = " + aPos);
        }

        static public RelativeScreenPosition ToRelativeScreenPos(AbsoluteScreenPosition aAbsoluteScreenPosition)
        {
            return RelativeScreenPosition.FromAbsoluteScreenPosition(aAbsoluteScreenPosition);
        }

        static public RelativeScreenPosition ToRelativeScreenPos(AbsoluteScreenPosition aAbsoluteScreenPosition, AbsoluteScreenPosition aContext)
        {
            return RelativeScreenPosition.FromAbsoluteScreenPosition(aAbsoluteScreenPosition, aContext);
        }

        public RelativeScreenPosition ToRelativeScreenPosition()
        {
            return ToRelativeScreenPos(this);
        }

        public RelativeScreenPosition ToRelativeScreenPosition(AbsoluteScreenPosition aContext)
        {
            return ToRelativeScreenPos(this, aContext);
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

        public override string ToString()
        {
            return position.ToString();
        }
    }
}
