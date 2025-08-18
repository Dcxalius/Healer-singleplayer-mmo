using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Tiles
{
    internal class CollisionManager
    {
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
            return collisions;
        }

        List<Rectangle> ConvertUnwalkableTilesToRectangles(WorldSpace aPos)
        {
            Tile[,] tilesSurroundingObject = TileManager.GetSurroundingTiles(TileManager.GetTileUnder(aPos));

            Rectangle?[,] colliders = GetColliders(tilesSurroundingObject);

            return Merge(colliders);
        }

        Rectangle?[,] GetColliders(Tile[,] aTileArray)
        {
            Rectangle?[,] colliders = new Rectangle?[aTileArray.GetLength(0), aTileArray.GetLength(1)];
            for (int i = 0; i < aTileArray.GetLength(0); i++)
            {
                for (int j = 0; j < aTileArray.GetLength(1); j++)
                {
                    if (aTileArray[i, j] == null) continue;
                    if (!aTileArray[i, j].Walkable)
                    {
                        colliders[i, j] = aTileArray[i, j].WorldRectangle;
                    }
                }
            }

            return colliders;
        }

        List<Rectangle> Merge(Rectangle?[,] aCollidersToMerge)
        {
            List<Rectangle> finalColliders = RightMerge(aCollidersToMerge);
            finalColliders.AddRange(DownMerge(aCollidersToMerge));

            return finalColliders;
        }
        List<Rectangle> DownMerge(Rectangle?[,] aCollidersToCheck) //TODO: This has bugs in it
        {
            List<Rectangle> finalColliders = new List<Rectangle>();
            int[,] consumedBy = new int[aCollidersToCheck.GetLength(0), aCollidersToCheck.GetLength(1)];
            for (int i = 0; i < aCollidersToCheck.GetLength(0); i++)
            {
                for (int j = 0; j < aCollidersToCheck.GetLength(1) - 1; j++)
                {
                    if (aCollidersToCheck[i, j] == null || aCollidersToCheck[i, j + 1] == null)
                    {
                        continue;
                    }
                    if (aCollidersToCheck[i, j].Value.Bottom == aCollidersToCheck[i, j + 1].Value.Top && aCollidersToCheck[i, j].Value.X == aCollidersToCheck[i, j + 1].Value.X)
                    {
                        consumedBy[i, j + 1] = finalColliders.Count;
                        if (consumedBy[i, j] != 0)
                        {
                            Rectangle r = Rectangle.Union(finalColliders[consumedBy[i, j]], aCollidersToCheck[i, j + 1].Value);

                            finalColliders[consumedBy[i, j]] = r;
                            consumedBy[i, j + 1] = consumedBy[i, j];
                        }
                        else
                        {
                            finalColliders.Add(Rectangle.Union(aCollidersToCheck[i, j].Value, aCollidersToCheck[i, j + 1].Value));
                        }

                    }
                }

            }

            return finalColliders;
        }

        List<Rectangle> RightMerge(Rectangle?[,] aCollidersToCheck)
        {
            List<Rectangle> finalColliders = new List<Rectangle>();
            bool[] consumed = new bool[aCollidersToCheck.Length];
            for (int i = 0; i < aCollidersToCheck.GetLength(0) - 1; i++)
            {
                for (int j = 0; j < aCollidersToCheck.GetLength(1); j++)
                {
                    if (aCollidersToCheck[i, j] == null || aCollidersToCheck[i + 1, j] == null)
                    {
                        continue;
                    }
                    if (aCollidersToCheck[i, j].Value.Right == aCollidersToCheck[i + 1, j].Value.Left && aCollidersToCheck[i, j].Value.Y == aCollidersToCheck[i + 1, j].Value.Y)
                    {
                        consumed[i + 1] = true;
                        if (consumed[i])
                        {
                            Rectangle r = Rectangle.Union(finalColliders.Last(), aCollidersToCheck[i + 1, j].Value);

                            finalColliders.RemoveAt(finalColliders.Count - 1);

                            finalColliders.Add(r);
                        }
                        else
                        {
                            finalColliders.Add(Rectangle.Union(aCollidersToCheck[i, j].Value, aCollidersToCheck[i + 1, j].Value));
                        }

                    }
                }
            }
            return finalColliders;
        }
    }
}
