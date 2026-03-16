using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Managers;
using GfxTexture = Project_1.Textures.Texture;
using Project_1.UI.UIElements;
using Project_1.Tiles;
using Project_1.Textures;
using Project_1.GameObjects;

namespace Project_1.GameObjects.Entities
{
    internal readonly struct EntityRenderSnapshot : IRenderSnapshot
    {
        readonly int renderId;
        readonly WorldSpace position;
        readonly float feetPosY;
        readonly Point size;
        readonly GfxTexture.TextureRenderSnapshot texture;
        readonly VisualEffectSnapshotBatch effects;
        readonly bool selected;
        readonly Color relationColor;
        readonly Color minimapColor;
        readonly Color shadowColor;

        static readonly GfxPath shadowPath = new GfxPath(GfxType.Object, "Shadow");
        static readonly GfxPath selectRingPath = new GfxPath(GfxType.Object, "SelectRing");
        static Texture2D shadowTexture;
        static Texture2D selectRingTexture;
        static Point shadowTextureSize;
        static Point selectRingTextureSize;
        static bool shadowInitialized;
        static bool selectRingInitialized;

        public EntityRenderSnapshot(int renderId,
            WorldSpace position,
            float feetPosY,
            Point size,
            GfxTexture.TextureRenderSnapshot texture,
            VisualEffectSnapshotBatch effects,
            bool selected,
            Color relationColor,
            Color minimapColor,
            Color shadowColor)
        {
            this.renderId = renderId;
            this.position = position;
            this.feetPosY = feetPosY;
            this.size = size;
            this.texture = texture;
            this.effects = effects;
            this.selected = selected;
            this.relationColor = relationColor;
            this.minimapColor = minimapColor;
            this.shadowColor = shadowColor;
        }

        public int RenderId => renderId;

        public void Draw(SpriteBatch batch)
        {
            ThreadAffinity.AssertMainThread();
            Rectangle screenRect = new Rectangle(position.ToAbsoltueScreenPosition(), ScaledSize(size));
            if (Camera.Camera.ScreenspaceBoundsCheck(screenRect))
            {
                DrawShadow(batch, screenRect);
                if (selected)
                {
                    DrawSelectRing(batch, screenRect);
                }
            }

            GfxTexture.DrawSnapshot(batch, texture, position, feetPosY);
            effects.Draw(batch, position, feetPosY + 0.01f);
        }

        public void DrawMinimap(SpriteBatch batch, WorldSpace origin, AbsoluteScreenPosition minimapOffset, AbsoluteScreenPosition minimapSize)
        {
            ThreadAffinity.AssertMainThread();
            Minimap.minimapDot.Draw(batch,
                new Rectangle(new AbsoluteScreenPosition((position - origin).ToPoint()) / (Tile.Size) + minimapOffset + minimapSize / 2 + new Point(0, 1), new Point(1)),
                minimapColor);
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

        void DrawSelectRing(SpriteBatch batch, Rectangle screenRect)
        {
            EnsureSelectRingResources();
            DrawGroundEffect(batch, selectRingTexture, selectRingTextureSize, screenRect, relationColor, feetPosY - 1f);
        }

        static void EnsureShadowResources()
        {
            if (shadowInitialized) return;
            shadowTexture = TextureManager.GetTexture(shadowPath);
            shadowTextureSize = TextureManager.GetTextureSize(shadowPath);
            if (shadowTextureSize == Point.Zero) shadowTextureSize = shadowTexture.Bounds.Size;
            shadowInitialized = true;
        }

        static void EnsureSelectRingResources()
        {
            if (selectRingInitialized) return;
            selectRingTexture = TextureManager.GetTexture(selectRingPath);
            selectRingTextureSize = TextureManager.GetTextureSize(selectRingPath);
            if (selectRingTextureSize == Point.Zero) selectRingTextureSize = selectRingTexture.Bounds.Size;
            selectRingInitialized = true;
        }

        static void DrawGroundEffect(SpriteBatch batch, Texture2D texture, Point textureSize, Rectangle screenRect, Color color, float order)
        {
            if (texture == null) return;
            float depth = (order - Camera.Camera.WorldRectangle.Top) / (Camera.Camera.WorldRectangle.Bottom - Camera.Camera.WorldRectangle.Top);
            batch.Draw(texture, screenRect, null, color, 0f, new Vector2(0, -textureSize.Y / 2f), SpriteEffects.None, depth);
        }
    }
}
