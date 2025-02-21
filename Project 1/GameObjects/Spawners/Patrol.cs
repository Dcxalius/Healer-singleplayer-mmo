using Project_1.Camera;
using Project_1.Managers;
using Project_1.Tiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Spawners
{
    internal class Patrol : MobPathing
    {
        public override PathingType UnderlyingType => PathingType.Patrol;

        public enum PatrolType
        {
            Circular,
            Bounce
        }

        WorldSpace[] queue;
        int nextIndex;
        int CurrentIndex
        {
            get
            {
                if (type == PatrolType.Circular || hasBounced == false)
                {
                    return (queue.Length + nextIndex - 1) % queue.Length;
                }
                return nextIndex + 1;
            }
        }
        PatrolType type;
        bool hasBounced;

        public override WorldSpace? GetNextSpace => GetNextInQueue();

        public override WorldSpace GetLatestSpace => queue[CurrentIndex];


        public Patrol(WorldSpace[] aQueue, PatrolType aType, WorldSpace aUnitSize)
        {
            for (int i = 0; i < aQueue.Length; i++)
            {
                Tile t = TileManager.GetTileUnder(aQueue[i]);
                if (t.Walkable) continue;

                TileManager.FindClosestWalkableWorldSpace(aQueue[i], aUnitSize);
            }
            type = aType;
            nextIndex = 1; //Make it take the closest one as first index??
            queue = aQueue;
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
                    nextWorldSpaceInQueue = queue[nextIndex];
                    nextIndex++;
                    nextIndex %= queue.Length;
                    return nextWorldSpaceInQueue;
                case PatrolType.Bounce:
                    nextWorldSpaceInQueue = queue[nextIndex];
                    if (nextIndex == queue.Length - 1) hasBounced = true;
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
            int currentIndex = RandomManager.RollInt(queue.Length);
            switch (type)
            {
                case PatrolType.Circular:
                    nextIndex = (currentIndex + 1) % queue.Length;
                    break;
                case PatrolType.Bounce:
                    if (currentIndex == 0)
                    {
                        hasBounced = false;
                        nextIndex = 1;
                    }
                    else if (currentIndex == queue.Length - 1)
                    {
                        hasBounced = true;
                        nextIndex = queue.Length - 2;
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
            WorldSpace newSpawn = TileManager.FindClosestWalkableWorldSpace(queue[currentIndex], aSize);
            return newSpawn;
        }
    }
}
