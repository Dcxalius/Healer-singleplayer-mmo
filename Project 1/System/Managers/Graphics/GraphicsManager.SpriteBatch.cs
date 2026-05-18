using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Project_1.Managers
{
    internal static partial class GraphicsManager
    {
        readonly struct TrackedSpriteBatchState
        {
            public TrackedSpriteBatchState(
                SpriteSortMode sortMode,
                BlendState blendState,
                SamplerState samplerState,
                DepthStencilState depthStencilState,
                RasterizerState rasterizerState,
                Effect effect,
                Matrix? transformMatrix,
                bool active)
            {
                SortMode = sortMode;
                BlendState = blendState;
                SamplerState = samplerState;
                DepthStencilState = depthStencilState;
                RasterizerState = rasterizerState;
                Effect = effect;
                TransformMatrix = transformMatrix;
                Active = active;
            }

            public SpriteSortMode SortMode { get; }
            public BlendState BlendState { get; }
            public SamplerState SamplerState { get; }
            public DepthStencilState DepthStencilState { get; }
            public RasterizerState RasterizerState { get; }
            public Effect Effect { get; }
            public Matrix? TransformMatrix { get; }
            public bool Active { get; }
        }

        static readonly Dictionary<SpriteBatch, TrackedSpriteBatchState> trackedSpriteBatches = new Dictionary<SpriteBatch, TrackedSpriteBatchState>();

        public static void BeginSpriteBatch(
            SpriteBatch spriteBatch,
            SpriteSortMode sortMode = SpriteSortMode.Deferred,
            BlendState blendState = null,
            SamplerState samplerState = null,
            DepthStencilState depthStencilState = null,
            RasterizerState rasterizerState = null,
            Effect effect = null,
            Matrix? transformMatrix = null)
        {
            ThreadAffinity.AssertMainThread();
            spriteBatch.Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, transformMatrix);
            trackedSpriteBatches[spriteBatch] = new TrackedSpriteBatchState(sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, transformMatrix, true);
        }

        public static void EndSpriteBatch(SpriteBatch spriteBatch)
        {
            ThreadAffinity.AssertMainThread();
            spriteBatch.End();
            if (trackedSpriteBatches.TryGetValue(spriteBatch, out TrackedSpriteBatchState state))
            {
                trackedSpriteBatches[spriteBatch] = new TrackedSpriteBatchState(state.SortMode, state.BlendState, state.SamplerState, state.DepthStencilState, state.RasterizerState, state.Effect, state.TransformMatrix, false);
            }
        }

        public static void DrawWithTemporaryEffect(SpriteBatch spriteBatch, Effect effect, Action<SpriteBatch> draw)
        {
            ThreadAffinity.AssertMainThread();
            if (draw == null) return;
            if (effect == null || !trackedSpriteBatches.TryGetValue(spriteBatch, out TrackedSpriteBatchState state) || !state.Active)
            {
                draw(spriteBatch);
                return;
            }
            //TODO: This is too hacky.
            //The alternatives I can think of in the moment is either drawing the text seperately to a render target before the main draw call. Say whenever text is changed
            //Alternatively, use a Master shader that has all the effects and use shader parameters to control which effects are active.
            spriteBatch.End();
            spriteBatch.Begin(state.SortMode, state.BlendState, state.SamplerState, state.DepthStencilState, state.RasterizerState, effect, state.TransformMatrix);
            draw(spriteBatch);
            spriteBatch.End();
            spriteBatch.Begin(state.SortMode, state.BlendState, state.SamplerState, state.DepthStencilState, state.RasterizerState, state.Effect, state.TransformMatrix);
        }
    }
}
