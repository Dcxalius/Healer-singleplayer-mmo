using Microsoft.Xna.Framework.Media;
using Newtonsoft.Json;
using Project_1.Camera;
using Project_1.Managers;
using Project_1.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Spawners.Pathing
{
    internal class Bound : MobPathing
    {
        public override PathingType UnderlyingType => PathingType.Bound;

        [JsonIgnore]
        public override WorldSpace? GetNextSpace
        {
            get
            {
                StartTimer();
                if (!TimeForMove()) return null;

                return NewSpawn(GetLatestSpace);
            }
        }

        [JsonIgnore]
        public override WorldSpace GetLatestSpace => lastDirection;

        WorldSpace lastDirection;

        public WorldSpace BindPoint => bindPoint;
        WorldSpace bindPoint;
        [JsonProperty("Leash")]
        float leash;

        [JsonConstructor]
        public Bound(WorldSpace bindPoint, float leash)
        {
            this.bindPoint = bindPoint;
            this.leash = leash;
        }

        public override WorldSpace NewSpawn(WorldSpace aSize)
        {
            Reset();
            float radians = (float)(RandomManager.RollDouble() * Math.PI * 2);
            WorldSpace dirVector = new WorldSpace(MathF.Sin(radians), -MathF.Cos(radians));
            WorldSpace newSpawn = bindPoint + dirVector * ((float)RandomManager.RollDouble() * leash);
            newSpawn = TileManager.FindClosestWalkableWorldSpace(newSpawn, aSize);
            return newSpawn;
        }
    }
}
