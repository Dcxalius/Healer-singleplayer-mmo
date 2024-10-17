using Microsoft.Xna.Framework;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Entities
{
    internal class Corpse : GameObject
    {


        public Corpse(Texture aGfx) : base(aGfx, Vector2.Zero)
        {
        }

        public void SpawnCorpe(Vector2 aPos)
        {
            Position = aPos;
            ObjectManager.AddCorpse(this);
        }
    }
}
