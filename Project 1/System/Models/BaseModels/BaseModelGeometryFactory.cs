using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Textures;
using System;
using System.Collections.Generic;

namespace Project_1.System.Models.BaseModels
{
    internal static class BaseModelGeometryFactory
    {
        const int FrustumSegments = 24;
        static readonly GfxPath baseTexturePath = new GfxPath(GfxType.Object, "BaseModel");
        static readonly GfxPath facingIndicatorTexturePath = new GfxPath(GfxType.Object, "BaseModelArrow");

        public static Model3D CreateGameplayBase(float aFrameHeight, float aPictureLength)
        {
            float baseHeight = ResolveBaseHeight(aFrameHeight, aPictureLength);
            float bottomRadius = Math.Max(0.15f, aPictureLength * 0.42f);
            float topRadius = bottomRadius * 0.76f;
            Material material = new Material("BaseModelGameplayBase", baseTexturePath, Color.White);
            return CreateCircularFrustum("GameplayBase", material, baseHeight, bottomRadius, topRadius);
        }

        public static Model3D CreateFrame(float aFrameHeight, float aPictureLength, float aPictureAspectRatio)
        {
            float aspectRatio = Math.Max(0.1f, aPictureAspectRatio);
            float baseHeight = ResolveBaseHeight(aFrameHeight, aPictureLength);
            (float pictureWidth, float pictureHeight) = ResolvePictureSize(aFrameHeight, aPictureLength, aspectRatio);

            float borderThickness = Math.Max(0.04f, Math.Min(pictureWidth, pictureHeight) * 0.12f);
            float depth = Math.Max(0.04f, borderThickness * 0.75f);
            float frameCenterY = baseHeight + pictureHeight * 0.5f;

            float outerWidth = pictureWidth + borderThickness * 2f;
            float outerHeight = pictureHeight + borderThickness * 2f;

            Material material = new Material("BaseModelPictureFrame", baseTexturePath, Color.White);
            List<VertexPositionColorTexture> vertices = new List<VertexPositionColorTexture>();
            List<short> indices = new List<short>();

            AppendBox(vertices, indices, new Vector3(0f, frameCenterY + outerHeight * 0.5f - borderThickness * 0.5f, 0f), new Vector3(outerWidth * 0.5f, borderThickness * 0.5f, depth * 0.5f), material.Tint);
            AppendBox(vertices, indices, new Vector3(0f, frameCenterY - outerHeight * 0.5f + borderThickness * 0.5f, 0f), new Vector3(outerWidth * 0.5f, borderThickness * 0.5f, depth * 0.5f), material.Tint);
            AppendBox(vertices, indices, new Vector3(-outerWidth * 0.5f + borderThickness * 0.5f, frameCenterY, 0f), new Vector3(borderThickness * 0.5f, pictureHeight * 0.5f, depth * 0.5f), material.Tint);
            AppendBox(vertices, indices, new Vector3(outerWidth * 0.5f - borderThickness * 0.5f, frameCenterY, 0f), new Vector3(borderThickness * 0.5f, pictureHeight * 0.5f, depth * 0.5f), material.Tint);

            return new ProceduralModel3D(0, "PictureFrame", material, vertices.ToArray(), indices.ToArray());
        }

        public static Model3D CreateFacingIndicator(float aFrameHeight, float aPictureLength)
        {
            float baseHeight = ResolveBaseHeight(aFrameHeight, aPictureLength);
            float bottomRadius = Math.Max(0.15f, aPictureLength * 0.42f);
            float topRadius = bottomRadius * 0.76f;
            float overlayHalfExtent = Math.Max(0.06f, topRadius * 0.7f);
            float overlayY = baseHeight + 0.01f;
            Material material = new Material("BaseModelFacingIndicator", facingIndicatorTexturePath, Color.White);

            List<VertexPositionColorTexture> vertices = new List<VertexPositionColorTexture>(4);
            List<short> indices = new List<short>(6);
            AppendQuad(
                vertices,
                indices,
                new Vector3(-overlayHalfExtent, overlayY, -overlayHalfExtent),
                new Vector3(overlayHalfExtent, overlayY, -overlayHalfExtent),
                new Vector3(-overlayHalfExtent, overlayY, overlayHalfExtent),
                new Vector3(overlayHalfExtent, overlayY, overlayHalfExtent),
                material.Tint);

            return new ProceduralModel3D(0, "FacingIndicator", material, vertices.ToArray(), indices.ToArray());
        }

