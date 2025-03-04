using Newtonsoft.Json;
using Project_1.Camera;
using Project_1.Managers;
using Project_1.Tiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Spawners.Pathing
{
    internal class Patrol : MobPathing
    {
        public override PathingType UnderlyingType => PathingType.Patrol;

        public enum PatrolType
        {
            Circular,
            Bounce
        }

        [JsonProperty("Route")]
        WorldSpace[] route;
        int nextIndex;
        [JsonIgnore]
        int CurrentIndex
        {
            get
            {
                if (type == PatrolType.Circular || hasBounced == false)
                {
                    return (route.Length + nextIndex - 1) % route.Length;
                }
                return nextIndex + 1;
            }
        }
        [JsonProperty("PatrolType")]
        PatrolType type;
        [JsonProperty("HasBounced")]
        bool hasBounced;

        [JsonIgnore]
        public override WorldSpace? GetNextSpace => GetNextInQueue();

        [JsonIgnore]
        public override WorldSpace GetLatestSpace => route[CurrentIndex];

        [JsonConstructor]
        Patrol(WorldSpace[] route, int nextIndex, PatrolType type, bool hasBounced)
        {
            
            this.route = route;
            this.nextIndex = 1;
            this.type = type;
            this.hasBounced = hasBounced;
        }

        public Patrol(WorldSpace[] aQueue, PatrolType aType, WorldSpace aUnitSize)
        {
            for (int i = 0; i < aQueue.Length; i++)
            {
                Tile t = TileManager.GetTileUnder(aQueue[i]);
                if (t.Walkable) continue;

                aQueue[i] = TileManager.FindClosestWalkableWorldSpace(aQueue[i], aUnitSize);
            }
            type = aType;
            nextIndex = 1; //Make it take the closest one as first index??
            route = aQueue;
            hasBounced = false;
        }

        public WorldSpace? GetNextInQueue()
        {
            StartTimer();
            if (!TimeForMove()) return null;
            WorldSpace nextWorldSpaceInQueue;
            switch (type)
            {
                case PatrolType.Circular:
                    nextWorldSpaceInQueue = route[nextIndex];
                    nextIndex++;
                    nextIndex %= route.Length;
                    return nextWorldSpaceInQueue;
                case PatrolType.Bounce:
                    nextWorldSpaceInQueue = route[nextIndex];
                    if (nextIndex == route.Length - 1) hasBounced = true;
                    if (nextIndex == 0) hasBounced = false;
                    if (!hasBounced) nextIndex++;
                    else nextIndex--;
                    return nextWorldSpaceInQueue;
                default:
                    throw new NotImplementedException();
            }
        }

        public override WorldSpace NewSpawn(WorldSpace aSize)
        {
            Reset();
            int currentIndex = RandomManager.RollInt(route.Length);
            switch (type)
            {
                case PatrolType.Circular:
                    nextIndex = (currentIndex + 1) % route.Length;
                    break;
                case PatrolType.Bounce:
                    if (currentIndex == 0)
                    {
                        hasBounced = false;
                        nextIndex = 1;
                    }
                    else if (currentIndex == route.Length - 1)
                    {
                        hasBounced = true;
                        nextIndex = route.Length - 2;
                    }
                    else
                    {
                        hasBounced = RandomManager.RollDouble() > 0.5d;
                        if (hasBounced == false)
                        {
                            nextIndex = currentIndex + 1;
                        }
                        else
                        {
                            nextIndex = currentIndex - 1;
                        }
                    }

                    break;
                default:
                    break;
            }
            WorldSpace newSpawn = TileManager.FindClosestWalkableWorldSpace(route[currentIndex], aSize);
            return newSpawn;
        }
    }
}
