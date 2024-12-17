using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Tiles
{
    internal struct TileData
    {
        public string Name => name;
        string name;
        public bool Walkable => walkable;
        bool walkable;
        public float DragCoeficient => dragCoeficient;
        float dragCoeficient;

        public bool Transparent => transparent;
        bool transparent;

        public TileData(string name, bool walkable, float dragCoeficient, bool transparent)
        {
            this.name = name;
            this.walkable = walkable;
            this.dragCoeficient = dragCoeficient;
            this.transparent = transparent;
        }
    }
}
