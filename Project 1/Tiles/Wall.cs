using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Tiles
{
    internal class Wall : Tile
    {
        public Wall(Point aPos) : base(false, "Wall", aPos)
        {
        }
    }
}
