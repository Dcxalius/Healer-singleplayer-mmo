using Project_1.Camera;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Spawners
{
    internal abstract class MobPathing
    {

        TimeSpan timeSinceMovedLast;
        double waitDuration;
        public abstract WorldSpace GetNextSpace { get; }
        public abstract WorldSpace GetLatestSpace { get; }

        public abstract WorldSpace? Update(WorldSpace aPosition);


        protected void StartTimer()
        {
            if (timeSinceMovedLast != TimeSpan.Zero) return;
            timeSinceMovedLast = TimeManager.TotalFrameTimeAsTimeSpan;

        }

        protected bool TimeForMove()
        {
            if (timeSinceMovedLast + TimeSpan.FromSeconds(waitDuration) > TimeManager.TotalFrameTimeAsTimeSpan) return false;

            timeSinceMovedLast = TimeSpan.Zero;
            waitDuration = RandomManager.RollDouble()* 5000;

            return true;
        }

        protected WorldSpace? UpdateTimer(WorldSpace aSpace) //TODO: Better name pls
        {
            if (GetLatestSpace.DistanceTo(aSpace) < 1f) return null;
            StartTimer();
            if (!TimeForMove()) return null;

            return GetNextSpace;
        }

        public virtual void Reset(WorldSpace aSpawn)
        {
            timeSinceMovedLast = TimeSpan.Zero;
            waitDuration = 0;
        }
    }
}
