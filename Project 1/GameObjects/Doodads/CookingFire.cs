using Project_1.Camera;
using Project_1.Textures;
using Project_1.Textures.AnimatedTextures;
using System;

namespace Project_1.GameObjects.Doodads
{
    internal sealed class CookingFire : Doodad
    {
        public CookingFire(WorldSpace aStartingPos)
            : base(new LoopingAnimatedTexture(new GfxPath(GfxType.Effect, "Flamestrike"), new Microsoft.Xna.Framework.Point(32), 0, TimeSpan.FromMilliseconds(180)), aStartingPos)
        {
        }
    }
}
