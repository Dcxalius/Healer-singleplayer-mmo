using Project_1.Camera;
using Project_1.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Spawners
{
    internal class Patrol : MobPathing
    {
        public enum Type
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
                if (type == Type.Circular || hasBounced == false)
                {
                    return (queue.Length + nextIndex - 1) % queue.Length;
                }
                return nextIndex + 1;
            }
        }
        Type type;
        bool hasBounced;

        public override WorldSpace? GetNextSpace => GetNextInQueue();

        public override WorldSpace GetLatestSpace => queue[CurrentIndex];

        public Patrol(WorldSpace[] aQueue, Type aType, WorldSpace aUnitSize)
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
            Reset();
            switch (type)
            {
                case Type.Circular:
                    nextWorldSpaceInQueue = queue[nextIndex];
                    nextIndex++;
                    nextIndex %= queue.Length;
                    return nextWorldSpaceInQueue;
                case Type.Bounce:
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
    }
}
