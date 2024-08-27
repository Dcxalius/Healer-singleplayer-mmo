using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Textures;
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
        public Tile(string aTextureName, Point aPos) : base(new Textures.RandomlyGeneratedTexture(true, tileSize, new GfxPath(GfxType.Tile, aTextureName)), aPos.ToVector2())
        { 
        
        }

    }
}
