using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Project_1.Camera;
using Project_1.GameObjects.Spawners;
using Project_1.GameObjects.Spawners.Pathing;
using Project_1.Managers;
using System;

namespace Project_1.Tiles
{
    internal class Chunk : IComparable<Chunk>
    {
        static readonly Point TileSize = Tiles.Tile.Size;
        public static readonly Point ChunkSize = new Point(100);
        [JsonIgnore]
        public Rectangle WorldRectangle => new Rectangle(Position.ToPoint(), ChunkSize * TileSize);

        [JsonIgnore]
        public Point ChunkPosition { get; private set; }

        [JsonIgnore]
        public WorldSpace Position { get; private set; }

        public Tile Tile((int, int) aXY) => Tile(aXY.Item1, aXY.Item2);
        public Tile Tile(int aX, int aY)
        {
            if (aX < 0 || aX >= ChunkSize.X || aY < 0 || aY >= ChunkSize.Y) throw new IndexOutOfRangeException();
            return tiles[aX, aY];
        }

        internal void FillMinimapColors(Color[] buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = tiles[i % ChunkSize.X, i / ChunkSize.Y].MinimapColor;
            }
        }

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
        ChunkRenderSnapshot renderSnapshot;
        [JsonIgnore]
        bool renderSnapshotBuilt;
        public int Id => id;
        int id;
        public int AverageLevel => averageLevel;
        [JsonProperty]
        int averageLevel;

        public static int[,] GenerateTileIds(int chunkId)
        {
            Point chunkPos = GetChunkPosition(chunkId);
            int dirtId = TileFactory.GetTileData("Dirt").ID;
            int grassId = TileFactory.GetTileData("Grass").ID;

            int[,] ids = new int[ChunkSize.X, ChunkSize.Y];
            int chunkTileX = chunkPos.X * ChunkSize.X;
            int chunkTileY = chunkPos.Y * ChunkSize.Y;

            for (int i = 0; i < ChunkSize.X; i++)
            {
                for (int j = 0; j < ChunkSize.Y; j++)
                {
                    int globalX = chunkTileX + i;
                    int globalY = chunkTileY + j;

                    float broadNoise = PerlinNoiseGenerator.Fractal01(globalX, globalY, seed: 4871, scale: 56f, octaves: 4, persistence: 0.5f, lacunarity: 2f);
                    float detailNoise = PerlinNoiseGenerator.Fractal01(globalX, globalY, seed: 9323, scale: 24f, octaves: 3, persistence: 0.55f, lacunarity: 2f);
                    float dirtBlend = (broadNoise * 0.75f) + (detailNoise * 0.25f);

                    ids[i, j] = dirtBlend < 0.5f ? dirtId : grassId;
                }
            }

            return ids;
        }
        public Chunk(Point aLeftUppermostTile, int aId) 
        {
            id = aId;
            tiles = new Tile[ChunkSize.X, ChunkSize.Y];
            Position = new WorldSpace(aLeftUppermostTile);
            ChunkPosition = GetChunkPosition(aId);
            averageLevel = GenerateAverageLevel(ChunkPosition);
            int[,] tileIds = GenerateTileIds(aId);

            Point pos;


            for (int i = 0; i < ChunkSize.X; i++)
            {
                for (int j = 0; j < ChunkSize.Y; j++)
                {
                    pos = new Point(aLeftUppermostTile.X + TileSize.X * i, aLeftUppermostTile.Y + TileSize.Y * j);
                    int tileId = tileIds[i, j];
                    tiles[i, j] = new Tile(TileFactory.GetTileData(tileId), pos, new Point(i, j));
                }
            }
            //SpawnerManager.CreateNewSpawnZone(new string[] { "sheep" });
            //for (int i = 0; i < 10; i++)
            //{
            //    Point size = new Point(10 + RandomManager.RollInt(500), 10 + RandomManager.RollInt(500));
            //    Rectangle r = new Rectangle(new Point(RandomManager.RollInt(size.X), RandomManager.RollInt(size.Y)), size);
            //    SpawnerManager.CreateNewSpawner(0, new Wander(r));

            //} 
        }

