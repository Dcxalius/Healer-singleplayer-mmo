using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects.Entities;
using Project_1.Managers;
using Project_1.Textures;
using GfxTexture = Project_1.Textures.Texture;

namespace Project_1.GameObjects
{
    internal readonly struct WorldObjectRenderSnapshot : IRenderSnapshot
    {
        readonly int renderId;
        readonly WorldSpace position;
        readonly float feetPosY;
        readonly Point size;
        readonly GfxTexture.TextureRenderSnapshot texture;
        readonly VisualEffectSnapshotBatch effects;
        readonly bool drawShadow;
        readonly Color shadowColor;

        static readonly GfxPath shadowPath = new GfxPath(GfxType.Object, "Shadow");
        static Texture2D shadowTexture;
        static Point shadowTextureSize;
        static bool shadowInitialized;

        public WorldObjectRenderSnapshot(int renderId,
            WorldSpace position,
            float feetPosY,
            Point size,
            GfxTexture.TextureRenderSnapshot texture,
            VisualEffectSnapshotBatch effects,
            bool drawShadow,
            Color shadowColor)
        {
            this.renderId = renderId;
            this.position = position;
            this.feetPosY = feetPosY;
            this.size = size;
            this.texture = texture;
            this.effects = effects;
            this.drawShadow = drawShadow;
            this.shadowColor = shadowColor;
        }

        public int RenderId => renderId;

        public void Draw(SpriteBatch batch)
        {
            ThreadAffinity.AssertMainThread();
            Rectangle screenRect = new Rectangle(position.ToAbsoltueScreenPosition(), ScaledSize(size));
            if (drawShadow && Camera.Camera.ScreenspaceBoundsCheck(screenRect))
            {
                DrawShadow(batch, screenRect);
            }

            GfxTexture.DrawSnapshot(batch, texture, position, feetPosY);
            effects.Draw(batch, position, feetPosY + 0.01f);
        }

        static Point ScaledSize(Point rawSize)
        {
            return new Point((int)System.Math.Ceiling(rawSize.X * Camera.Camera.Scale),
                (int)System.Math.Ceiling(rawSize.Y * Camera.Camera.Scale));
        }

        void DrawShadow(SpriteBatch batch, Rectangle screenRect)
        {
            EnsureShadowResources();
            DrawGroundEffect(batch, shadowTexture, shadowTextureSize, screenRect, shadowColor, feetPosY - 2f);
        }

        static void EnsureShadowResources()
        {
            if (shadowInitialized) return;
            shadowTexture = TextureManager.GetTexture(shadowPath);
            shadowTextureSize = TextureManager.GetTextureSize(shadowPath);
            if (shadowTextureSize == Point.Zero) shadowTextureSize = shadowTexture.Bounds.Size;
            shadowInitialized = true;
        }

        static void DrawGroundEffect(SpriteBatch batch, Texture2D texture, Point textureSize, Rectangle screenRect, Color color, float order)
        {
            if (texture == null) return;
            float depth = (order - Camera.Camera.WorldRectangle.Top) / (Camera.Camera.WorldRectangle.Bottom - Camera.Camera.WorldRectangle.Top);
            batch.Draw(texture, screenRect, null, color, 0f, new Vector2(0, -textureSize.Y / 2f), SpriteEffects.None, depth);
        }
    }
}
