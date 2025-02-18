using Project_1.Camera;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Spawners
{
    internal class Bound : MobPathing
    {
        public enum Type
        {
            Square,
            Circle
        }
        public override WorldSpace? GetNextSpace
        {
            get
            {
                StartTimer();
                if (!TimeForMove()) return null;
                Reset();
                return GetNext();
            }
        }

        public override WorldSpace GetLatestSpace => lastDirection;

        WorldSpace lastDirection;
        WorldSpace bindPoint;
        float leash;

        Type type;

        public Bound(WorldSpace aBindPoint , float aMaxDistanceOfLeash, Type aType)
        {
            bindPoint = aBindPoint;
            leash = aMaxDistanceOfLeash;
            type = aType;
        }

        WorldSpace GetNext()
        {
            switch (type)
            {
                case Type.Square:
                    return GetNextSquare();
                case Type.Circle:
                    return GetNextCircle();
                default:
                    throw new NotImplementedException();
            }
        }

        WorldSpace GetNextSquare()
        {

            WorldSpace space = WorldSpace.Zero;
            space.X = (float)(RandomManager.RollDouble() * 2 * leash - leash);
            space.Y = (float)(RandomManager.RollDouble() * 2 * leash - leash);
            return space;
        }

        WorldSpace GetNextCircle()
        {
            WorldSpace randomDir = new WorldSpace((float)RandomManager.RollDouble() * 2 - 1, (float)RandomManager.RollDouble() * 2 - 1);
            float randomDistance = (float)(leash * RandomManager.RollDouble());
            return randomDir * randomDistance;
        }
    }
}
