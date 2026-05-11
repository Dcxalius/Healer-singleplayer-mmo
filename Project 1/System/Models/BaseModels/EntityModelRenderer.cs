using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Managers;
using Project_1.Rendering;
using Project_1.Textures;
using Project_1.Tiles;
using System;
using System.Collections.Generic;

namespace Project_1.System.Models.BaseModels
{
    internal static class EntityModelRenderer
    {
        const float GameplayModelFacingOffsetRadians = -MathHelper.PiOver2;
        const float BillboardFacingOffsetRadians = 0f;
        const float FacingIndicatorOffsetRadians = MathHelper.Pi;

        static readonly Dictionary<string, UnitModel> unitModelsByKey = new Dictionary<string, UnitModel>(StringComparer.OrdinalIgnoreCase);
        static readonly Dictionary<string, ProceduralModel3D> presentationModelsByKey = new Dictionary<string, ProceduralModel3D>(StringComparer.OrdinalIgnoreCase);

        public static void DrawSnapshots(IEnumerable<EntityModelRenderSnapshot> aSnapshots)
        {
            ThreadAffinity.AssertMainThread();
            if (!DebugManager.Mode(DebugMode.ModelPreview) || aSnapshots == null) return;

            Camera3D renderCamera = WorldBlockRenderer.CurrentCamera;
            if (renderCamera == null)
            {
                WorldBlockRenderer.PrepareFrame();
                renderCamera = WorldBlockRenderer.CurrentCamera;
                if (renderCamera == null) return;
            }

            foreach (EntityModelRenderSnapshot snapshot in aSnapshots)
            {
                DrawSnapshot(snapshot, renderCamera);
            }
        }

        static void DrawSnapshot(in EntityModelRenderSnapshot aSnapshot, Camera3D aCamera)
        {
            UnitModel baseModel = GetOrCreateBaseModel(aSnapshot);
            Vector3 worldPosition = aSnapshot.WorldPosition.ToVector3();

            float baseFacingYaw = aSnapshot.BaseFacingYawRadians;
            float frameFacingYaw = aSnapshot.FrameFacesCameraInPreview
                ? ResolveFacingYaw(aCamera.Position, aSnapshot.WorldPosition)
                : aSnapshot.FrameFacingYawRadians;
            float presentationFacingYaw = aSnapshot.PresentationFacesCameraInPreview
                ? ResolveFacingYaw(aCamera.Position, aSnapshot.WorldPosition)
                : aSnapshot.PresentationFacingYawRadians;

            Matrix baseWorldTransform = Matrix.CreateRotationY(baseFacingYaw + GameplayModelFacingOffsetRadians) * Matrix.CreateTranslation(worldPosition);
            Matrix facingIndicatorWorldTransform = Matrix.CreateRotationY(baseFacingYaw + FacingIndicatorOffsetRadians) * Matrix.CreateTranslation(worldPosition);
            Matrix frameWorldTransform = Matrix.CreateRotationY(frameFacingYaw + BillboardFacingOffsetRadians) * Matrix.CreateTranslation(worldPosition);
            Matrix presentationWorldTransform = Matrix.CreateRotationY(presentationFacingYaw + BillboardFacingOffsetRadians) * Matrix.CreateTranslation(worldPosition);

            RenderModel(baseModel.GameplayModel, baseWorldTransform, aCamera);
            RenderModel(baseModel.FacingIndicatorModel, facingIndicatorWorldTransform, aCamera);
            RenderModel(baseModel.FrameModel, frameWorldTransform, aCamera);
            RenderModel(GetOrCreatePresentationModel(aSnapshot, baseModel), presentationWorldTransform, aCamera);
        }

        static UnitModel GetOrCreateBaseModel(in EntityModelRenderSnapshot aSnapshot)
        {
            if (unitModelsByKey.TryGetValue(aSnapshot.DefinitionKey, out UnitModel existing))
            {
                return existing;
            }

            UnitModel created = new UnitModel(aSnapshot.ShellType, null);
            unitModelsByKey[aSnapshot.DefinitionKey] = created;
            return created;
        }

        static ProceduralModel3D GetOrCreatePresentationModel(in EntityModelRenderSnapshot aSnapshot, UnitModel aBaseModel)
        {
            if (!aSnapshot.HasPresentationTexture) return null;

            string textureKey = aSnapshot.PresentationTextureType + "\\" + aSnapshot.PresentationTextureName;
            string cacheKey = aSnapshot.DefinitionKey + "|" + textureKey + "|" + aSnapshot.PresentationFrameSizePixels + "|" + aSnapshot.PresentationDirectionIndex;
            if (presentationModelsByKey.TryGetValue(cacheKey, out ProceduralModel3D existing))
            {
                return existing;
            }

            GfxPath texturePath = new GfxPath(aSnapshot.PresentationTextureType, aSnapshot.PresentationTextureName);
            Point textureSize = TextureManager.GetTextureSize(texturePath);
            Rectangle sourceRectangle = ResolveSourceRectangle(textureSize, aSnapshot.PresentationFrameSizePixels, aSnapshot.PresentationDirectionIndex);
            float aspectRatio = sourceRectangle.Width / (float)Math.Max(1, sourceRectangle.Height);
            Material material = new Material("EntityPresentation_" + SanitizeKey(cacheKey), texturePath, Color.White);
            ProceduralModel3D created = BaseModelGeometryFactory.CreatePresentationPlane(
                aBaseModel.FrameHeight,
                aBaseModel.PictureLength,
                aspectRatio,
                material,
                textureSize,
                sourceRectangle) as ProceduralModel3D;
            if (created == null) return null;

            presentationModelsByKey[cacheKey] = created;
            return created;
        }

        static Rectangle ResolveSourceRectangle(Point aTextureSize, int aFrameSizePixels, int aDirectionIndex)
        {
            if (aFrameSizePixels > 0 &&
                aTextureSize.Y >= aFrameSizePixels &&
                aTextureSize.X >= aFrameSizePixels * 8)
            {
                int frameIndex = ((aDirectionIndex % 8) + 8) % 8;
                return new Rectangle(frameIndex * aFrameSizePixels, 0, aFrameSizePixels, aFrameSizePixels);
            }

            int width = Math.Max(1, Math.Min(aFrameSizePixels, aTextureSize.X));
            int height = Math.Max(1, Math.Min(aFrameSizePixels, aTextureSize.Y));
            return new Rectangle(0, 0, width, height);
        }

        static string SanitizeKey(string aKey)
        {
            return aKey
                .Replace('\\', '_')
                .Replace('/', '_')
                .Replace('|', '_')
                .Replace(':', '_');
        }

        static void RenderModel(Model3D aModel, Matrix aWorldTransform, Camera3D aCamera)
        {
            if (aModel is not ProceduralModel3D proceduralModel) return;

            proceduralModel.WorldTransform = aWorldTransform;
            proceduralModel.RenderCamera = aCamera;
            proceduralModel.Render();
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
