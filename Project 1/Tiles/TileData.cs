using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Project_1.Textures;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Tiles
{
    internal class TileData : IComparable<TileData>
    {
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
        }

        public int CompareTo(TileData other)
        {
            if (other.id > id) return -1;
            if (other.id < id) return 1;
            return 0;
        }
    }
}
