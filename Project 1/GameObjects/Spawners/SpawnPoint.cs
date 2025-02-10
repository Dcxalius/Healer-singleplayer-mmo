using Project_1.Camera;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Spawners
{
    internal class SpawnPoint : SpawnGeometry
    {
        public override WorldSpace GetNewSpawnPosition => point;

        public override WorldSpace Centre => point;

        public override float Length => 50f;

        WorldSpace point;

        public SpawnPoint(WorldSpace aPoint)
        {
            point = aPoint;
        }
    }
}
