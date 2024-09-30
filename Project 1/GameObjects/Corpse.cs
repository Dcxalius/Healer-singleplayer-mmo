using Microsoft.Xna.Framework;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects 
{
    internal class Corpse : GameObject
    {
        public Corpse(Texture aGfx, Vector2 aStartingPos) : base(aGfx, aStartingPos)
        {
        }
    }
}
