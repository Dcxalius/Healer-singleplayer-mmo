using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Project_1.Tiles
{
    internal static class TileQuery
    {
        [ThreadStatic] static Tile[,] surroundingTilesCache;

        public static Tile[,] GetSurroundingTiles(Tile aTile)
        {
            const int sizeOfSquareToCheck = 3; // this should always be odd
            Debug.Assert(sizeOfSquareToCheck % 2 == 1);
            Tile[,] a = surroundingTilesCache;
            if (a == null || a.GetLength(0) != sizeOfSquareToCheck || a.GetLength(1) != sizeOfSquareToCheck)
            {
                a = new Tile[sizeOfSquareToCheck, sizeOfSquareToCheck];
                surroundingTilesCache = a;
            }
            Point centreGrid = TileManager.GetGridPos(aTile.Centre);
            for (int i = 0; i < sizeOfSquareToCheck; i++)
            {
                for (int j = 0; j < sizeOfSquareToCheck; j++)
                {
                    int x = centreGrid.X - sizeOfSquareToCheck / 2 + i;
                    int y = centreGrid.Y - sizeOfSquareToCheck / 2 + j;
                    a[i, j] = TileManager.GetTileAtGrid(new Point(x, y));
                }
            }
            return a;
        }

        public static Tile[] GetTilesAroundPosition(WorldSpace aPosition, float aDistance)
        {
            List<Tile> returnable = new List<Tile>();
            Tile midTile = TileManager.GetTile(aPosition);
            float distanceInTiles = aDistance / Tile.Size.X;
            int tilesAround = (int)(distanceInTiles * Math.PI) * 2;
            Point midGrid = TileManager.GetGridPos(midTile.Centre);

            for (int i = 0; i < tilesAround; i++)
            {
                int x = (int)Math.Round(midGrid.X + distanceInTiles * Math.Sin(i * 2 * Math.PI / tilesAround));
                int y = (int)Math.Floor(midGrid.Y + distanceInTiles * Math.Cos(i * 2 * Math.PI / tilesAround));
                Tile t = TileManager.GetTileAtGrid(new Point(x, y));

                if (t == null)
                {
                    DebugManager.Print($"Tried to get tile at grid {x},{y} but it returned null.");
                    continue;
                }

                if (!t.Walkable) continue;
                returnable.Add(t);
            }

            return returnable.ToArray();
        }
    }
}
