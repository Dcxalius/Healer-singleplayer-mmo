using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.GameObjects.Entities.Friendlies.Players;
using Project_1.Managers;
using Project_1.Rendering;
using Project_1.Textures;
using Project_1.Tiles;
using System;
using System.Collections.Generic;

namespace Project_1.System.Models.BaseModels
{
    internal static class PlayerModelRenderer
    {
        const int SpriteFrameSizePixels = 32;

        static readonly UnitModel playerModel = new UnitModel(UnitModel.Type.Large, null);
        static readonly Dictionary<string, ProceduralModel3D> presentationModelsByPath = new Dictionary<string, ProceduralModel3D>(StringComparer.OrdinalIgnoreCase);

        public static void DrawPlayer()
        {
            ThreadAffinity.AssertMainThread();
            if (!DebugManager.Mode(DebugMode.ModelPreview)) return;

            Player player = ObjectManager.Player;
            if (player == null) return;

            Camera3D renderCamera = WorldBlockRenderer.CurrentCamera;
            if (renderCamera == null)
            {
                WorldBlockRenderer.PrepareFrame();
                renderCamera = WorldBlockRenderer.CurrentCamera;
                if (renderCamera == null) return;
            }

            WorldSpace3D worldPosition = ResolvePlayerWorldPosition(player);
            float facingYaw = ResolveFacingYaw(renderCamera.Position, worldPosition);
            Matrix worldTransform = Matrix.CreateRotationY(facingYaw) * Matrix.CreateTranslation(worldPosition.ToVector3());

            RenderModel(playerModel.GameplayModel, worldTransform, renderCamera);
            RenderModel(playerModel.FrameModel, worldTransform, renderCamera);
            RenderModel(GetOrCreatePresentationModel(player.PlayerData.GfxPath), worldTransform, renderCamera);
        }

        static void RenderModel(Model3D aModel, Matrix aWorldTransform, Camera3D aCamera)
        {
            if (aModel is not ProceduralModel3D proceduralModel) return;

            proceduralModel.WorldTransform = aWorldTransform;
            proceduralModel.RenderCamera = aCamera;
            proceduralModel.Render();
        }

        static ProceduralModel3D GetOrCreatePresentationModel(GfxPath aGfxPath)
        {
            if (aGfxPath == null || string.IsNullOrWhiteSpace(aGfxPath.Name)) return null;

            string cacheKey = aGfxPath.ToString();
            if (presentationModelsByPath.TryGetValue(cacheKey, out ProceduralModel3D existing))
            {
                return existing;
            }

            Point textureSize = TextureManager.GetTextureSize(aGfxPath);
            Rectangle sourceRectangle = ResolveSourceRectangle(textureSize);
            float aspectRatio = sourceRectangle.Width / (float)Math.Max(1, sourceRectangle.Height);
            Material material = new Material("PlayerPresentation_" + cacheKey.Replace('\\', '_'), aGfxPath, Color.White);
            ProceduralModel3D created = BaseModelGeometryFactory.CreatePresentationPlane(
                playerModel.FrameHeight,
                playerModel.PictureLength,
                aspectRatio,
                material,
                textureSize,
                sourceRectangle) as ProceduralModel3D;
            if (created == null) return null;

            presentationModelsByPath[cacheKey] = created;
            return created;
        }

        static Rectangle ResolveSourceRectangle(Point aTextureSize)
        {
            int width = Math.Max(1, Math.Min(SpriteFrameSizePixels, aTextureSize.X));
            int height = Math.Max(1, Math.Min(SpriteFrameSizePixels, aTextureSize.Y));
            return new Rectangle(0, 0, width, height);
        }

        static WorldSpace3D ResolvePlayerWorldPosition(Player aPlayer)
        {
            WorldSpace feetPosition = aPlayer.FeetPosition;
            return new WorldSpace3D(
                feetPosition.X / Tile.Size.X,
                ResolveSurfaceHeight(feetPosition),
                feetPosition.Y / Tile.Size.Y);
        }

        static float ResolveSurfaceHeight(WorldSpace aFeetPosition)
        {
            Chunk chunk = TileManager.GetChunk(aFeetPosition);
            if (chunk == null) return 0f;

            Point gridPosition = TileManager.GetGridPos(aFeetPosition);
            int localX = PositiveModulo(gridPosition.X, Chunk.ChunkSize.X);
            int localY = PositiveModulo(gridPosition.Y, Chunk.ChunkSize.Y);

            for (int z = Chunk.ChunkHeight - 1; z >= 0; z--)
            {
                if (chunk.GetBlock(localX, localY, z) != null)
                {
                    return z + 1f;
                }
            }

            return 0f;
        }

        static int PositiveModulo(int aValue, int aDivisor)
        {
            int result = aValue % aDivisor;
            return result < 0 ? result + aDivisor : result;
        }

        static float ResolveFacingYaw(WorldSpace3D aCameraPosition, WorldSpace3D aWorldPosition)
        {
            Vector3 toCamera = aCameraPosition.ToVector3() - aWorldPosition.ToVector3();
            toCamera.Y = 0f;
            if (toCamera.LengthSquared() <= float.Epsilon) return 0f;

            toCamera.Normalize();
            return MathF.Atan2(toCamera.X, toCamera.Z);
        }
    }
}
