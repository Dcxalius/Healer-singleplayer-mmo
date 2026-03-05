using Microsoft.Xna.Framework;
using Project_1.Managers;
using System;

namespace Project_1.Tiles
{
    /// <summary>
    /// CPU field-of-view helper using recursive shadowcasting over 8 octants.
    /// Sim-thread only.
    /// </summary>
    internal static class RecursiveShadowcaster
    {
        delegate void VisibilityWriter(Point tile, int distanceSquared);

        public static void ComputeVisibleTiles(Point originTile, int radius, Action<Point, int> onVisibleTile)
        {
            ThreadAffinity.AssertSimThread();
            if (onVisibleTile == null) return;
            if (radius < 0) return;

            ComputeVisibleTiles(originTile, radius, IsOpaqueByTileTransparency, onVisibleTile);
        }

        public static void ComputeVisibleTiles(
            Point originTile,
            int radius,
            Func<Point, bool> isOpaque,
            Action<Point, int> onVisibleTile)
        {
            ThreadAffinity.AssertSimThread();
            if (onVisibleTile == null) return;
            if (isOpaque == null) return;
            if (radius < 0) return;

            VisibilityWriter writer = (tile, distanceSquared) =>
            {
                onVisibleTile(tile, distanceSquared);
            };

            int radiusSquared = radius * radius;
            writer(originTile, 0);

            // 8 octants around the source.
            CastLight(originTile, 1, 1f, 0f, radius, radiusSquared, 1, 0, 0, 1, isOpaque, writer);
            CastLight(originTile, 1, 1f, 0f, radius, radiusSquared, 0, 1, 1, 0, isOpaque, writer);
            CastLight(originTile, 1, 1f, 0f, radius, radiusSquared, 0, -1, 1, 0, isOpaque, writer);
            CastLight(originTile, 1, 1f, 0f, radius, radiusSquared, -1, 0, 0, 1, isOpaque, writer);
            CastLight(originTile, 1, 1f, 0f, radius, radiusSquared, -1, 0, 0, -1, isOpaque, writer);
            CastLight(originTile, 1, 1f, 0f, radius, radiusSquared, 0, -1, -1, 0, isOpaque, writer);
            CastLight(originTile, 1, 1f, 0f, radius, radiusSquared, 0, 1, -1, 0, isOpaque, writer);
            CastLight(originTile, 1, 1f, 0f, radius, radiusSquared, 1, 0, 0, -1, isOpaque, writer);
        }

        static bool IsOpaqueByTileTransparency(Point tilePos)
        {
            Tile tile = TileManager.GetTileAtGrid(tilePos);
            if (tile == null) return true;
            return !tile.Transparent;
        }

        static void CastLight(
            Point origin,
            int row,
            float startSlope,
            float endSlope,
            int radius,
            int radiusSquared,
            int xx,
            int xy,
            int yx,
            int yy,
            Func<Point, bool> isOpaque,
            VisibilityWriter onVisible)
        {
            if (startSlope < endSlope) return;

            float currentStartSlope = startSlope;
            for (int distance = row; distance <= radius; distance++)
            {
                bool blocked = false;
                float nextStartSlope = currentStartSlope;

                for (int dx = -distance, dy = -distance; dx <= 0; dx++)
                {
                    float leftSlope = (dx - 0.5f) / (dy + 0.5f);
                    float rightSlope = (dx + 0.5f) / (dy - 0.5f);

                    if (currentStartSlope < rightSlope) continue;
                    if (endSlope > leftSlope) break;

                    int tx = origin.X + dx * xx + dy * xy;
                    int ty = origin.Y + dx * yx + dy * yy;
                    Point tile = new Point(tx, ty);

                    int distanceSquared = dx * dx + dy * dy;
                    if (distanceSquared <= radiusSquared)
                    {
                        onVisible(tile, distanceSquared);
                    }

                    bool opaque = isOpaque(tile);
                    if (blocked)
                    {
                        if (opaque)
                        {
                            nextStartSlope = rightSlope;
                            continue;
                        }

                        blocked = false;
                        currentStartSlope = nextStartSlope;
                        continue;
                    }

                    if (!opaque || distance >= radius) continue;

                    blocked = true;
                    CastLight(origin, distance + 1, currentStartSlope, leftSlope, radius, radiusSquared, xx, xy, yx, yy, isOpaque, onVisible);
                    nextStartSlope = rightSlope;
                }

                if (blocked)
                {
                    break;
                }
            }
        }
    }
}
