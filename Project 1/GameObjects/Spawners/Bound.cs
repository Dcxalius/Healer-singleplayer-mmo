using Project_1.Camera;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Spawners
{
    internal class Bound : MobPathing
    {
        public override WorldSpace GetNextSpace
        {
            get
            {
                WorldSpace space = WorldSpace.Zero;
                space.X = (float)(RandomManager.RollDouble() * 2 * leash - leash);
                space.Y = (float)(RandomManager.RollDouble() * 2 * leash - leash);
                return space;
            }
        }

        public override WorldSpace GetLatestSpace => lastDirection;

        WorldSpace lastDirection;
        WorldSpace bindPoint;
        float leash;

        public Bound(WorldSpace aBindPoint , float aMaxDistanceOfLeash)
        {
            bindPoint = aBindPoint;
            leash = aMaxDistanceOfLeash;
        }

        public override WorldSpace? Update(WorldSpace aPosition)
        {

            WorldSpace? r = UpdateTimer(aPosition);
            if (!r.HasValue) return null;
            lastDirection = r.Value;
            return lastDirection;
        }

        public override void Reset(WorldSpace aSpawn)
        {
            base.Reset(aSpawn);

            lastDirection = bindPoint;
        }
    }
}
