using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Tiles
{
    internal class Path
    {
        public WorldSpace ComsumeNextPoint
        {
            get
            {
                WorldSpace p = pointsOnPath.First();
                pointsOnPath.RemoveAt(0);
                return p;
            }
        }

        public int Count => pointsOnPath.Count;

        List<WorldSpace> pointsOnPath;
        
        public Path(List<Tile> aPath, WorldSpace aEndPosition) //TODO: Include offset
        {
            pointsOnPath = new List<WorldSpace>();
            //pointsOnPath.Add(aPath[aPath.Count - 1].Centre);
            Vector2 lastDir = WorldSpace.Zero;
            for (int i = aPath.Count - 2; i >= 1; i--)
            {
                Vector2 dir = Vector2.Normalize((aPath[i].GridPos - aPath[i - 1].GridPos).ToVector2());
                if (lastDir == dir) continue;

                lastDir = dir;
                pointsOnPath.Add(aPath[i].Centre);
                DebugManager.AddDebugShape(new DebugTools.DebugPoint(pointsOnPath.Last(), 10f));
            }

            pointsOnPath.Add(aEndPosition);
            DebugManager.AddDebugShape(new DebugTools.DebugPoint(pointsOnPath.Last(), 10f));
        }
    }
}
