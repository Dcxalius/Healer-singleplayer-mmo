using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Tiles
{
    internal class PathFinder
    {
        List<PathFindingTile> availableTiles;
        List<PathFindingTile> searchedTiles;

        public PathFinder()
        {
            availableTiles = new List<PathFindingTile>();
            searchedTiles = new List<PathFindingTile>();
        }

        public Path GeneratePath(WorldSpace aStartPos, WorldSpace aEndPos)
        {
            Tile startTile = TileManager.GetTileUnder(aStartPos);
            Tile endTile = TileManager.GetTileUnder(aEndPos);

            if (endTile == startTile) return new Path(new List<Tile> { startTile });

            availableTiles.Add(new PathFindingTile(startTile, endTile));

            PathFindingTile currentTile = null;

            while (true)
            {
                currentTile = GetLowestTotalTile(availableTiles);
                DebugManager.AddDebugShape(new DebugTools.DebugSquare(currentTile.Tile.WorldRectangle, Color.Yellow * 0.4f));

                if (currentTile.Tile == endTile)
                {
                    break;
                }


                Tile[] neighbours = TileManager.GetNeighbours(currentTile.Tile.GridPos);
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

                    PathFindingTile neighbour = new PathFindingTile(currentTile, tileNeighbour, endTile);

                    if (tileNeighbour.GridPos.X == currentTile.Tile.GridPos.X || tileNeighbour.GridPos.Y == currentTile.Tile.GridPos.Y)
                    {
                        neighbour.SetHomeCost(currentTile.HomeCost + 10, currentTile);
                    }
                    else
                    {
                        //if (wallIndicies.Contains())
                        neighbour.SetHomeCost(currentTile.HomeCost + 14, currentTile);
                    }
                    if (searchedTiles.Contains(neighbour)) continue;

                    availableTiles.Add(neighbour);
                    searchedTiles.Add(neighbour);

                    DebugManager.AddDebugShape(new DebugTools.DebugSquare(neighbour.Tile.WorldRectangle, Color.Red * 0.2f));
                }
            }

            List<Tile> returnTiles = new List<Tile>();
            while(currentTile != null)
            {
                returnTiles.Add(currentTile.Tile);
                
                DebugManager.AddDebugShape(new DebugTools.DebugSquare(currentTile.Tile.WorldRectangle, Color.Green * 1f));
                
                currentTile = currentTile.Parent;
                
            }

            availableTiles.Clear();
            searchedTiles.Clear();
            return new Path(returnTiles);
        }


        PathFindingTile GetLowestTotalTile(List<PathFindingTile> aList)
        {

            PathFindingTile t = aList.MinBy(t => t.TotalCost);
            aList.Remove(t);
            return t;
        }
    }



    internal class PathFindingTile
    {
        public int TotalCost => homeCost + goalCost;

        public int HomeCost
        {
            get
            {
                return homeCost;
            }
        }

        public void SetHomeCost(int aHomeCost, PathFindingTile aPossibleParent)
        {
            if (homeCost < aHomeCost)
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
    }
}
