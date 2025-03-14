using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Project_1.GameObjects.Spawners;
using Project_1.GameObjects.Spawners.Pathing;
using Project_1.Managers;
using System;

namespace Project_1.Tiles
{
    internal class Chunk : IComparable<Chunk>
    {
        readonly Point TileSize = new Point(32);
        public static readonly Point ChunkSize = new Point(100);

        public Tile Tile((int, int) aXY) => Tile(aXY.Item1, aXY.Item2);
        public Tile Tile(int aX, int aY) => tiles[aX, aY];

        [JsonProperty]
        int[,] tilesAsIDs
        {
            get
            {
                int[,] tilesAsId = new int[ChunkSize.X, ChunkSize.Y];
                //int?[,] tilesAsId = new int?[tiles.GetLength(0),tiles.GetLength(1)];
                for (int i = 0; i < tilesAsId.GetLength(0); i++)
                {
                    for (int j = 0; j < tilesAsId.GetLength(1); j++)
                    {
                        tilesAsId[i, j] = Tile(i, j).ID;
                        //tilesAsId[i, j] = tiles[i, j].ID;

                    }
                }
                return tilesAsId;
            }
        }



        Tile[,] tiles;
        [JsonIgnore]
        public int ID => id;
        int id;
        public Chunk(Point aLeftUppermostTile, int aId) 
        {
            id = aId;
            tiles = new Tile[ChunkSize.X, ChunkSize.Y];

            for (int i = 0; i < ChunkSize.X; i++)
            {
                for (int j = 0; j < ChunkSize.Y; j++)
                {
                    Point pos = new Point(aLeftUppermostTile.X + TileSize.X * i, aLeftUppermostTile.Y + TileSize.Y * j);

                    if (i == 0 || j == 0 || i == ChunkSize.X - 1 || j == ChunkSize.Y - 1 || (i >= 4 && i < 6 && j >= 4 && j < 6))
                    {
                        tiles[i, j] = new Tile(TileFactory.GetTileData("Wall"), pos, new Point(i, j));
                    }
                    else
                    {
                        Tile leftTile = tiles[i - 1, j];
                        Tile upTile = tiles[i, j - 1];

                        float oddsOfDirt = 0.1f;

                        if (leftTile.Name == "Wall" || upTile.Name == "Wall")
                        {
                            oddsOfDirt++;
                        }
                        if (leftTile.Name == "Wall" || leftTile.Name == "Dirt")
                        {
                            oddsOfDirt += 0.1f;

                        }
                        if (upTile.Name == "Wall" || upTile.Name == "Dirt")
                        {
                            if (oddsOfDirt > 0)
                            {
                                oddsOfDirt += 0.3f;
                            }
                            oddsOfDirt += 0.2f;
                        }

                        if (RandomManager.RollDouble() < oddsOfDirt)
                        {
                            tiles[i, j] = new Tile(TileFactory.GetTileData("Dirt"), pos, new Point(i, j));
                        }
                        else
                        {
                            tiles[i, j] = new Tile(TileFactory.GetTileData("Grass"), pos, new Point(i, j));
                        }
                    }
                }
            }
            SpawnerManager.CreateNewSpawnZone(new string[] { "sheep" });
            for (int i = 0; i < 10; i++)
            {
                Point size = new Point(10 + RandomManager.RollInt(500), 10 + RandomManager.RollInt(500));
                Rectangle r = new Rectangle(new Point(RandomManager.RollInt(size.X), RandomManager.RollInt(size.Y)), size);
                SpawnerManager.CreateNewSpawner(0, new Wander(r));

            } 
        }

        [JsonConstructor]
        public Chunk(int[,] tilesAsIDs, int aId)
        {
            id = aId;
            Point tilePos = TileManager.GetChunkPosition(id);
            tiles = new Tile[tilesAsIDs.GetLength(0), tilesAsIDs.GetLength(1)];

            for (int i = 0; i < tiles.GetLength(0); i++)
            {
                for (int j = 0; j < tiles.GetLength(1); j++)
                {
                    Point pos = new Point(tilePos.X * ChunkSize.X + TileSize.X * i, tilePos.Y * ChunkSize.Y + TileSize.Y * j);

                    tiles[i, j] = new Tile(TileFactory.GetTileData(tilesAsIDs[i, j]), pos, new Point(i, j));
                }
            }
        }

        public int CompareTo(Chunk other)
        {
            if (id < other.id) return -1;
            if (id > other.id) return 1;
            throw new NotImplementedException();
        }

        public void Draw(SpriteBatch aBatch)
        {
            foreach (var tile in tiles)
            {
                tile.Draw(aBatch);
            }
        }
    }
}
