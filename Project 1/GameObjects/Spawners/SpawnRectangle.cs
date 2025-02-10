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
        public override WorldSpace GetNewSpawnPosition => new WorldSpace( topLeft + (bottomRight - topLeft) * ((float)RandomManager.RollDouble())); //TODO: This is wrong //i think xdd
        public override WorldSpace Centre => new WorldSpace(topLeft + bottomRight / 2);
        public override float Length => MathF.Min(Math.Abs(topLeft.X - bottomRight.X), Math.Abs(topLeft.Y - bottomRight.Y));

        WorldSpace topLeft;
        WorldSpace bottomRight;

        public SpawnRectangle(WorldSpace aTopLeft, WorldSpace aBottomRight)
        {
            topLeft = aTopLeft;
            bottomRight = aBottomRight;
        }
    }
}
