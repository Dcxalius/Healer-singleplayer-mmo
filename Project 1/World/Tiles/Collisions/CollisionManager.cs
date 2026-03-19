using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects.Entities;
using System;
using System.Collections.Generic;

namespace Project_1.Tiles
{
    internal class CollisionManager
    {
        readonly List<Rectangle> mergedColliders;
        readonly Dictionary<(int X, int Width), ActiveStrip> activeStrips;
        readonly List<(int X, int Width)> keysToRemove;

        struct ActiveStrip
        {
            public Rectangle Rectangle;
            public int LastRow;
        }

        public CollisionManager()
        {
            mergedColliders = new List<Rectangle>();
            activeStrips = new Dictionary<(int X, int Width), ActiveStrip>();
            keysToRemove = new List<(int X, int Width)>();
        }

        public List<(Rectangle, Rectangle)> CollisionsWithUnwalkable(Entity aEntity)
        {
            List<Rectangle> finalColliders = ConvertUnwalkableTilesToRectangles(aEntity.FeetPosition);
            Rectangle entityRectangle = aEntity.WorldRectangle;
            List<(Rectangle, Rectangle)> collisions = new List<(Rectangle, Rectangle)>();
            foreach (var collider in finalColliders)
            {
                if (entityRectangle.Intersects(collider))
                {
                    collisions.Add((Rectangle.Intersect(entityRectangle, collider), collider));
                }
            }
            mergedColliders.Clear();
            return collisions;
        }

        List<Rectangle> ConvertUnwalkableTilesToRectangles(WorldSpace aPos)
        {
            Tile[,] tilesSurroundingObject = TileManager.GetSurroundingTiles(TileManager.GetTile(aPos));

            BuildMergedColliders(tilesSurroundingObject);
            return mergedColliders;
        }

        void BuildMergedColliders(Tile[,] tiles)
        {
            mergedColliders.Clear();
            activeStrips.Clear();
            keysToRemove.Clear();

            int width = tiles.GetLength(0);
            int height = tiles.GetLength(1);

            for (int row = 0; row < height; row++)
            {
                Tile runStartTile = null;
                int runStartIndex = -1;
                for (int column = 0; column < width; column++)
                {
                    Tile tile = tiles[column, row];
                    bool blocked = tile != null && !tile.Walkable;
                    if (blocked)
                    {
                        if (runStartIndex < 0)
                        {
                            runStartIndex = column;
                            runStartTile = tile;
                        }
                    }
                    else if (runStartIndex >= 0)
                    {
                        AddHorizontalStrip(runStartTile, runStartIndex, column, row);
                        runStartIndex = -1;
                        runStartTile = null;
                    }
                }

                if (runStartIndex >= 0 && runStartTile != null)
                {
                    AddHorizontalStrip(runStartTile, runStartIndex, width, row);
                    runStartIndex = -1;
                    runStartTile = null;
                }

                keysToRemove.Clear();
                foreach (var kvp in activeStrips)
                {
                    if (kvp.Value.LastRow < row)
                    {
                        mergedColliders.Add(kvp.Value.Rectangle);
                        keysToRemove.Add(kvp.Key);
                    }
                }
                for (int i = 0; i < keysToRemove.Count; i++)
                {
                    activeStrips.Remove(keysToRemove[i]);
                }
            }

            foreach (var kvp in activeStrips)
            {
                mergedColliders.Add(kvp.Value.Rectangle);
            }
            activeStrips.Clear();
            keysToRemove.Clear();
        }

        void AddHorizontalStrip(Tile startTile, int runStart, int runEnd, int row)
        {
            Point location = startTile.WorldRectangle.Location;
            int width = Tile.Size.X * (runEnd - runStart);
            Rectangle strip = new Rectangle(location, new Point(width, Tile.Size.Y));
            var key = (strip.X, strip.Width);

            if (activeStrips.TryGetValue(key, out ActiveStrip active))
            {
                if (active.LastRow == row - 1 && active.Rectangle.Bottom == strip.Top)
                {
                    active.Rectangle = Rectangle.Union(active.Rectangle, strip);
                    active.LastRow = row;
                    activeStrips[key] = active;
                }
                else
                {
                    mergedColliders.Add(active.Rectangle);
                    activeStrips[key] = new ActiveStrip
                    {
                        Rectangle = strip,
                        LastRow = row
                    };
                }
            }
            else
            {
                activeStrips[key] = new ActiveStrip
                {
                    Rectangle = strip,
                    LastRow = row
                };
            }
        }

