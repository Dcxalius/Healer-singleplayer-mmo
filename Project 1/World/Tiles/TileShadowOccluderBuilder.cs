using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Project_1.Managers;

namespace Project_1.Tiles
{
    internal readonly struct TileShadowSegment
    {
        public TileShadowSegment(Vector2 a, Vector2 b)
        {
            A = a;
            B = b;
        }

        public Vector2 A { get; }
        public Vector2 B { get; }
    }

    internal static class TileShadowOccluderBuilder
    {
        public static int BuildSegments(Rectangle tileBounds, List<TileShadowSegment> destination, int maxSegments)
        {
            return BuildSegments(tileBounds, destination, maxSegments, out _);
        }

        public static int BuildSegments(Rectangle tileBounds, List<TileShadowSegment> destination, int maxSegments, out bool capped)
        {
            ThreadAffinity.AssertSimThread();
            capped = false;
            if (destination == null || maxSegments <= 0) return 0;
            destination.Clear();

            int minX = tileBounds.Left;
            int minY = tileBounds.Top;
            int maxX = tileBounds.Right;
            int maxY = tileBounds.Bottom;

            // Horizontal runs: top and bottom edges.
            for (int y = minY; y < maxY; y++)
            {
                int x = minX;
                while (x < maxX)
                {
                    if (HasTopBoundary(x, y))
                    {
                        int runStart = x;
                        x++;
                        while (x < maxX && HasTopBoundary(x, y)) x++;
                        if (!TryAddHorizontalTop(destination, maxSegments, runStart, x - 1, y))
                        {
                            capped = true;
                            return destination.Count;
                        }
                        continue;
                    }

                    if (HasBottomBoundary(x, y))
                    {
                        int runStart = x;
                        x++;
                        while (x < maxX && HasBottomBoundary(x, y)) x++;
                        if (!TryAddHorizontalBottom(destination, maxSegments, runStart, x - 1, y))
                        {
                            capped = true;
                            return destination.Count;
                        }
                        continue;
                    }

                    x++;
                }
            }

            // Vertical runs: left and right edges.
            for (int x = minX; x < maxX; x++)
            {
                int y = minY;
                while (y < maxY)
                {
                    if (HasLeftBoundary(x, y))
                    {
                        int runStart = y;
                        y++;
                        while (y < maxY && HasLeftBoundary(x, y)) y++;
                        if (!TryAddVerticalLeft(destination, maxSegments, x, runStart, y - 1))
                        {
                            capped = true;
                            return destination.Count;
                        }
                        continue;
                    }

                    if (HasRightBoundary(x, y))
                    {
                        int runStart = y;
                        y++;
                        while (y < maxY && HasRightBoundary(x, y)) y++;
                        if (!TryAddVerticalRight(destination, maxSegments, x, runStart, y - 1))
                        {
                            capped = true;
                            return destination.Count;
                        }
                        continue;
                    }

                    y++;
                }
            }

            return destination.Count;
        }

        public static void BuildMesh(IReadOnlyList<TileShadowSegment> segments, ShadowVertex[] vertices, short[] indices, out int vertexCount, out int indexCount)
        {
            ThreadAffinity.AssertSimThread();
            vertexCount = 0;
            indexCount = 0;
            if (segments == null || vertices == null || indices == null) return;

            int segmentCount = segments.Count;
            int requiredVertices = segmentCount * 4;
            int requiredIndices = segmentCount * 6;
            if (vertices.Length < requiredVertices || indices.Length < requiredIndices)
            {
                return;
            }

            for (int i = 0; i < segmentCount; i++)
            {
                TileShadowSegment segment = segments[i];
                Vector4 packed = new Vector4(segment.B.X, segment.B.Y, segment.A.X, segment.A.Y);
                int v = i * 4;
                vertices[v + 0] = new ShadowVertex(packed, new Vector2(0f, 0f));
                vertices[v + 1] = new ShadowVertex(packed, new Vector2(1f, 0f));
                vertices[v + 2] = new ShadowVertex(packed, new Vector2(0f, 1f));
                vertices[v + 3] = new ShadowVertex(packed, new Vector2(1f, 1f));

                int ii = i * 6;
                short baseIndex = (short)v;
                indices[ii + 0] = (short)(baseIndex + 0);
                indices[ii + 1] = (short)(baseIndex + 1);
                indices[ii + 2] = (short)(baseIndex + 2);
                indices[ii + 3] = (short)(baseIndex + 2);
                indices[ii + 4] = (short)(baseIndex + 1);
                indices[ii + 5] = (short)(baseIndex + 3);
            }

            vertexCount = requiredVertices;
            indexCount = requiredIndices;
        }

        static bool IsOpen(int gridX, int gridY)
        {
            Tile neighbour = TileManager.GetTileAtGrid(new Point(gridX, gridY));
            return neighbour == null || neighbour.Transparent;
        }

        static bool IsSolid(int gridX, int gridY)
        {
            Tile tile = TileManager.GetTileAtGrid(new Point(gridX, gridY));
            return tile != null && !tile.Transparent;
        }

        static bool HasTopBoundary(int x, int y) => IsSolid(x, y) && IsOpen(x, y - 1);
        static bool HasBottomBoundary(int x, int y) => IsSolid(x, y) && IsOpen(x, y + 1);
        static bool HasLeftBoundary(int x, int y) => IsSolid(x, y) && IsOpen(x - 1, y);
        static bool HasRightBoundary(int x, int y) => IsSolid(x, y) && IsOpen(x + 1, y);

        static bool TryAddHorizontalTop(List<TileShadowSegment> destination, int maxSegments, int startX, int endX, int y)
        {
            float wy = y * Tile.Size.Y;
            float ax = (endX + 1) * Tile.Size.X;
            float bx = startX * Tile.Size.X;
            return TryAdd(destination, maxSegments, new Vector2(ax, wy), new Vector2(bx, wy));
        }

        static bool TryAddHorizontalBottom(List<TileShadowSegment> destination, int maxSegments, int startX, int endX, int y)
        {
            float wy = (y + 1) * Tile.Size.Y;
            float ax = startX * Tile.Size.X;
            float bx = (endX + 1) * Tile.Size.X;
            return TryAdd(destination, maxSegments, new Vector2(ax, wy), new Vector2(bx, wy));
        }

        static bool TryAddVerticalLeft(List<TileShadowSegment> destination, int maxSegments, int x, int startY, int endY)
        {
            float wx = x * Tile.Size.X;
            float ay = startY * Tile.Size.Y;
            float by = (endY + 1) * Tile.Size.Y;
            return TryAdd(destination, maxSegments, new Vector2(wx, ay), new Vector2(wx, by));
        }

        static bool TryAddVerticalRight(List<TileShadowSegment> destination, int maxSegments, int x, int startY, int endY)
        {
            float wx = (x + 1) * Tile.Size.X;
            float ay = (endY + 1) * Tile.Size.Y;
            float by = startY * Tile.Size.Y;
            return TryAdd(destination, maxSegments, new Vector2(wx, ay), new Vector2(wx, by));
        }

        static bool TryAdd(List<TileShadowSegment> destination, int maxSegments, Vector2 a, Vector2 b)
        {
            if (destination.Count >= maxSegments) return false;
            destination.Add(new TileShadowSegment(a, b));
            return true;
        }
    }
}