        [JsonConstructor]
        public Chunk(int[,] tilesAsIDs, int id, int? averageLevel = null)
        {
            ChunkPosition = GetChunkPosition(id);
            Position = new WorldSpace(ChunkPosition * ChunkSize * TileSize);
            this.id = id;
            this.averageLevel = Math.Clamp(averageLevel ?? GenerateAverageLevel(ChunkPosition), 1, 60);
            tiles = new Tile[tilesAsIDs.GetLength(0), tilesAsIDs.GetLength(1)];

            for (int i = 0; i < tiles.GetLength(0); i++)
            {
                for (int j = 0; j < tiles.GetLength(1); j++)
                {
                    Point pos = new Point((int)Position.X + TileSize.X * i, (int)Position.Y + TileSize.Y * j);

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

        internal ChunkRenderSnapshot BuildRenderSnapshot()
        {
            ThreadAffinity.AssertSimThread();
            if (!renderSnapshotBuilt)
            {
                int width = ChunkSize.X;
                int height = ChunkSize.Y;
                var snapshots = new Textures.Texture.TextureRenderSnapshot[width * height];
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        snapshots[i + j * width] = tiles[i, j].BuildRenderSnapshot();
                    }
                }
                renderSnapshot = new ChunkRenderSnapshot(id, Position, snapshots);
                renderSnapshotBuilt = true;
            }

            return renderSnapshot;
        }

        public void Draw(SpriteBatch aBatch)
        {
            ThreadAffinity.AssertMainThread();
            int minI = 0;
            int minJ = 0;
            int maxJ = tiles.GetLength(1);
            for (int i = minI; i < tiles.GetLength(0); i++)
            {
                for (int j = minJ; j < maxJ; j++)
                {
                    if (tiles[i, j].WorldRectangle.Bottom < Camera.Camera.WorldRectangle.Top)
                    {
                        minJ = j + 1;
                        continue;
                    }
                    if (tiles[i, j].WorldRectangle.Right < Camera.Camera.WorldRectangle.Left)
                    {
                        minI = i;
                        break;
                    }
                    if (tiles[i, j].WorldRectangle.Left > Camera.Camera.WorldRectangle.Right)
                    {
                        return;
                    }
                    if (tiles[i, j].WorldRectangle.Top > Camera.Camera.WorldRectangle.Bottom)
                    {
                        maxJ = j;
                        break;
                    }

                    tiles[i, j].Draw(aBatch);

                }
            }
        }

        public static int GetChunkId(Point pos) => GetChunkId(pos.X, pos.Y);

        public static int GetChunkId(int x, int y)
        {
            if (x == 0 && y == 0) return 0;

            int dirInt;
            int furthestDir;
            int shortestDir;

            if (Math.Abs(x) >= Math.Abs(y))
            {
                if (x < 0) //Left
                {
                    dirInt = 3;
                    shortestDir = y;
                }
                else //Right
                {
                    dirInt = 7;
                    shortestDir = -y;
                }

                furthestDir = x;
            }
            else
            {
                if (y < 0) //Up
                {
                    dirInt = 1;
                    shortestDir = -x;
                }
                else //Down
                {
                    dirInt = 5;
                    shortestDir = x;
                }

                furthestDir = y;
            }

            return dirInt * Math.Abs(furthestDir) + HighestNrInCircle(Math.Abs(furthestDir) - 1) + shortestDir;
        }

        public static Point GetChunkPosition(int id)
        {
            int circle = (int)Math.Ceiling((Math.Sqrt(id + 1) - 1) / 2);
            int highestNrInCircle = HighestNrInCircle(circle);
            int dif = highestNrInCircle - id;
            if (dif == 0)
            {
                return new Point(circle, -circle);
            }

            int sideLength = circle * 2;
            if (dif % sideLength == 0)
            {
                if (dif / sideLength == 1) return new Point(circle, circle);
                if (dif / sideLength == 2) return new Point(-circle, circle);
                if (dif / sideLength == 3) return new Point(-circle, -circle);
                throw new Exception("ohno");
            }

            Point returnPoint = new Point();

            if ((float)dif / sideLength < 1f)//Right
            {
                returnPoint.X = circle;
                returnPoint.Y = -circle + dif;
            }
            else if ((float)dif / sideLength < 2f)//Down
            {
                returnPoint.X = circle - (dif - sideLength);
                returnPoint.Y = circle;
            }
            else if ((float)dif / sideLength < 3f)//Left
            {
                returnPoint.X = -circle;
                returnPoint.Y = circle - (dif - sideLength * 2);
            }
            else if ((float)dif / sideLength < 4f)//Up
            {
                returnPoint.X = -circle + (dif - sideLength * 3);
                returnPoint.Y = -circle;
            }
            else throw new Exception("ohno");

            return returnPoint;
        }

        static int HighestNrInCircle(int circleSize) => 4 * (((circleSize + 1) * (circleSize + 1)) - (circleSize + 1));

        static int GenerateAverageLevel(Point chunkPos)
        {
            float macroNoise = PerlinNoiseGenerator.Fractal01(chunkPos.X, chunkPos.Y, seed: 14717, scale: 46f, octaves: 4, persistence: 0.5f, lacunarity: 2f);
            float detailNoise = PerlinNoiseGenerator.Fractal01(chunkPos.X, chunkPos.Y, seed: 21341, scale: 16f, octaves: 3, persistence: 0.55f, lacunarity: 2f);
            float worldLevelSignal = MathHelper.Clamp((macroNoise * 0.72f) + (detailNoise * 0.28f), 0f, 1f);

            float starterZoneNoise = PerlinNoiseGenerator.Fractal01(chunkPos.X, chunkPos.Y, seed: 38183, scale: 24f, octaves: 2, persistence: 0.5f, lacunarity: 2f);
            const float starterZoneThreshold = 0.11f;
            if (starterZoneNoise < starterZoneThreshold)
            {
                float pocketSignal = starterZoneNoise / starterZoneThreshold;
                float starterLevelSignal = pocketSignal * pocketSignal;
                return 1 + (int)MathF.Round(starterLevelSignal * 9f);
            }

            return 2 + (int)MathF.Round(worldLevelSignal * 58f);
        }
    }
}