        public static Model3D CreatePresentationPlane(float aFrameHeight, float aPictureLength, float aPictureAspectRatio, Material aMaterial, Point aTextureSize, Rectangle aSourceRectangle)
        {
            float aspectRatio = Math.Max(0.1f, aPictureAspectRatio);
            float baseHeight = ResolveBaseHeight(aFrameHeight, aPictureLength);
            (float pictureWidth, float pictureHeight) = ResolvePictureSize(aFrameHeight, aPictureLength, aspectRatio);
            float frameCenterY = baseHeight + pictureHeight * 0.5f;
            float planeDepth = 0.015f;
            Rectangle sourceRectangle = ClampSourceRectangle(aTextureSize, aSourceRectangle);
            Color tint = aMaterial?.Tint ?? Color.White;

            Vector2 uvTopLeft = BuildUv(aTextureSize, sourceRectangle.Left, sourceRectangle.Top);
            Vector2 uvTopRight = BuildUv(aTextureSize, sourceRectangle.Right, sourceRectangle.Top);
            Vector2 uvBottomLeft = BuildUv(aTextureSize, sourceRectangle.Left, sourceRectangle.Bottom);
            Vector2 uvBottomRight = BuildUv(aTextureSize, sourceRectangle.Right, sourceRectangle.Bottom);

            List<VertexPositionColorTexture> vertices = new List<VertexPositionColorTexture>(4);
            List<short> indices = new List<short>(6);

            AppendQuad(
                vertices,
                indices,
                new Vector3(-pictureWidth * 0.5f, frameCenterY + pictureHeight * 0.5f, planeDepth),
                new Vector3(pictureWidth * 0.5f, frameCenterY + pictureHeight * 0.5f, planeDepth),
                new Vector3(-pictureWidth * 0.5f, frameCenterY - pictureHeight * 0.5f, planeDepth),
                new Vector3(pictureWidth * 0.5f, frameCenterY - pictureHeight * 0.5f, planeDepth),
                tint,
                uvTopLeft,
                uvTopRight,
                uvBottomLeft,
                uvBottomRight);

            return new ProceduralModel3D(0, "PresentationPlane", aMaterial, vertices.ToArray(), indices.ToArray());
        }

        static ProceduralModel3D CreateCircularFrustum(string aName, Material aMaterial, float aHeight, float aBottomRadius, float aTopRadius)
        {
            List<VertexPositionColorTexture> vertices = new List<VertexPositionColorTexture>();
            List<short> indices = new List<short>();

            float bottomY = 0f;
            float topY = aHeight;

            for (int i = 0; i < FrustumSegments; i++)
            {
                float t0 = i / (float)FrustumSegments;
                float t1 = (i + 1) / (float)FrustumSegments;
                float angle0 = MathHelper.TwoPi * t0;
                float angle1 = MathHelper.TwoPi * t1;

                Vector3 bottom0 = new Vector3(MathF.Cos(angle0) * aBottomRadius, bottomY, MathF.Sin(angle0) * aBottomRadius);
                Vector3 bottom1 = new Vector3(MathF.Cos(angle1) * aBottomRadius, bottomY, MathF.Sin(angle1) * aBottomRadius);
                Vector3 top0 = new Vector3(MathF.Cos(angle0) * aTopRadius, topY, MathF.Sin(angle0) * aTopRadius);
                Vector3 top1 = new Vector3(MathF.Cos(angle1) * aTopRadius, topY, MathF.Sin(angle1) * aTopRadius);

                AppendQuad(vertices, indices, bottom0, bottom1, top0, top1, aMaterial.Tint, new Vector2(t0, 1f), new Vector2(t1, 1f), new Vector2(t0, 0f), new Vector2(t1, 0f));
                AppendTriangle(vertices, indices, new Vector3(0f, topY, 0f), top0, top1, aMaterial.Tint);
                AppendTriangle(vertices, indices, new Vector3(0f, bottomY, 0f), bottom1, bottom0, aMaterial.Tint);
            }

            return new ProceduralModel3D(0, aName, aMaterial, vertices.ToArray(), indices.ToArray());
        }

        static (float Width, float Height) ResolvePictureSize(float aFrameHeight, float aPictureLength, float aAspectRatio)
        {
            float width = Math.Max(0.1f, aPictureLength);
            float height = width / aAspectRatio;
            if (height <= aFrameHeight) return (width, height);

            height = Math.Max(0.1f, aFrameHeight);
            width = height * aAspectRatio;
            return (width, height);
        }

        static float ResolveBaseHeight(float aFrameHeight, float aPictureLength)
        {
            return Math.Max(0.1f, Math.Min(aFrameHeight * 0.35f, aPictureLength * 0.6f));
        }

