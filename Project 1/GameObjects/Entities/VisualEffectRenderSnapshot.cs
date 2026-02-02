using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Managers;
using GfxTexture = Project_1.Textures.Texture;

namespace Project_1.GameObjects.Entities
{
    internal readonly struct VisualEffectRenderSnapshot
    {
        readonly GfxTexture.TextureRenderSnapshot texture;

        public VisualEffectRenderSnapshot(GfxTexture.TextureRenderSnapshot texture)
        {
            this.texture = texture;
        }

        public void Draw(SpriteBatch batch, WorldSpace position, float feetPosY)
        {
            ThreadAffinity.AssertMainThread();
            GfxTexture.DrawSnapshot(batch, texture, position, feetPosY);
        }
    }
}
