using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Entities
{
    internal class AggroEntity
    {
        public Entity Entity { get => entity; set => entity = value; }
        Entity entity;
        public float Threat { get => threat; set => threat = value; }
        float threat;
        public TimeSpan TimeSinceLastHit { get => timeSinceLastHit; set => timeSinceLastHit = value; }
        TimeSpan timeSinceLastHit;
        public TimeSpan TimeFirstHitMe { get => timeFirstHitMe; set => timeFirstHitMe = value; }
        TimeSpan timeFirstHitMe;

        public AggroEntity(Entity aEntityToAdd, float aThreatValue, TimeSpan aCurrentTime)
        {
            entity = aEntityToAdd;
            threat = aThreatValue;
            timeFirstHitMe = aCurrentTime;
            timeSinceLastHit = aCurrentTime;
        }
    }
}