        static Rectangle ClampSourceRectangle(Point aTextureSize, Rectangle aSourceRectangle)
        {
            int textureWidth = Math.Max(1, aTextureSize.X);
            int textureHeight = Math.Max(1, aTextureSize.Y);
            int width = Math.Clamp(aSourceRectangle.Width, 1, textureWidth);
            int height = Math.Clamp(aSourceRectangle.Height, 1, textureHeight);
            int x = Math.Clamp(aSourceRectangle.X, 0, textureWidth - width);
            int y = Math.Clamp(aSourceRectangle.Y, 0, textureHeight - height);
            return new Rectangle(x, y, width, height);
        }

        static Vector2 BuildUv(Point aTextureSize, int aX, int aY)
        {
            float width = Math.Max(1f, aTextureSize.X);
            float height = Math.Max(1f, aTextureSize.Y);
            return new Vector2(aX / width, aY / height);
        }

        static void AppendBox(List<VertexPositionColorTexture> aVertices, List<short> aIndices, Vector3 aCenter, Vector3 aHalfSize, Color aTint)
        {
            Vector3 min = aCenter - aHalfSize;
            Vector3 max = aCenter + aHalfSize;

            AppendQuad(aVertices, aIndices, new Vector3(min.X, max.Y, min.Z), new Vector3(max.X, max.Y, min.Z), new Vector3(min.X, max.Y, max.Z), new Vector3(max.X, max.Y, max.Z), aTint);
            AppendQuad(aVertices, aIndices, new Vector3(min.X, min.Y, max.Z), new Vector3(max.X, min.Y, max.Z), new Vector3(min.X, min.Y, min.Z), new Vector3(max.X, min.Y, min.Z), aTint);
            AppendQuad(aVertices, aIndices, new Vector3(min.X, max.Y, max.Z), new Vector3(max.X, max.Y, max.Z), new Vector3(min.X, min.Y, max.Z), new Vector3(max.X, min.Y, max.Z), aTint);
            AppendQuad(aVertices, aIndices, new Vector3(max.X, max.Y, min.Z), new Vector3(min.X, max.Y, min.Z), new Vector3(max.X, min.Y, min.Z), new Vector3(min.X, min.Y, min.Z), aTint);
            AppendQuad(aVertices, aIndices, new Vector3(min.X, max.Y, min.Z), new Vector3(min.X, max.Y, max.Z), new Vector3(min.X, min.Y, min.Z), new Vector3(min.X, min.Y, max.Z), aTint);
            AppendQuad(aVertices, aIndices, new Vector3(max.X, max.Y, max.Z), new Vector3(max.X, max.Y, min.Z), new Vector3(max.X, min.Y, max.Z), new Vector3(max.X, min.Y, min.Z), aTint);
        }

        static void AppendQuad(
            List<VertexPositionColorTexture> aVertices,
            List<short> aIndices,
            Vector3 aTopLeft,
            Vector3 aTopRight,
            Vector3 aBottomLeft,
            Vector3 aBottomRight,
            Color aTint,
            Vector2? aUvTopLeft = null,
            Vector2? aUvTopRight = null,
            Vector2? aUvBottomLeft = null,
            Vector2? aUvBottomRight = null)
        {
            short baseIndex = (short)aVertices.Count;
            aVertices.Add(new VertexPositionColorTexture(aTopLeft, aTint, aUvTopLeft ?? new Vector2(0f, 0f)));
            aVertices.Add(new VertexPositionColorTexture(aTopRight, aTint, aUvTopRight ?? new Vector2(1f, 0f)));
            aVertices.Add(new VertexPositionColorTexture(aBottomLeft, aTint, aUvBottomLeft ?? new Vector2(0f, 1f)));
            aVertices.Add(new VertexPositionColorTexture(aBottomRight, aTint, aUvBottomRight ?? new Vector2(1f, 1f)));
            aIndices.Add(baseIndex);
            aIndices.Add((short)(baseIndex + 1));
            aIndices.Add((short)(baseIndex + 2));
            aIndices.Add((short)(baseIndex + 2));
            aIndices.Add((short)(baseIndex + 1));
            aIndices.Add((short)(baseIndex + 3));
        }

        static void AppendTriangle(List<VertexPositionColorTexture> aVertices, List<short> aIndices, Vector3 aA, Vector3 aB, Vector3 aC, Color aTint)
        {
            short baseIndex = (short)aVertices.Count;
            aVertices.Add(new VertexPositionColorTexture(aA, aTint, new Vector2(0.5f, 0.5f)));
            aVertices.Add(new VertexPositionColorTexture(aB, aTint, new Vector2(0f, 1f)));
            aVertices.Add(new VertexPositionColorTexture(aC, aTint, new Vector2(1f, 1f)));
            aIndices.Add(baseIndex);
            aIndices.Add((short)(baseIndex + 1));
            aIndices.Add((short)(baseIndex + 2));
        }
    }
}
