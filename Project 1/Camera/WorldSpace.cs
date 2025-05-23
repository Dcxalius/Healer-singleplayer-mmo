﻿using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using SharpDX.Direct2D1.Effects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Camera
{
    [DebuggerStepThrough]
    internal struct WorldSpace
    {
        [JsonProperty]
        Vector2 position;

        [JsonIgnore]
        public static WorldSpace Zero { get { return new WorldSpace(); } }

        [JsonIgnore]
        public WorldSpace OnlyX => new WorldSpace(position.X, 0);
        [JsonIgnore]
        public WorldSpace OnlyY => new WorldSpace(0, position.Y);
        [JsonIgnore]
        public float X { get { return position.X; }  set { position.X = value; } }
        [JsonIgnore]
        public float Y { get { return position.Y; } set { position.Y = value; } }

        public WorldSpace()
        {
            position = Vector2.Zero;
        }

        public WorldSpace(Point aPoint) : this(aPoint.ToVector2()) { }

        [JsonConstructor]
        public WorldSpace(Vector2 aPosition)
        {
            position = aPosition;
        }

        public WorldSpace(float aX) : this(aX, aX) { }
        public WorldSpace(float aX, float aY) : this(new Vector2(aX, aY)) { }

        public float DistanceTo(WorldSpace aOtherSpace)
        {
            return (this - aOtherSpace).ToVector2().Length();
        }

        public AbsoluteScreenPosition ToAbsoltueScreenPosition()
        {
            WorldSpace topLeft = Camera.CentreInWorldSpace * Camera.Scale - new WorldSpace(Camera.WindowSize.ToVector2() / 2);

            //return new AbsoluteScreenPosition((int)Math.Round(this.X * Camera.Scale - topLeft.X, MidpointRounding.ToPositiveInfinity), (int)Math.Round(this.Y * Camera.Scale - topLeft.Y, MidpointRounding.ToPositiveInfinity));
            return new AbsoluteScreenPosition((int)Math.Floor(this.X * Camera.Scale - topLeft.X), (int)Math.Floor(this.Y * Camera.Scale - topLeft.Y));
        }
        public static WorldSpace FromRelativeScreenSpace(RelativeScreenPosition aScreenSpace)
        {
            return FromAbsoluteScreenSpace(aScreenSpace.ToAbsoluteScreenPos());
        }

        public static WorldSpace FromAbsoluteScreenSpace(AbsoluteScreenPosition aScreenSpace)
        {

            WorldSpace vectorInScreen = (WorldSpace)(Camera.CentrePointInScreenSpace - aScreenSpace).ToVector2();

            WorldSpace vectorInWorld = (WorldSpace)(Camera.CentreInWorldSpace - vectorInScreen * Camera.Zoom);

            return vectorInWorld;
        }

        public static implicit operator Vector2(WorldSpace ws) => ws.position; 
        public static explicit operator WorldSpace(Vector2 v) => new WorldSpace(v);

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

        public static WorldSpace operator *(float aMultiplier, WorldSpace aScreenPosition)
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

        public void Normalize()
        {
            position.Normalize();
        }

        static public WorldSpace Normalize(WorldSpace ws)
        {
            return new WorldSpace(Vector2.Normalize(ws.position));
        }

        public Point ToPoint()
        {
            return position.ToPoint();
        }

        public Vector2 ToVector2()
        {
            return position;
        }

        public override string ToString()
        {
            return position.ToString();
        }
    }
}
