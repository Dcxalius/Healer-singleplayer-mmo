using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Managers;
using Project_1.Rendering;
using Project_1.Textures;
using System;
using System.Linq;
using CameraTypes = Project_1.Camera;

namespace Project_1.System.Models.BaseModels
{
    internal class BlockModel : Model3D
    {
        readonly struct FaceGeometry
        {
            public FaceGeometry(Vector3[] aCorners, Vector3 aNormal, Material aMaterial, int aSortOrder)
            {
                Corners = aCorners;
                Normal = aNormal;
                Material = aMaterial;
                SortOrder = aSortOrder;
            }

            public Vector3[] Corners { get; }
            public Vector3 Normal { get; }
            public Material Material { get; }
            public int SortOrder { get; }
        }

        public enum Sides
        {
            Up,
            Down,
            North,
            West,
            South,
            East
        }

        public Material[] Materials => materials;
        readonly Material[] materials;

        public float EdgeLength => edgeLength;
        readonly float edgeLength;

        public Matrix WorldTransform { get => worldTransform; set => worldTransform = value; }
        Matrix worldTransform = Matrix.Identity;

        public Camera3D RenderCamera { get => renderCamera; set => renderCamera = value; }
        Camera3D renderCamera;

        static readonly short[] quadIndices = { 0, 1, 2, 2, 1, 3 };
        static readonly RasterizerState rasterizerState = new RasterizerState { CullMode = CullMode.None };
        static readonly BlendState blendState = BlendState.AlphaBlend;
        static readonly SamplerState samplerState = SamplerState.LinearClamp;
        static readonly DepthStencilState depthStencilState = DepthStencilState.Default;
        static readonly Material fallbackMaterial = new Material("MissingMaterial", new GfxPath(GfxType.Debug, "MissingTexture"), Color.White);

        public BlockModel(int id, string name, Material[] aMaterials, float aEdgeLength = 1f) : base(id, name)
        {
            materials = aMaterials ?? Array.Empty<Material>();
            edgeLength = Math.Max(0.0001f, aEdgeLength);
        }

        public override void Update()
        {
        }

        public override void Render()
        {
            ThreadAffinity.AssertMainThread();
            Effect effect = EffectManager.GetEffect("ModelRender");
            GraphicsDevice device = GraphicsManager.GraphicsDevice;
            Viewport viewport = device.Viewport;

            if (renderCamera == null)
            {
                renderCamera = new Camera3D(
                    new CameraTypes.AbsoluteScreenPosition(viewport.Width, viewport.Height),
                    new CameraTypes.WorldSpace3D(0f, 0.8f, 4f),
                    new CameraTypes.WorldSpace3D(0f, 0f, 0f));
            }
            else
            {
                renderCamera.Resize(new CameraTypes.AbsoluteScreenPosition(viewport.Width, viewport.Height));
            }

            EffectParameter worldParam = effect.Parameters["World"];
            EffectParameter viewParam = effect.Parameters["View"];
            EffectParameter projectionParam = effect.Parameters["Projection"];
            EffectParameter tintParam = effect.Parameters["TintColor"];
            EffectParameter diffuseParam = effect.Parameters["DiffuseTexture"];

            worldParam?.SetValue(worldTransform);
            viewParam?.SetValue(renderCamera.View);
            projectionParam?.SetValue(renderCamera.Projection);

            device.BlendState = blendState;
            device.DepthStencilState = depthStencilState;
            device.RasterizerState = rasterizerState;
            device.SamplerStates[0] = samplerState;

            FaceGeometry[] faces = BuildFaces();
            FaceGeometry[] orderedFaces = faces
                .OrderBy(face => CalculateViewDepth(face))
                .ThenBy(face => face.SortOrder)
                .ToArray();

            for (int i = 0; i < orderedFaces.Length; i++)
            {
                FaceGeometry face = orderedFaces[i];
                Material material = face.Material ?? fallbackMaterial;
                Texture2D texture = TextureManager.GetTexture(material.DiffuseTexturePath ?? fallbackMaterial.DiffuseTexturePath);

                diffuseParam?.SetValue(texture);
                tintParam?.SetValue(material.Tint.ToVector4());

                VertexPositionColorTexture[] vertices = BuildVertices(face, material.Tint);
                EffectTechnique technique = effect.Techniques["ModelTextured"] ?? effect.CurrentTechnique;
                for (int passIndex = 0; passIndex < technique.Passes.Count; passIndex++)
                {
                    EffectPass pass = technique.Passes[passIndex];
                    pass.Apply();
                    device.DrawUserIndexedPrimitives(
                        PrimitiveType.TriangleList,
                        vertices,
                        0,
                        4,
                        quadIndices,
                        0,
                        2);
                }
            }
        }

