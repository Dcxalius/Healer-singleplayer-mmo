using Newtonsoft.Json;
using Project_1.Camera;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Spawners.Pathing
{
    internal abstract class MobPathing
    {
        public enum PathingType
        {
            Wander,
            Bound,
            Patrol
        }

        TimeSpan timeSinceMovedLast;
        double waitDuration;
        [JsonIgnore]
        public abstract WorldSpace? GetNextSpace { get; }
        [JsonIgnore]
        public abstract WorldSpace GetLatestSpace { get; }

        public abstract PathingType UnderlyingType { get; }

        protected void StartTimer()
        {
            if (timeSinceMovedLast != TimeSpan.Zero) return;
            timeSinceMovedLast = TimeManager.TotalFrameTimeAsTimeSpan;
            waitDuration = RandomManager.RollDouble() * 5;

        }

        protected bool TimeForMove()
        {
            if (timeSinceMovedLast + TimeSpan.FromSeconds(waitDuration) > TimeManager.TotalFrameTimeAsTimeSpan) return false;

            timeSinceMovedLast = TimeSpan.Zero;

            return true;
        }

        public virtual void Reset()
        {
            timeSinceMovedLast = TimeSpan.Zero;
            waitDuration = 0;
        }

        public abstract WorldSpace NewSpawn(WorldSpace aSize); //TODO: Rename
    }
}
