using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Tiles
{
    internal class PathFinder
    {
        const float DefaultMaxSearchDistance = 1024f;

        Heap<PathFindingTile> availableTiles;
        List<PathFindingTile> searchedTiles;
        readonly float maxSearchDistanceSquared;

        public PathFinder(float aMaxSearchDistance = DefaultMaxSearchDistance)
        {
            float clampedDistance = Math.Max(1f, aMaxSearchDistance);
            maxSearchDistanceSquared = clampedDistance * clampedDistance;
            availableTiles = new Heap<PathFindingTile>();
            searchedTiles = new List<PathFindingTile>();
        }

        public Path GeneratePath(WorldSpace aStartPos, WorldSpace aEndPos, WorldSpace aSize)
        {
            Tile startTile = TileManager.GetTileUnder(aStartPos);
            Tile endTile = TileManager.GetTileUnder(aEndPos);

            if (!endTile.Walkable)
            {
                return GeneratePath(aStartPos, TileManager.FindClosestWalkableWorldSpace(aEndPos, aSize), aSize);
            }

            Vector2 startToEnd = aEndPos - aStartPos;
            if (startToEnd.LengthSquared() > maxSearchDistanceSquared) return null;

            if (endTile == startTile) return new Path(new List<Tile> { }, aEndPos);

            if (HasClearStraightLine(aStartPos, aEndPos, aSize))
            {
                return new Path(new List<Tile>(), aEndPos);
            }

            availableTiles.Add(new PathFindingTile(startTile, endTile));

            PathFindingTile currentTile = null;
            bool reachedTarget = false;

            while (availableTiles.Count > 0)
            {
                currentTile = availableTiles.PopFirst();
                if (currentTile == null) break;
                //DebugManager.AddDebugShape(new DebugTools.DebugSquare(currentTile.Tile.WorldRectangle, Color.Yellow * 0.4f));

                if (currentTile.Tile == endTile)
                {
                    reachedTarget = true;
                    break;
                }

                Tile[] neighbours = GetSortedNeighbours(TileManager.GetSurroundingTiles(currentTile.Tile));
                List<int> wallIndicies = new List<int>();
                for (int i = 0; i < neighbours.Length; i++)
                {
                    Tile tileNeighbour = neighbours[i];
                    if (tileNeighbour == null) continue;
                    if (!tileNeighbour.Walkable)
                    {
                        wallIndicies.Add(i);
                        continue;
                    }

                    Vector2 offsetToStart = tileNeighbour.Centre - aStartPos;
                    if (offsetToStart.LengthSquared() > maxSearchDistanceSquared) continue;

                    PathFindingTile neighbour = new PathFindingTile(currentTile, tileNeighbour, endTile);

                    if (tileNeighbour.GridPos.X == currentTile.Tile.GridPos.X || tileNeighbour.GridPos.Y == currentTile.Tile.GridPos.Y)
                    {
                        neighbour.SetHomeCost(currentTile.HomeCost + 10, currentTile);
                    }
                    else
                    {
                        bool adjecentToWall = false;
                        for (int j = 0; j < wallIndicies.Count; j++)
                        {
                            if (neighbours[wallIndicies[j]].IsAdjacent(neighbour.Tile))
                            {
                                adjecentToWall = true;
                                break;
                            }
                        }
                        if (adjecentToWall) break;

                        neighbour.SetHomeCost(currentTile.HomeCost + 14, currentTile);
                    }
                    if (searchedTiles.Contains(neighbour)) continue;

                    availableTiles.Add(neighbour);
                    searchedTiles.Add(neighbour);

                    //DebugManager.AddDebugShape(new DebugTools.DebugSquare(neighbour.Tile.WorldRectangle, Color.Red * 0.1f));
                }
            }

            List<Tile> returnTiles = new List<Tile>();
            Path returnPath = null;

            if (reachedTarget && currentTile != null)
            {
                while (currentTile != null)
                {
                    returnTiles.Add(currentTile.Tile);

                    //DebugManager.AddDebugShape(new DebugTools.DebugSquare(currentTile.Tile.WorldRectangle, Color.Green * 1f));

                    currentTile = currentTile.Parent;
                }

                returnPath = new Path(returnTiles, aEndPos);
            }

            availableTiles.Clear();
            searchedTiles.Clear();

            return returnPath;
        }

        bool HasClearStraightLine(WorldSpace aStartPos, WorldSpace aEndPos, WorldSpace aSize)
        {
            Vector2 delta = aEndPos - aStartPos;
            float distance = delta.Length();
            if (distance <= float.Epsilon) return true;

            Vector2 direction = Vector2.Normalize(delta);
            float sampleSpacing = Math.Min(TileManager.TileSize.X, TileManager.TileSize.Y) / 2f;
            int steps = Math.Max(1, (int)Math.Ceiling(distance / sampleSpacing));
            WorldSpace[] offsets = BuildCollisionOffsets(aSize);

            for (int i = 0; i <= steps; i++)
            {
                float progress = i / (float)steps;
                WorldSpace alongLine = aStartPos + (WorldSpace)(direction * (distance * progress));
                if (!PositionIsWalkable(alongLine, offsets)) return false;
            }

            return true;
        }

        WorldSpace[] BuildCollisionOffsets(WorldSpace aSize)
        {
            if (aSize == WorldSpace.Zero) return new[] { WorldSpace.Zero };

            WorldSpace halfSize = aSize / 2f;
            return new[]
            {
                WorldSpace.Zero,
                new WorldSpace( halfSize.X, 0),
                new WorldSpace(-halfSize.X, 0),
                new WorldSpace(0,  halfSize.Y),
                new WorldSpace(0, -halfSize.Y),
                new WorldSpace( halfSize.X,  halfSize.Y),
                new WorldSpace(-halfSize.X,  halfSize.Y),
                new WorldSpace( halfSize.X, -halfSize.Y),
                new WorldSpace(-halfSize.X, -halfSize.Y)
            };
        }

        bool PositionIsWalkable(WorldSpace aPosition, WorldSpace[] aOffsets)
        {
            for (int i = 0; i < aOffsets.Length; i++)
            {
                Tile tile = TryGetTileAt(aPosition + aOffsets[i]);
                if (tile == null || !tile.Walkable)
                {
                    return false;
                }
            }

            return true;
        }

        Tile TryGetTileAt(WorldSpace aPosition)
        {
            Chunk chunk = TileManager.GetChunkUnder(aPosition);
            if (chunk == null) return null;

            float localX = (aPosition.X - chunk.Position.X) / TileManager.TileSize.X;
            float localY = (aPosition.Y - chunk.Position.Y) / TileManager.TileSize.Y;
            int tileX = (int)Math.Floor(localX);
            int tileY = (int)Math.Floor(localY);

            if (tileX < 0 || tileX >= Chunk.ChunkSize.X || tileY < 0 || tileY >= Chunk.ChunkSize.Y)
            {
                return null;
            }

            return chunk.Tile(tileX, tileY);
        }

        Tile[] GetSortedNeighbours(Tile[,] aNeighbours) //UGLY EWW YUCK
        {
            Tile[] neighbours = new Tile[8];
            neighbours[4] = aNeighbours[0, 0];
            neighbours[0] = aNeighbours[0, 1];
            neighbours[5] = aNeighbours[0, 2];
            neighbours[1] = aNeighbours[1, 0];
            _ = aNeighbours[1, 1];
            neighbours[3] = aNeighbours[1, 2];
            neighbours[6] = aNeighbours[2, 0];
            neighbours[2] = aNeighbours[2, 1];
            neighbours[7] = aNeighbours[2, 2];



            return neighbours;
        }
    }

    


    internal class PathFindingTile : IHeapItem<PathFindingTile>
    {
        public int TotalCost => homeCost + goalCost;
        public int HeapIndex { get => heapIndex; set => heapIndex = value; }
        int heapIndex;

        public int HomeCost
        {
            get
            {
                return homeCost;
            }
        }

        public void SetHomeCost(int aHomeCost, PathFindingTile aPossibleParent)
        {
            if (homeCost > aHomeCost)
            {
                homeCost = aHomeCost;
                parent = aPossibleParent;
            }
        }

        int homeCost;
        int goalCost;

        public PathFindingTile Parent => parent;
        PathFindingTile parent;
        public Tile Tile => tile;


        Tile tile;


        public PathFindingTile(Tile aTile, Tile aEndTile) : this(null, aTile, aEndTile) { }
        public PathFindingTile(PathFindingTile aParent, Tile aTile, Tile aEndTile)
        {
            parent = aParent;
            tile = aTile;
            int xDistance = Math.Abs(aTile.GridPos.X - aEndTile.GridPos.X);
            int yDistance = Math.Abs(aTile.GridPos.Y - aEndTile.GridPos.Y);
            CalculateGoalCost(Math.Max(xDistance, yDistance), Math.Min(xDistance, yDistance));
        }

        void CalculateGoalCost(int aMaxXorY, int aMinXorY)
        {
            goalCost = aMinXorY * 14 + (aMaxXorY - aMinXorY) * 10;
        }

        public override bool Equals(object obj)
        {
            return obj is PathFindingTile tile &&
                   tile.Tile.GridPos == Tile.GridPos;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TotalCost, HomeCost, homeCost, goalCost, parent, Tile, tile);
        }

        [DebuggerStepThrough]

        public int CompareTo(PathFindingTile other)
        {
            int totalCostCompare = TotalCost.CompareTo(other.TotalCost);
            if (totalCostCompare != 0) return -totalCostCompare;
            return -goalCost.CompareTo(other.goalCost);
        }
    }
}
