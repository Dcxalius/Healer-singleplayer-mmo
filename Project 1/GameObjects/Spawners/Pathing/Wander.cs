using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Project_1.Camera;
using Project_1.Managers;
using Project_1.Tiles;
using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Spawners.Pathing
{
    internal class Wander : MobPathing
    {
        public override PathingType UnderlyingType => PathingType.Wander;

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

        [JsonProperty("Boundry")]
        Rectangle boundry;


        [JsonConstructor]
        public Wander(Rectangle boundry)
        {
            this.boundry = boundry;
        }

        public override WorldSpace NewSpawn(WorldSpace aSize)
        {
            Reset();
            WorldSpace newSpace = new WorldSpace();
            newSpace.X = (float)(boundry.Location.X + RandomManager.RollDouble() * boundry.Width);
            newSpace.Y = (float)(boundry.Location.Y + RandomManager.RollDouble() * boundry.Height);
            newSpace = TileManager.FindClosestWalkableWorldSpace(newSpace, aSize);
            return newSpace;
        }
    }
}