        FaceGeometry[] BuildFaces()
        {
            float half = edgeLength / 2f;

            return new[]
            {
                new FaceGeometry(
                    new[]
                    {
                        new Vector3(-half, half, -half),
                        new Vector3(half, half, -half),
                        new Vector3(-half, half, half),
                        new Vector3(half, half, half)
                    },
                    Vector3.Up,
                    GetMaterial(Sides.Up),
                    (int)Sides.Up),
                new FaceGeometry(
                    new[]
                    {
                        new Vector3(-half, -half, half),
                        new Vector3(half, -half, half),
                        new Vector3(-half, -half, -half),
                        new Vector3(half, -half, -half)
                    },
                    Vector3.Down,
                    GetMaterial(Sides.Down),
                    (int)Sides.Down),
                new FaceGeometry(
                    new[]
                    {
                        new Vector3(-half, half, half),
                        new Vector3(half, half, half),
                        new Vector3(-half, -half, half),
                        new Vector3(half, -half, half)
                    },
                    Vector3.Forward,
                    GetMaterial(Sides.South),
                    (int)Sides.South),
                new FaceGeometry(
                    new[]
                    {
                        new Vector3(half, half, -half),
                        new Vector3(-half, half, -half),
                        new Vector3(half, -half, -half),
                        new Vector3(-half, -half, -half)
                    },
                    Vector3.Backward,
                    GetMaterial(Sides.North),
                    (int)Sides.North),
                new FaceGeometry(
                    new[]
                    {
                        new Vector3(-half, half, -half),
                        new Vector3(-half, half, half),
                        new Vector3(-half, -half, -half),
                        new Vector3(-half, -half, half)
                    },
                    Vector3.Left,
                    GetMaterial(Sides.West),
                    (int)Sides.West),
                new FaceGeometry(
                    new[]
                    {
                        new Vector3(half, half, half),
                        new Vector3(half, half, -half),
                        new Vector3(half, -half, half),
                        new Vector3(half, -half, -half)
                    },
                    Vector3.Right,
                    GetMaterial(Sides.East),
                    (int)Sides.East)
            };
        }

        Material GetMaterial(Sides aSide)
        {
            int index = (int)aSide;
            if (index < 0 || index >= materials.Length) return fallbackMaterial;
            return materials[index] ?? fallbackMaterial;
        }

        VertexPositionColorTexture[] BuildVertices(FaceGeometry aFace, Color aTint)
        {
            return new[]
            {
                new VertexPositionColorTexture(aFace.Corners[0], aTint, new Vector2(0f, 0f)),
                new VertexPositionColorTexture(aFace.Corners[1], aTint, new Vector2(1f, 0f)),
                new VertexPositionColorTexture(aFace.Corners[2], aTint, new Vector2(0f, 1f)),
                new VertexPositionColorTexture(aFace.Corners[3], aTint, new Vector2(1f, 1f))
            };
        }

        float CalculateViewDepth(FaceGeometry aFace)
        {
            Vector3 centre =
                (aFace.Corners[0] +
                aFace.Corners[1] +
                aFace.Corners[2] +
                aFace.Corners[3]) / 4f;
            Vector3 worldCentre = Vector3.Transform(centre, worldTransform);
            Vector3 viewCentre = Vector3.Transform(worldCentre, renderCamera.View);
            return viewCentre.Z;
        }
    }
}
