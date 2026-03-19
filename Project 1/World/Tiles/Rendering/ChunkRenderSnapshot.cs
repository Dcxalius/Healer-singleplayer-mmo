using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Managers;
using GfxTexture = Project_1.Textures.Texture;
using System;

namespace Project_1.Tiles
{
    internal readonly struct ChunkRenderSnapshot : IRenderSnapshot
    {
        readonly int id;
        readonly WorldSpace position;
        readonly GfxTexture.TextureRenderSnapshot[] tiles;

        public ChunkRenderSnapshot(int id, WorldSpace position, GfxTexture.TextureRenderSnapshot[] tiles)
        {
            this.id = id;
            this.position = position;
            this.tiles = tiles ?? System.Array.Empty<GfxTexture.TextureRenderSnapshot>();
        }

        public int RenderId => id;

        public Rectangle WorldRectangle => new Rectangle(position.ToPoint(), Chunk.ChunkSize * Tile.Size);

        public void Draw(SpriteBatch batch)
        {
            ThreadAffinity.AssertMainThread();
            Rectangle cameraBounds = Camera.Camera.WorldRectangle;
            int minI = 0;
            int minJ = 0;
            int maxJ = Chunk.ChunkSize.Y;

            for (int i = minI; i < Chunk.ChunkSize.X; i++)
            {
                for (int j = minJ; j < maxJ; j++)
                {
                    Rectangle tileRect = new Rectangle(
                        (int)position.X + Tile.Size.X * i,
                        (int)position.Y + Tile.Size.Y * j,
                        Tile.Size.X,
                        Tile.Size.Y);

                    if (tileRect.Bottom < cameraBounds.Top)
                    {
                        minJ = j + 1;
                        continue;
                    }
                    if (tileRect.Right < cameraBounds.Left)
                    {
                        minI = i;
                        break;
                    }
                    if (tileRect.Left > cameraBounds.Right)
                    {
                        return;
                    }
                    if (tileRect.Top > cameraBounds.Bottom)
                    {
                        maxJ = j;
                        break;
                    }

                    int index = i + j * Chunk.ChunkSize.X;
                    GfxTexture.DrawSnapshot(batch, tiles[index], new WorldSpace(tileRect.Location), Camera.Camera.WorldRectangle.Top);
                }
            }
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
