using Microsoft.Xna.Framework;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects
{
    internal class Walker : Entity
    {
        public Walker(Vector2 aStartingPos) : base(new Textures.AnimatedTexture(new GfxPath(GfxType.Object, "Walker"), new Point(32), Textures.AnimatedTexture.AnimationType.Random, 0, TimeSpan.FromMilliseconds(500)), aStartingPos, 100)
        {

        }

        public void AssumingDirectControl(ref Player aPlayer)
        {
            aPlayer.AddToCommand(this);
        }

    }
}
