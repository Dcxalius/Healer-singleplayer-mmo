using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Textures;
using Project_1.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Tiles
{
    internal abstract class Tile : GameObject
    {
        readonly static Point tileSize = new Point(32);

        bool walkable;

        public bool Walkable { get => walkable; }

        public Tile(bool aWalkable, string aTextureName, Point aPos) : base(new Textures.RandomlyGeneratedTexture(true, tileSize, new GfxPath(GfxType.Tile, aTextureName)), aPos.ToVector2())
        { 
            walkable = aWalkable;
        }
    }
}
