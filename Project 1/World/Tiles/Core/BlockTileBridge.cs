using Microsoft.Xna.Framework;

namespace Project_1.Tiles
{
    internal static class BlockTileBridge
    {
        public static Block CreateBlockFromTileId(int tileId)
        {
            TileData tileData = TileFactory.GetTileData(tileId);
            return new Block(
                ResolveMaterial(tileData),
                tileData.Walkable ? BlockCollisionState.Walkable : BlockCollisionState.Solid,
                tileData.Transparent ? BlockVisionState.Transparent : BlockVisionState.Opaque,
                surfaceData: new BlockSurfaceData(tileIdOverride: tileId));
        }

        public static Tile BuildSurfaceTile(Block[,,] blocks, int x, int y, Point worldPosition, Point localTilePosition)
        {
            TileData surfaceTileData = ResolveSurfaceTileData(blocks, x, y);
            if (surfaceTileData == null) return null;
            return new Tile(surfaceTileData, worldPosition, localTilePosition);
        }

        public static TileData ResolveSurfaceTileData(Block[,,] blocks, int x, int y)
        {
            Block topBlock = FindTopBlock(blocks, x, y);
            if (topBlock == null) return null;

            if (topBlock.SurfaceData?.TileIdOverride is int tileIdOverride)
            {
                return TileFactory.GetTileData(tileIdOverride);
            }

            if (!string.IsNullOrWhiteSpace(topBlock.SurfaceData?.SurfaceId))
            {
                return TileFactory.GetTileData(topBlock.SurfaceData.SurfaceId);
            }

            return ResolveTileData(topBlock);
        }

        public static Block FindTopBlock(Block[,,] blocks, int x, int y)
        {
            if (blocks == null) return null;

            int height = blocks.GetLength(2);
            for (int z = height - 1; z >= 0; z--)
            {
                Block block = blocks[x, y, z];
                if (block != null) return block;
            }

            return null;
        }

        public static TileData ResolveTileData(Block block)
        {
            return block.Material switch
            {
                BlockMaterialType.Grass => TileFactory.GetTileData("Grass"),
                BlockMaterialType.Dirt => TileFactory.GetTileData("Dirt"),
                BlockMaterialType.Sand => TileFactory.GetTileData("Dirt"),
                _ => block.Solid || block.BlocksVision
                    ? TileFactory.GetTileData("Wall")
                    : TileFactory.GetTileData("Grass")
            };
        }

        static BlockMaterialType ResolveMaterial(TileData tileData)
        {
            return tileData.Name switch
            {
                "Grass" => BlockMaterialType.Grass,
                "Dirt" => BlockMaterialType.Dirt,
                "Wall" => BlockMaterialType.Stone,
                _ => tileData.Walkable ? BlockMaterialType.Dirt : BlockMaterialType.Stone
            };
        }
    }
}