        public WorldSpace FindClosestWalkableWorldSpace(WorldSpace aWorldSpace, WorldSpace aSize)
        {
            if (TileManager.GetTile(aWorldSpace).Walkable)
            {
                return aWorldSpace;
            }

            Tile closestTile = FindClosestWalkableTile(aWorldSpace);
            if (closestTile == null) return aWorldSpace;

            Vector2 start = aWorldSpace;
            Vector2 end = closestTile.Centre;
            if (start == end) return closestTile.Centre;

            float x = start.X / Tile.Size.X;
            float y = start.Y / Tile.Size.Y;
            float endX = end.X / Tile.Size.X;
            float endY = end.Y / Tile.Size.Y;

            Point startGrid = TileManager.GetGridPos(aWorldSpace);
            Point targetGrid = TileManager.GetGridPos(closestTile.Centre);
            int tileX = startGrid.X;
            int tileY = startGrid.Y;
            int targetX = targetGrid.X;
            int targetY = targetGrid.Y;

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
                bool movedX = false;
                bool movedY = false;
                float tCross;

                if (MathF.Abs(tMaxX - tMaxY) < epsilon)
                {
                    tCross = tMaxX;
                    tileX += stepX;
                    tileY += stepY;
                    tMaxX += tDeltaX;
                    tMaxY += tDeltaY;
                    movedX = true;
                    movedY = true;
                }
                else if (tMaxX < tMaxY)
                {
                    tCross = tMaxX;
                    tileX += stepX;
                    tMaxX += tDeltaX;
                    movedX = true;
                }
                else
                {
                    tCross = tMaxY;
                    tileY += stepY;
                    tMaxY += tDeltaY;
                    movedY = true;
                }

                Tile tile = TileManager.GetTileAtGrid(new Point(tileX, tileY));
                if (tile == null) return aWorldSpace;
                if (!tile.Walkable) continue;

                WorldSpace intersection = (WorldSpace)(start + (end - start) * tCross);
                if (movedX)
                {
                    float borderX = stepX > 0 ? tile.WorldRectangle.Left : tile.WorldRectangle.Right;
                    intersection.X = borderX + (stepX > 0 ? aSize.X / 2f : -aSize.X / 2f);
                }
                if (movedY)
                {
                    float borderY = stepY > 0 ? tile.WorldRectangle.Top : tile.WorldRectangle.Bottom;
                    intersection.Y = borderY + (stepY > 0 ? aSize.Y / 2f : -aSize.Y / 2f);
                }
                return intersection;
            }

            return closestTile.Centre;
        }

        public Tile FindClosestWalkableTile(WorldSpace aWorldSpace)
        {
            Tile underStart = TileManager.GetTile(aWorldSpace);
            if (underStart.Walkable) return underStart;

            float x = aWorldSpace.X / Tile.Size.X;
            x -= (MathF.Floor(x) + 0.5f);
            float y = aWorldSpace.Y / Tile.Size.Y;
            y -= (MathF.Floor(y) + 0.5f);

            return LookThroughNeighboursForWalkable(aWorldSpace, new WorldSpace(x, y), new WorldSpace(x, 0), new WorldSpace(Math.Abs(x) + 0.5f * -Math.Sign(x), 0), new WorldSpace(0, y), new WorldSpace(0, Math.Abs(y) + 0.5f * -Math.Sign(y)));
        }

