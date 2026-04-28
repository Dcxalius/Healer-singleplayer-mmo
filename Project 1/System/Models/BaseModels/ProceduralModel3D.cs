using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Managers;
using Project_1.Rendering;
using Project_1.Textures;
using System;
using CameraTypes = Project_1.Camera;

namespace Project_1.System.Models.BaseModels
{
    internal sealed class ProceduralModel3D : Model3D
    {
        public Matrix WorldTransform { get => worldTransform; set => worldTransform = value; }
        Matrix worldTransform = Matrix.Identity;

        public Camera3D RenderCamera { get => renderCamera; set => renderCamera = value; }
        Camera3D renderCamera;

        readonly Material material;
        readonly VertexPositionColorTexture[] geometryVertices;
        readonly short[] indices;

        static readonly RasterizerState rasterizerState = new RasterizerState { CullMode = CullMode.None };
        static readonly BlendState blendState = BlendState.AlphaBlend;
        static readonly SamplerState samplerState = SamplerState.LinearClamp;
        static readonly DepthStencilState depthStencilState = DepthStencilState.Default;
        static readonly Material fallbackMaterial = new Material("MissingMaterial", new GfxPath(GfxType.Debug, "MissingTexture"), Color.White);

        public ProceduralModel3D(int aId, string aName, Material aMaterial, VertexPositionColorTexture[] aVertices, short[] aIndices)
            : base(aId, aName)
        {
            material = aMaterial ?? fallbackMaterial;
            geometryVertices = aVertices ?? Array.Empty<VertexPositionColorTexture>();
            indices = aIndices ?? Array.Empty<short>();
        }

        public override void Update()
        {
        }

        public override void Render()
        {
            ThreadAffinity.AssertMainThread();
            if (geometryVertices.Length == 0 || indices.Length == 0) return;

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

            Texture2D texture = TextureManager.GetTexture(material.DiffuseTexturePath ?? fallbackMaterial.DiffuseTexturePath);

            effect.Parameters["World"]?.SetValue(worldTransform);
            effect.Parameters["View"]?.SetValue(renderCamera.View);
            effect.Parameters["Projection"]?.SetValue(renderCamera.Projection);
            effect.Parameters["TintColor"]?.SetValue(material.Tint.ToVector4());
            effect.Parameters["DiffuseTexture"]?.SetValue(texture);

            device.BlendState = blendState;
            device.DepthStencilState = depthStencilState;
            device.RasterizerState = rasterizerState;
            device.SamplerStates[0] = samplerState;

            EffectTechnique technique = effect.Techniques["ModelTextured"] ?? effect.CurrentTechnique;
            for (int passIndex = 0; passIndex < technique.Passes.Count; passIndex++)
            {
                EffectPass pass = technique.Passes[passIndex];
                pass.Apply();
                device.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    geometryVertices,
                    0,
                    geometryVertices.Length,
                    indices,
                    0,
                    indices.Length / 3);
            }
        }
    }
}
