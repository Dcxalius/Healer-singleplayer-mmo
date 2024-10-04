using Microsoft.Xna.Framework;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects
{
    internal class Sheep : Entity
    {
        readonly static GfxPath path = new GfxPath(GfxType.Object, "Sheep");
        readonly static GfxPath corpsePath = new GfxPath(GfxType.Object, "SheepCorpse");
        readonly static Point visualSize = new Point(32);
        
        public Sheep(Vector2 aStartingPos) : base(new AnimatedTexture(path, visualSize, AnimatedTexture.AnimationType.Random, 0, TimeSpan.FromMilliseconds(250)), aStartingPos)
        {
            corpse = new Corpse(new Texture(corpsePath));
        }
    }
}
