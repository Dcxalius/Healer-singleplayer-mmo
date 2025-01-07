using Project_1.Camera;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Spawners
{
    internal class SpawnRectangle : SpawnGeometry
    {
        public override WorldSpace Position => new WorldSpace( topLeft + (topLeft - bottomRight) * ((float)RandomManager.RollDouble()));

        WorldSpace topLeft;
        WorldSpace bottomRight;

        public SpawnRectangle(WorldSpace aTopLeft, WorldSpace aBottomRight)
        {
            topLeft = aTopLeft;
            bottomRight = aBottomRight;
        }
    }
}
