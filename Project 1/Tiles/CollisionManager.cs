using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects.Entities;
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
            Tile[,] tilesSurroundingObject = TileManager.GetSurroundingTiles(TileManager.GetTileUnder(aPos));

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
            int width = TileManager.TileSize.X * (runEnd - runStart);
            Rectangle strip = new Rectangle(location, new Point(width, TileManager.TileSize.Y));
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
    }
}
