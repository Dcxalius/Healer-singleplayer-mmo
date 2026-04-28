using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Managers;
using System;

namespace Project_1.Tiles
{
    [Flags]
    internal enum BlockFaceMask : byte
    {
        None = 0,
        Up = 1 << 0,
        Down = 1 << 1,
        North = 1 << 2,
        West = 1 << 3,
        South = 1 << 4,
        East = 1 << 5
    }

    internal readonly struct ChunkBlockRenderSnapshot
    {
        public ChunkBlockRenderSnapshot(int tileId, WorldSpace3D worldPosition, BlockFaceMask exposedFaces)
        {
            TileId = tileId;
            WorldPosition = worldPosition;
            ExposedFaces = exposedFaces;
        }

        public int TileId { get; }
        public WorldSpace3D WorldPosition { get; }
        public BlockFaceMask ExposedFaces { get; }
    }

    internal readonly struct ChunkRenderSnapshot : IRenderSnapshot
    {
        readonly int id;
        readonly WorldSpace position;
        readonly Point chunkPosition;
        readonly ChunkBlockRenderSnapshot[] blocks;

        public ChunkRenderSnapshot(int id, WorldSpace position, Point chunkPosition, ChunkBlockRenderSnapshot[] blocks)
        {
            this.id = id;
            this.position = position;
            this.chunkPosition = chunkPosition;
            this.blocks = blocks ?? Array.Empty<ChunkBlockRenderSnapshot>();
        }

        public int RenderId => id;

        public Rectangle WorldRectangle => new Rectangle(position.ToPoint(), Chunk.ChunkSize * Tile.Size);
        public Point ChunkPosition => chunkPosition;

        public void Draw()
        {
            ThreadAffinity.AssertMainThread();
            WorldBlockRenderer.DrawBlocks(chunkPosition, blocks);
        }

        public void MinimapDraw(SpriteBatch batch, WorldSpace origin, AbsoluteScreenPosition minimapOffset, AbsoluteScreenPosition minimapSize)
        {
            ThreadAffinity.AssertMainThread();
            var minimapTexture = TileRenderCache.GetChunkMinimap(id);
            if (minimapTexture == null) return;

            int tileX = (int)MathF.Floor((position.X - origin.X) / Tile.Size.X);
            int tileY = (int)MathF.Floor((position.Y - origin.Y) / Tile.Size.Y);
            Point minimapCentre = (minimapOffset + minimapSize / 2).ToPoint();
            var drawPos = new AbsoluteScreenPosition(minimapCentre + new Point(tileX, tileY));
            batch.Draw(minimapTexture, drawPos.ToVector2(), Color.White);
        }
    }
}
