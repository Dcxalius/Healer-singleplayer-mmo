using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Managers;

namespace Project_1.Tiles
{
    /// <summary>
    /// Render-side cache for tile-derived GPU resources (transparency maps, etc.).
    /// </summary>
    internal static class TileRenderCache
    {
        static Tile cachedCentreTile;
        static Texture2D transparencyMap;
        const int TransparencySize = 65; // matches HLSL in TestDarkness.fx
        static readonly Dictionary<int, Texture2D> minimapTargets = new Dictionary<int, Texture2D>();

        public static Texture2D GetTransparencyMap(WorldSpace origin)
        {
            ThreadAffinity.AssertMainThread();

            Tile centre = TileManager.GetTileUnder(origin);
            if (cachedCentreTile != null && cachedCentreTile == centre && transparencyMap != null)
            {
                return transparencyMap;
            }

            cachedCentreTile = centre;
            transparencyMap ??= GraphicsManager.CreateNewTexture(new Point(TransparencySize));

            Color[] data = new Color[TransparencySize * TransparencySize];

            for (int x = 0; x < TransparencySize; x++)
            {
                for (int y = 0; y < TransparencySize; y++)
                {
                    Tile tile = TileManager.GetTile(origin + new WorldSpace(TileManager.TileSize.X * (x - TransparencySize / 2), TileManager.TileSize.Y * (y - TransparencySize / 2)));
                    if (tile == null)
                    {
                        data[y * TransparencySize + x] = new Color(0, 0, 0, 0);
                        continue;
                    }

                    data[y * TransparencySize + x] = tile.Transparent ? new Color(0, 0, 0, 0) : new Color(1, 1, 1, 1);
                }
            }

            transparencyMap.SetData(data);
            return transparencyMap;
        }

        public static Texture2D GetChunkMinimap(Chunk chunk)
        {
            ThreadAffinity.AssertMainThread();

            if (minimapTargets.TryGetValue(chunk.Id, out var cached))
            {
                return cached;
            }

            Texture2D rt = GraphicsManager.CreateRenderTarget(Chunk.ChunkSize);
            Color[] colors = new Color[Chunk.ChunkSize.X * Chunk.ChunkSize.Y];
            chunk.FillMinimapColors(colors);
            rt.SetData(colors);
            minimapTargets[chunk.Id] = rt;
            return rt;
        }
    }
}
