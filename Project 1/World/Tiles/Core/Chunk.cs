using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Project_1.Camera;
using Project_1.GameObjects.Doodads;
using Project_1.Managers;
using System;

namespace Project_1.Tiles
{
    internal class Chunk : IComparable<Chunk>
    {
        static readonly Point TileSize = Tiles.Tile.Size;
        public static readonly Point ChunkSize = new Point(100);
        public const int ChunkHeight = 16;

        Block[,,] blocks;
        [JsonIgnore]
        Tile[,] surfaceTiles;
        [JsonIgnore]
        bool surfaceTilesBuilt;
        [JsonIgnore]
        ChunkRenderSnapshot renderSnapshot;
        [JsonIgnore]
        bool renderSnapshotBuilt;
        int id;
        [JsonProperty]
        int averageLevel;

        [JsonIgnore]
        public Rectangle WorldRectangle => new Rectangle(Position.ToPoint(), ChunkSize * TileSize);
        [JsonIgnore]
        public Point ChunkPosition { get; private set; }
        [JsonIgnore]
        public WorldSpace Position { get; private set; }
        [JsonIgnore]
        public DoodadManager Doodads { get; }
        public int Id => id;
        public int AverageLevel => averageLevel;

        [JsonProperty("blocks")]
        Block[,,] SerializedBlocks => blocks;

        public Chunk(Point aLeftUppermostTile, int aId)
        {
            id = aId;
            Doodads = new DoodadManager();
            Position = new WorldSpace(aLeftUppermostTile);
            ChunkPosition = ChunkAddressing.GetChunkPosition(aId);
            averageLevel = ChunkGenerator.GetAverageLevelForChunkPosition(ChunkPosition);
            InitializeBlocks(ChunkGenerator.GenerateBlocks(aId));
        }

        [JsonConstructor]
        public Chunk(Block[,,] blocks = null, int id = 0, int? averageLevel = null, BlockColumn[,] blocksAsColumns = null, int[,] tilesAsIDs = null)
        {
            ChunkPosition = ChunkAddressing.GetChunkPosition(id);
            Position = new WorldSpace(ChunkPosition * ChunkSize * TileSize);
            this.id = id;
            Doodads = new DoodadManager();
            this.averageLevel = Math.Clamp(averageLevel ?? ChunkGenerator.GetAverageLevelForChunkPosition(ChunkPosition), 1, 60);
            InitializeBlocks(blocks ?? ConvertLegacyColumns(blocksAsColumns) ?? ConvertLegacyTileIds(tilesAsIDs));
        }

        public Tile Tile((int, int) aXY) => Tile(aXY.Item1, aXY.Item2);

        // Temporary surface bridge for current 2D runtime systems.
        public Tile Tile(int aX, int aY)
        {
            ValidateXY(aX, aY);
            return GetSurfaceTiles()[aX, aY];
        }

        public Block GetBlock(int aX, int aY, int aZ)
        {
            ValidateXYZ(aX, aY, aZ);
            return blocks[aX, aY, aZ];
        }

        public Block GetTopBlock(int aX, int aY)
        {
            ValidateXY(aX, aY);
            return BlockTileBridge.FindTopBlock(blocks, aX, aY);
        }

        public bool SetBlock(int aX, int aY, int aZ, Block block)
        {
            ValidateXYZ(aX, aY, aZ);
            if (block == null) return false;
            blocks[aX, aY, aZ] = block;
            InvalidateDerivedState();
            return true;
        }

        public bool ClearBlock(int aX, int aY, int aZ)
        {
            ValidateXYZ(aX, aY, aZ);
            if (blocks[aX, aY, aZ] == null) return false;
            blocks[aX, aY, aZ] = null;
            InvalidateDerivedState();
            return true;
        }

        public bool AddBlock(int aX, int aY, Block block)
        {
            ValidateXY(aX, aY);
            if (block == null) return false;

            for (int z = 0; z < ChunkHeight; z++)
            {
                if (blocks[aX, aY, z] != null) continue;
                blocks[aX, aY, z] = block;
                InvalidateDerivedState();
                return true;
            }

            return false;
        }

