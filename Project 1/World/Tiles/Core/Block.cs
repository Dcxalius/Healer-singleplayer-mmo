using Newtonsoft.Json;
using System;

namespace Project_1.Tiles
{
    internal enum BlockMaterialType
    {
        Empty,
        Dirt,
        Grass,
        Stone,
        Sand,
        Water,
        Wood,
        Metal
    }

    internal enum BlockCollisionState
    {
        None,
        Walkable,
        Solid
    }

    internal enum BlockVisionState
    {
        Transparent,
        Partial,
        Opaque
    }

    internal sealed class BlockSurfaceData
    {
        public string SurfaceId => surfaceId;
        readonly string surfaceId;

        public string DecorationId => decorationId;
        readonly string decorationId;

        public int? TileIdOverride => tileIdOverride;
        readonly int? tileIdOverride;

        [JsonIgnore]
        public bool HasData => !string.IsNullOrWhiteSpace(surfaceId) || !string.IsNullOrWhiteSpace(decorationId) || tileIdOverride.HasValue;

        [JsonConstructor]
        public BlockSurfaceData(string surfaceId = null, string decorationId = null, int? tileIdOverride = null)
        {
            this.surfaceId = surfaceId;
            this.decorationId = decorationId;
            this.tileIdOverride = tileIdOverride;
        }
    }

    internal sealed class Block //TODO: This use use a system similar to Tile and TileData. Everything that is the same for all blocks of a given material should be in a BlockData class, and the Block class should just have a reference to its BlockData and any properties that can vary between blocks of the same material (like elevation and surface data). This will save a lot of memory as currently each block has its own copy of all its properties, even if they are the same as many other blocks. It will also make it easier to change properties of all blocks of a given material at once, and to add new materials without needing to change the Block class. The current implementation is very simple and straightforward, but it is not very efficient or flexible.
    {
        //TODO: Thinking about if we want blocks to be combination of materials and properties for visual purposes.
        //TODO: This also helps with construction of structures, like creating a wall that has stone on the outside but wood on the inside.
        
        public BlockMaterialType Material => material;
        BlockMaterialType material;

        public BlockCollisionState Collision => collision;
        BlockCollisionState collision;

        public BlockVisionState Vision => vision;
        BlockVisionState vision;

        public int Elevation => elevation;
        int elevation;

        public int Depth => depth;
        int depth;

        public BlockSurfaceData SurfaceData => surfaceData;
        BlockSurfaceData surfaceData;

        [JsonIgnore]
        public bool Solid => collision == BlockCollisionState.Solid;

        [JsonIgnore]
        public bool Walkable => collision != BlockCollisionState.Solid;

        [JsonIgnore]
        public bool Transparent => vision == BlockVisionState.Transparent;

        [JsonIgnore]
        public bool BlocksVision => vision == BlockVisionState.Opaque;

        [JsonIgnore]
        public bool HasSurfaceData => surfaceData?.HasData ?? false;

        [JsonConstructor]
        public Block(
            BlockMaterialType material,
            BlockCollisionState collision,
            BlockVisionState vision,
            int elevation = 0,
            int depth = 0,
            BlockSurfaceData surfaceData = null)
        {
            this.material = material;
            this.collision = collision;
            this.vision = vision;
            this.elevation = elevation;
            this.depth = Math.Max(0, depth);
            this.surfaceData = surfaceData;
        }

        public static Block CreateAir(int elevation = 0, int depth = 0)
        {
            return new Block(BlockMaterialType.Empty, BlockCollisionState.None, BlockVisionState.Transparent, elevation, depth);
        }

        public static Block CreateSolid(
            BlockMaterialType material,
            int elevation = 0,
            int depth = 0,
            BlockSurfaceData surfaceData = null,
            BlockVisionState vision = BlockVisionState.Opaque)
        {
            return new Block(material, BlockCollisionState.Solid, vision, elevation, depth, surfaceData);
        }

        public void SetMaterial(BlockMaterialType material)
        {
            this.material = material;
        }

        public void SetCollision(BlockCollisionState collision)
        {
            this.collision = collision;
        }

        public void SetVision(BlockVisionState vision)
        {
            this.vision = vision;
        }

        public void SetElevation(int elevation)
        {
            this.elevation = elevation;
        }

        public void SetDepth(int depth)
        {
            this.depth = Math.Max(0, depth);
        }

        public void SetSurfaceData(BlockSurfaceData surfaceData)
        {
            this.surfaceData = surfaceData;
        }
    }
}
