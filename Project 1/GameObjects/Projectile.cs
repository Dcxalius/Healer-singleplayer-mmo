using Microsoft.Xna.Framework;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects
{
    internal class Projectile : MovingObject
    {
        Vector2 velVect;

        public Projectile(Vector2 pos) : base(new Texture(new GfxPath(GfxType.Object, "Arrow")), pos, 1f )
        {

        }
    }
}