        public bool SetTopBlock(int aX, int aY, Block block)
        {
            ValidateXY(aX, aY);
            if (block == null) return false;

            for (int z = ChunkHeight - 1; z >= 0; z--)
            {
                if (blocks[aX, aY, z] == null) continue;
                blocks[aX, aY, z] = block;
                InvalidateDerivedState();
                return true;
            }

            blocks[aX, aY, 0] = block;
            InvalidateDerivedState();
            return true;
        }

        internal void FillMinimapColors(Color[] buffer)
        {
            Tile[,] tiles = GetSurfaceTiles();
            for (int i = 0; i < buffer.Length; i++)
            {
                Tile tile = tiles[i % ChunkSize.X, i / ChunkSize.Y];
                buffer[i] = tile?.MinimapColor ?? Color.Transparent;
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
                ChunkBlockRenderSnapshot[] blockSnapshots = new ChunkBlockRenderSnapshot[width * height * ChunkHeight];
                int count = 0;
                Point chunkGridOrigin = ChunkPosition * ChunkSize;
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        for (int z = 0; z < ChunkHeight; z++)
                        {
                            Block block = blocks[i, j, z];
                            if (block == null) continue;

                            TileData tileData = BlockTileBridge.ResolveTileData(block);
                            if (tileData == null) continue;

                            BlockFaceMask exposedFaces = ResolveExposedFaces(i, j, z);
                            if (exposedFaces == BlockFaceMask.None) continue;

                            WorldSpace3D worldPosition = new WorldSpace3D(
                                chunkGridOrigin.X + i + 0.5f,
                                z + 0.5f,
                                chunkGridOrigin.Y + j + 0.5f);
                            blockSnapshots[count++] = new ChunkBlockRenderSnapshot(tileData.ID, worldPosition, exposedFaces);
                        }
                    }
                }

                if (count != blockSnapshots.Length)
                {
                    Array.Resize(ref blockSnapshots, count);
                }

                renderSnapshot = new ChunkRenderSnapshot(id, Position, ChunkPosition, blockSnapshots);
                renderSnapshotBuilt = true;
            }

