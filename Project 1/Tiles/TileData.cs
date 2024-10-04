using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Tiles
{
    internal struct TileData
    {
        public string Name { get => name; }
        public bool Walkable { get => walkable; }
        public float DragCoeficient { get => dragCoeficient; }
        string name;
        bool walkable;
        float dragCoeficient;

        public TileData(string name, bool walkable, float dragCoeficient)
        {
            this.name = name;
            this.walkable = walkable;
            this.dragCoeficient = dragCoeficient;
        }
    }
}
