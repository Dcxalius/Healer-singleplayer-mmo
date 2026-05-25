using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Tiles
{
    internal class TileData : IComparable<TileData> //TODO: Change name
    {
        static readonly Point tileVisibleSize = Tile.Size;
        public int ID => id;
        int id;
        public string Name => name;
        string name;
        public bool Walkable => walkable;
        bool walkable;
        public float DragCoeficient => dragCoeficient;
        float dragCoeficient;

        public bool Transparent => transparent;
        bool transparent;
        [JsonIgnore]
        internal Texture Texture => texture;
        [JsonIgnore]
        readonly Texture texture; //TODO: Currently the same texture is used for all sides, we should have the option to specify different textures for each side
        [JsonIgnore]
        readonly GfxPath texturePath;
        [JsonIgnore]
        readonly Point textureSheetSize;

        [JsonIgnore]
        public Color AvgColor
        {
            get
            {
                if (avgColor == null)
                {
                    avgColor = Textures.Texture.AvgColor(new GfxPath(GfxType.Tile, Name));
                }
                return avgColor.Value;
            }
        }
        Color? avgColor;

        public TileData(int id, string name, bool walkable, float dragCoeficient, bool transparent)
        {
            this.id = id;
            this.name = name;
            this.walkable = walkable;
            this.dragCoeficient = dragCoeficient;
            this.transparent = transparent;
            texturePath = new GfxPath(GfxType.Tile, name);
            texture = new Texture(texturePath, tileVisibleSize); //TODO: Currently each 
            textureSheetSize = TextureCatalog.GetSize(texturePath);
        }

        /// <summary>
        /// Uses the hash of the tiles id and position to generate a consistent random offset for the tile's texture. This allows us to have some variation in the appearance of tiles without needing to create separate textures for each variation.
        /// </summary>
        /// <param name="worldTile"></param>
        /// <returns></returns>
        internal Point GetRandomTextureOffset(Point worldTile)
        {
            //TODO: Ponder if we should do more to create variation, like rotation of the texture or flipping it.
            int maxOffsetX = Math.Max(0, textureSheetSize.X - tileVisibleSize.X);
            int maxOffsetY = Math.Max(0, textureSheetSize.Y - tileVisibleSize.Y);
            if (maxOffsetX == 0 && maxOffsetY == 0) return Point.Zero;

            int hash = HashCode.Combine(id, worldTile.X, worldTile.Y);
            int x = maxOffsetX == 0 ? 0 : PositiveModulo(hash, maxOffsetX + 1);
            int y = maxOffsetY == 0 ? 0 : PositiveModulo(hash * 31, maxOffsetY + 1);
            return new Point(x, y);
        }

        internal Texture.TextureRenderSnapshot BuildRenderSnapshot(Point textureOffset)
        {
            return texture.BuildRenderSnapshot(new Rectangle(textureOffset, tileVisibleSize));
        }

        static int PositiveModulo(int value, int modulo) //TODO: Move this to a math utility class if we need it anywhere else
        {
            int result = value % modulo;
            return result < 0 ? result + modulo : result;
        }

        public int CompareTo(TileData other)
        {
            if (other.id > id) return -1;
            if (other.id < id) return 1;
            return 0;
        }
    }
}