            return renderSnapshot;
        }

        BlockFaceMask ResolveExposedFaces(int x, int y, int z)
        {
            BlockFaceMask mask = BlockFaceMask.None;
            if (IsEmpty(x, y, z + 1)) mask |= BlockFaceMask.Up;
            if (IsEmpty(x, y, z - 1)) mask |= BlockFaceMask.Down;
            if (IsEmpty(x, y - 1, z)) mask |= BlockFaceMask.North;
            if (IsEmpty(x - 1, y, z)) mask |= BlockFaceMask.West;
            if (IsEmpty(x, y + 1, z)) mask |= BlockFaceMask.South;
            if (IsEmpty(x + 1, y, z)) mask |= BlockFaceMask.East;
            return mask;
        }

        bool IsEmpty(int x, int y, int z)
        {
            if (x < 0 || x >= ChunkSize.X) return true;
            if (y < 0 || y >= ChunkSize.Y) return true;
            if (z < 0 || z >= ChunkHeight) return true;
            return blocks[x, y, z] == null;
        }

        public void Draw(SpriteBatch aBatch)
        {
            ThreadAffinity.AssertMainThread();
            Tile[,] tiles = GetSurfaceTiles();
            int minI = 0;
            int minJ = 0;
            int maxJ = tiles.GetLength(1);
            for (int i = minI; i < tiles.GetLength(0); i++)
            {
                for (int j = minJ; j < maxJ; j++)
                {
                    Tile tile = tiles[i, j];
                    if (tile == null) continue;

                    if (tile.WorldRectangle.Bottom < Camera.Camera.WorldRectangle.Top)
                    {
                        minJ = j + 1;
                        continue;
                    }

                    if (tile.WorldRectangle.Right < Camera.Camera.WorldRectangle.Left)
                    {
                        minI = i;
                        break;
                    }

                    if (tile.WorldRectangle.Left > Camera.Camera.WorldRectangle.Right)
                    {
                        return;
                    }

                    if (tile.WorldRectangle.Top > Camera.Camera.WorldRectangle.Bottom)
                    {
                        maxJ = j;
                        break;
                    }

                    tile.Draw(aBatch);
                }
            }
        }

        void InitializeBlocks(Block[,,] blocks)
        {
            this.blocks = NormalizeBlocks(blocks);
            InvalidateDerivedState();
        }

        Tile[,] GetSurfaceTiles()
        {
            if (!surfaceTilesBuilt)
            {
                surfaceTiles = new Tile[ChunkSize.X, ChunkSize.Y];
                Point chunkOrigin = Position.ToPoint();
                for (int i = 0; i < ChunkSize.X; i++)
                {
                    for (int j = 0; j < ChunkSize.Y; j++)
                    {
                        Point worldPos = new Point(chunkOrigin.X + TileSize.X * i, chunkOrigin.Y + TileSize.Y * j);
                        surfaceTiles[i, j] = BlockTileBridge.BuildSurfaceTile(blocks, i, j, worldPos, new Point(i, j));
                    }
                }

                surfaceTilesBuilt = true;
            }

            return surfaceTiles;
        }

        void InvalidateDerivedState()
        {
            surfaceTiles = null;
            surfaceTilesBuilt = false;
            renderSnapshot = default;
            renderSnapshotBuilt = false;
        }

        static Block[,,] NormalizeBlocks(Block[,,] source)
        {
            Block[,,] normalized = new Block[ChunkSize.X, ChunkSize.Y, ChunkHeight];
            if (source == null) return normalized;

            int maxX = Math.Min(source.GetLength(0), ChunkSize.X);
            int maxY = Math.Min(source.GetLength(1), ChunkSize.Y);
            int maxZ = Math.Min(source.GetLength(2), ChunkHeight);

            for (int x = 0; x < maxX; x++)
            {
                for (int y = 0; y < maxY; y++)
                {
                    for (int z = 0; z < maxZ; z++)
                    {
                        normalized[x, y, z] = source[x, y, z];
                    }
                }
            }

            return normalized;
        }

        static Block[,,] ConvertLegacyColumns(BlockColumn[,] blocksAsColumns)
        {
            if (blocksAsColumns == null) return null;

            Block[,,] converted = new Block[ChunkSize.X, ChunkSize.Y, ChunkHeight];
            int maxX = Math.Min(blocksAsColumns.GetLength(0), ChunkSize.X);
            int maxY = Math.Min(blocksAsColumns.GetLength(1), ChunkSize.Y);

            for (int x = 0; x < maxX; x++)
            {
                for (int y = 0; y < maxY; y++)
                {
                    if (blocksAsColumns[x, y]?.Blocks == null) continue;

                    int maxZ = Math.Min(blocksAsColumns[x, y].Blocks.Count, ChunkHeight);
                    for (int z = 0; z < maxZ; z++)
                    {
                        converted[x, y, z] = blocksAsColumns[x, y].Blocks[z];
                    }
                }
            }

            return converted;
        }

        static Block[,,] ConvertLegacyTileIds(int[,] tilesAsIDs)
        {
            if (tilesAsIDs == null) return null;

            Block[,,] converted = new Block[ChunkSize.X, ChunkSize.Y, ChunkHeight];
            int maxX = Math.Min(tilesAsIDs.GetLength(0), ChunkSize.X);
            int maxY = Math.Min(tilesAsIDs.GetLength(1), ChunkSize.Y);

            for (int x = 0; x < maxX; x++)
            {
                for (int y = 0; y < maxY; y++)
                {
                    converted[x, y, 0] = BlockTileBridge.CreateBlockFromTileId(tilesAsIDs[x, y]);
                }
            }

            return converted;
        }

        static void ValidateXY(int x, int y)
        {
            if (x < 0 || x >= ChunkSize.X || y < 0 || y >= ChunkSize.Y) throw new IndexOutOfRangeException();
        }

        static void ValidateXYZ(int x, int y, int z)
        {
            if (x < 0 || x >= ChunkSize.X || y < 0 || y >= ChunkSize.Y || z < 0 || z >= ChunkHeight) throw new IndexOutOfRangeException();
        }
    }
}
