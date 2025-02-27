using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Tiles
{
    internal struct TileData : IComparable<TileData>
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