        static Tile LookThroughNeighboursForWalkable(WorldSpace aStart, WorldSpace aStartInRelationToStartTile, WorldSpace aCloserDistanceToLeftRight, WorldSpace aFurtherDistanceToLeftRight, WorldSpace aCloserDistanceToTopBottom, WorldSpace aFurtherDistanceToTopBottom)
        {
            List<WorldSpace> spaces = new List<WorldSpace>();
            WorldSpace closerCloserDistance;
            WorldSpace closerFurtherDistance;
            WorldSpace furtherCloserDistance;
            WorldSpace furtherFurtherDistance;

            if (aCloserDistanceToLeftRight.DistanceTo(WorldSpace.Zero) > aCloserDistanceToTopBottom.DistanceTo(WorldSpace.Zero))
            {
                closerCloserDistance = aCloserDistanceToLeftRight;
                closerFurtherDistance = aCloserDistanceToTopBottom;
            }
            else
            {
                closerCloserDistance = aCloserDistanceToTopBottom;
                closerFurtherDistance = aCloserDistanceToLeftRight;
            }

            spaces.Add(closerCloserDistance);
            spaces.Add(closerFurtherDistance);
            spaces.Add(new WorldSpace(aCloserDistanceToLeftRight.X, aCloserDistanceToTopBottom.Y));

            if (aFurtherDistanceToLeftRight.DistanceTo(WorldSpace.Zero) > aFurtherDistanceToTopBottom.DistanceTo(WorldSpace.Zero))
            {
                furtherCloserDistance = aFurtherDistanceToLeftRight;
                furtherFurtherDistance = aFurtherDistanceToTopBottom;
                spaces.Add(furtherCloserDistance);
                spaces.Add(new WorldSpace(aFurtherDistanceToLeftRight.X, aCloserDistanceToTopBottom.Y));
                spaces.Add(furtherFurtherDistance);
                spaces.Add(new WorldSpace(aCloserDistanceToLeftRight.X, aFurtherDistanceToTopBottom.Y));
            }
            else
            {
                furtherCloserDistance = aFurtherDistanceToTopBottom;
                furtherFurtherDistance = aFurtherDistanceToLeftRight;

                spaces.Add(furtherCloserDistance);
                spaces.Add(new WorldSpace(aCloserDistanceToLeftRight.X, aFurtherDistanceToTopBottom.Y));
                spaces.Add(furtherFurtherDistance);
                spaces.Add(new WorldSpace(aFurtherDistanceToLeftRight.X, aCloserDistanceToTopBottom.Y));
            }

            spaces.Add(new WorldSpace(furtherFurtherDistance.X, furtherFurtherDistance.Y));

            for (int i = 0; i < spaces.Count; i++)
            {
                WorldSpace s = new WorldSpace(Math.Sign(spaces[i].X), Math.Sign(spaces[i].Y));
                spaces[i] = s;
            }

            int counter = 0;
            while (true)
            {
                counter++;
                List<int> removables = new List<int>();
                for (int i = 0; i < spaces.Count; i++)
                {
                    Tile t = TileManager.GetTile(aStart + new WorldSpace(Tile.Size.X, Tile.Size.Y) * spaces[i]); //TODO: Inefficent I think
                    if (t == null)
                    {
                        removables.Add(i);
                        continue;
                    }

                    if (t.Walkable)
                    {
                        return t;
                    }
                }
                for (int i = removables.Count - 1; i >= 0; i--)
                {
                    spaces.RemoveAt(removables[i]);
                }

                for (int i = spaces.Count - 1; i >= 0; i--)
                {
                    float x = Math.Sign(spaces[i].X);
                    float y = Math.Sign(spaces[i].Y);
                    spaces[i] += new WorldSpace(x, y);
                    WorldSpace s = spaces[i];
                    if (Math.Abs(spaces[i].X) == counter && Math.Abs(spaces[i].Y) == counter)
                    {
                        WorldSpace closerNew;
                        WorldSpace furtherNew;

                        WorldSpace newInXDir = new WorldSpace(s.X, y);
                        WorldSpace newInYDir = new WorldSpace(x, s.Y);

                        if (newInXDir.DistanceTo(aStartInRelationToStartTile) >= newInYDir.DistanceTo(aStartInRelationToStartTile))
                        {
                            closerNew = newInXDir;
                            furtherNew = newInYDir;
                        }
                        else
                        {
                            closerNew = newInYDir;
                            furtherNew = newInXDir;
                        }

                        spaces.Insert(i, closerNew);
                        spaces.Insert(i + 1, furtherNew);
                    }
                }
            }
        }
    }
}
