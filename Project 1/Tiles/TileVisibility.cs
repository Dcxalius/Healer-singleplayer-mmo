using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects.Entities;
using System;

namespace Project_1.Tiles
{
    internal static partial class TileManager
    {
        public static bool CheckLineOfSight(Entity aCaster, WorldSpace aTarget)
        {
            WorldSpace start = aCaster.FeetPosition;
            if (start == aTarget) return true;

            Tile startTile = GetTile(start);
            Tile targetTile = GetTile(aTarget);
            if (!startTile.Transparent || !targetTile.Transparent) return false;

            return LineOfSight(start, aTarget, lastTile => !lastTile.Transparent, lastTile => lastTile == targetTile);
        }

        static bool LineOfSight(WorldSpace aStartPos, WorldSpace aEndPos, Func<Tile, bool> aFalseCondition, Func<Tile, bool> aTrueCondition)
        {
            Vector2 start = aStartPos;
            Vector2 end = aEndPos;
            float x = start.X / Tile.Size.X;
            float y = start.Y / Tile.Size.Y;
            float endX = end.X / Tile.Size.X;
            float endY = end.Y / Tile.Size.Y;

            Point startGrid = GetGridPos(aStartPos);
            Point targetGrid = GetGridPos(aEndPos);
            int tileX = startGrid.X;
            int tileY = startGrid.Y;
            int targetX = targetGrid.X;
            int targetY = targetGrid.Y;

            if (tileX == targetX && tileY == targetY) return true;

            float dx = endX - x;
            float dy = endY - y;
            int stepX = Math.Sign(dx);
            int stepY = Math.Sign(dy);

            float tDeltaX = stepX == 0 ? float.PositiveInfinity : MathF.Abs(1f / dx);
            float tDeltaY = stepY == 0 ? float.PositiveInfinity : MathF.Abs(1f / dy);

            float nextBoundaryX = stepX > 0 ? MathF.Floor(x) + 1 : MathF.Floor(x);
            float nextBoundaryY = stepY > 0 ? MathF.Floor(y) + 1 : MathF.Floor(y);
            float tMaxX = stepX == 0 ? float.PositiveInfinity : MathF.Abs((nextBoundaryX - x) / dx);
            float tMaxY = stepY == 0 ? float.PositiveInfinity : MathF.Abs((nextBoundaryY - y) / dy);

            const float epsilon = 0.00001f;
            while (tileX != targetX || tileY != targetY)
            {
                if (MathF.Abs(tMaxX - tMaxY) < epsilon)
                {
                    tileX += stepX;
                    tileY += stepY;
                    tMaxX += tDeltaX;
                    tMaxY += tDeltaY;
                }
                else if (tMaxX < tMaxY)
                {
                    tileX += stepX;
                    tMaxX += tDeltaX;
                }
                else
                {
                    tileY += stepY;
                    tMaxY += tDeltaY;
                }

                Tile tile = GetTileAtGrid(new Point(tileX, tileY));
                if (tile == null) return false;
                if (aFalseCondition(tile)) return false;
                if (aTrueCondition(tile)) return true;
            }

            return true;
        }
    }
}
