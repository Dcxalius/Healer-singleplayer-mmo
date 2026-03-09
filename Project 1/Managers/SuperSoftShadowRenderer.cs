using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Tiles;
using System.Diagnostics;

namespace Project_1.Managers
{
    internal static class SuperSoftShadowRenderer
    {
        const string ShadowEffectName = "SuperSoftShadows";

        static readonly BlendState maxBlendState = new BlendState
        {
            ColorSourceBlend = Blend.One,
            ColorDestinationBlend = Blend.One,
            ColorBlendFunction = BlendFunction.Max,
            AlphaSourceBlend = Blend.One,
            AlphaDestinationBlend = Blend.One,
            AlphaBlendFunction = BlendFunction.Max
        };

        static readonly BlendState darkenByMaskBlendState = new BlendState
        {
            ColorSourceBlend = Blend.Zero,
            ColorDestinationBlend = Blend.InverseSourceColor,
            ColorBlendFunction = BlendFunction.Add,
            AlphaSourceBlend = Blend.Zero,
            AlphaDestinationBlend = Blend.One,
            AlphaBlendFunction = BlendFunction.Add
        };

        static SpriteBatch spriteBatch;
        static Effect shadowEffect;
        static RenderTarget2D lightMaskTarget;
        static RenderTarget2D combinedMaskTarget;
        static Point renderSize;
        static bool initialized;

        static float LightPenetrationWorld => 0.01f;

        public static void DrawAndComposite(RenderTarget2D destination)
        {
            ThreadAffinity.AssertMainThread();
            long startTicks = Stopwatch.GetTimestamp();
            if (destination == null)
            {
                ShadowRenderTelemetry.RecordRender(0, 0, 0);
                return;
            }

            ShadowFrameSnapshot frameSnapshot = ShadowSnapshotManager.Snapshot;
            if (frameSnapshot == null || frameSnapshot.Count == 0)
            {
                ShadowRenderTelemetry.RecordRender(Stopwatch.GetTimestamp() - startTicks, 0, 0);
                return;
            }

            EnsureInitialized();
            EnsureTargets(new Point(destination.Width, destination.Height));
            if (lightMaskTarget == null || combinedMaskTarget == null || shadowEffect == null)
            {
                ShadowRenderTelemetry.RecordRender(Stopwatch.GetTimestamp() - startTicks, 0, 0);
                return;
            }

            EffectParameter matrixParam = shadowEffect.Parameters["u_matrix"];
            EffectParameter lightParam = shadowEffect.Parameters["u_light"];
            EffectParameter penetrationParam = shadowEffect.Parameters["LightPenetration"];
            if (matrixParam == null || lightParam == null || penetrationParam == null)
            {
                ShadowRenderTelemetry.RecordRender(Stopwatch.GetTimestamp() - startTicks, 0, 0);
                return;
            }

            Rectangle worldBounds = frameSnapshot.WorldBounds;
            if (worldBounds.Width <= 0 || worldBounds.Height <= 0)
            {
                worldBounds = Camera.Camera.WorldRectangle;
            }

            Matrix worldToClip = Matrix.CreateOrthographicOffCenter(
                worldBounds.Left,
                worldBounds.Right,
                worldBounds.Bottom,
                worldBounds.Top,
                0f,
                1f);

            GraphicsDevice device = Game1.Instance.GraphicsDevice;

            GraphicsManager.SetRenderTarget(combinedMaskTarget);
            GraphicsManager.ClearScreen(Color.Black);

            int renderedLights = 0;
            int drawCalls = 0;
            for (int i = 0; i < frameSnapshot.Count; i++)
            {
                ShadowLightSnapshot light = frameSnapshot.Lights[i];
                if (light.VertexCount <= 0 || light.IndexCount <= 0) continue;

                GraphicsManager.SetRenderTarget(lightMaskTarget);
                GraphicsManager.ClearScreen(Color.Black);

                device.BlendState = maxBlendState;
                device.DepthStencilState = DepthStencilState.None;
                device.RasterizerState = RasterizerState.CullNone;

                matrixParam.SetValue(worldToClip);
                lightParam.SetValue(new Vector3(light.Position.X, light.Position.Y, light.RadiusTiles * Tile.Size.X));
                penetrationParam.SetValue(LightPenetrationWorld);

                EffectTechnique technique = shadowEffect.Techniques["SoftShadow"] ?? shadowEffect.CurrentTechnique;
                for (int passIndex = 0; passIndex < technique.Passes.Count; passIndex++)
                {
                    EffectPass pass = technique.Passes[passIndex];
                    pass.Apply();
                    device.DrawUserIndexedPrimitives(
                        PrimitiveType.TriangleList,
                        light.Vertices,
                        0,
                        light.VertexCount,
                        light.Indices,
                        0,
                        light.IndexCount / 3);
                    drawCalls++;
                }

                GraphicsManager.SetRenderTarget(combinedMaskTarget);
                spriteBatch.Begin(
                    SpriteSortMode.Immediate,
                    maxBlendState,
                    SamplerState.PointClamp,
                    DepthStencilState.None,
                    RasterizerState.CullNone);
                spriteBatch.Draw(lightMaskTarget, new Rectangle(0, 0, renderSize.X, renderSize.Y), Color.White);
                spriteBatch.End();
                drawCalls++;
                renderedLights++;
            }

            if (renderedLights <= 0)
            {
                GraphicsManager.SetRenderTarget(destination);
                ShadowRenderTelemetry.RecordRender(Stopwatch.GetTimestamp() - startTicks, drawCalls, 0);
                return;
            }

            GraphicsManager.SetRenderTarget(destination);
            spriteBatch.Begin(
                SpriteSortMode.Immediate,
                darkenByMaskBlendState,
                SamplerState.PointClamp,
                DepthStencilState.None,
                RasterizerState.CullNone);
            spriteBatch.Draw(combinedMaskTarget, new Rectangle(0, 0, destination.Width, destination.Height), Color.White);
            spriteBatch.End();
            drawCalls++;
            ShadowRenderTelemetry.RecordRender(Stopwatch.GetTimestamp() - startTicks, drawCalls, renderedLights);
        }

        static void EnsureInitialized()
        {
            ThreadAffinity.AssertMainThread();
            if (initialized) return;
            initialized = true;

            spriteBatch = GraphicsManager.CreateSpriteBatch();
            shadowEffect = EffectManager.GetEffect(ShadowEffectName);
        }

        static void EnsureTargets(Point size)
        {
            ThreadAffinity.AssertMainThread();
            if (size.X <= 0 || size.Y <= 0) return;
            if (size == renderSize && lightMaskTarget != null && combinedMaskTarget != null) return;

            lightMaskTarget?.Dispose();
            combinedMaskTarget?.Dispose();
            lightMaskTarget = GraphicsManager.CreateRenderTarget(size);
            combinedMaskTarget = GraphicsManager.CreateRenderTarget(size);
            renderSize = size;
        }
    }
}
