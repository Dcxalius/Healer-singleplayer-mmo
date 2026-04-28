using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Diagnostics;

namespace Project_1.Camera
{
    [DebuggerStepThrough]
    internal struct WorldSpace3D
    {
        [JsonProperty]
        Vector3 position;

        [JsonIgnore]
        public static WorldSpace3D Zero => new WorldSpace3D();

        [JsonIgnore]
        public WorldSpace3D OnlyX => new WorldSpace3D(position.X, 0, 0);
        [JsonIgnore]
        public WorldSpace3D OnlyY => new WorldSpace3D(0, position.Y, 0);
        [JsonIgnore]
        public WorldSpace3D OnlyZ => new WorldSpace3D(0, 0, position.Z);
        [JsonIgnore]
        public float X { get => position.X; set => position.X = value; }
        [JsonIgnore]
        public float Y { get => position.Y; set => position.Y = value; }
        [JsonIgnore]
        public float Z { get => position.Z; set => position.Z = value; }

        public WorldSpace3D()
        {
            position = Vector3.Zero;
        }

        [JsonConstructor]
        public WorldSpace3D(Vector3 aPosition)
        {
            position = aPosition;
        }

        public WorldSpace3D(float aValue) : this(aValue, aValue, aValue)
        {
        }

        public WorldSpace3D(float aX, float aY, float aZ) : this(new Vector3(aX, aY, aZ))
        {
        }

        public float DistanceTo(WorldSpace3D aOtherSpace) => (this - aOtherSpace).ToVector3().Length();

        public static implicit operator Vector3(WorldSpace3D ws) => ws.position;
        public static explicit operator WorldSpace3D(Vector3 v) => new WorldSpace3D(v);

        public static WorldSpace3D operator +(WorldSpace3D aWorldPosition) => aWorldPosition;
        public static WorldSpace3D operator +(WorldSpace3D aWorldPosition, WorldSpace3D bWorldPosition) => new WorldSpace3D(aWorldPosition.position + bWorldPosition.position);
        public static WorldSpace3D operator -(WorldSpace3D aWorldPosition, WorldSpace3D bWorldPosition) => new WorldSpace3D(aWorldPosition.position - bWorldPosition.position);

        public static WorldSpace3D operator *(WorldSpace3D aWorldPosition, WorldSpace3D bWorldPosition) => new WorldSpace3D(aWorldPosition.position * bWorldPosition.position);
        public static WorldSpace3D operator *(WorldSpace3D aWorldPosition, float aMultiplier) => new WorldSpace3D(aWorldPosition.position * aMultiplier);
        public static WorldSpace3D operator *(float aMultiplier, WorldSpace3D aWorldPosition) => new WorldSpace3D(aWorldPosition.position * aMultiplier);

        public static WorldSpace3D operator /(WorldSpace3D aWorldPosition, WorldSpace3D bWorldPosition) => new WorldSpace3D(aWorldPosition.position / bWorldPosition.position);
        public static WorldSpace3D operator /(WorldSpace3D aWorldPosition, float aDivisor) => new WorldSpace3D(aWorldPosition.position / aDivisor);

        public static bool operator ==(WorldSpace3D aLhs, WorldSpace3D aRhs) => aLhs.position == aRhs.position;
        public static bool operator !=(WorldSpace3D aLhs, WorldSpace3D aRhs) => aLhs.position != aRhs.position;

        public override int GetHashCode() => position.GetHashCode();

        public override bool Equals(object obj)
        {
            if (obj is WorldSpace3D)
            {
                return Equals((WorldSpace3D)obj);
            }
            return false;
        }

        public bool Equals(WorldSpace3D obj) => position == obj.position;

        public void Normalize() => position.Normalize();

        public static WorldSpace3D Normalize(WorldSpace3D ws) => new WorldSpace3D(Vector3.Normalize(ws.position));

        public Vector3 ToVector3() => position;

        public override string ToString() => position.ToString();
    }
}
