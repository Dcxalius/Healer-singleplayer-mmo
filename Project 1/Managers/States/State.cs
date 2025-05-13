using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Input;
using Project_1.UI.UIElements.Boxes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Managers.States
{
    internal abstract class State
    {
        protected SpriteBatch spriteBatch;
        protected RenderTarget2D renderTarget;

        public abstract StateManager.States GetStateEnum { get; }

        protected State()
        {
            spriteBatch = GraphicsManager.CreateSpriteBatch();
            renderTarget = GraphicsManager.CreateRenderTarget(Camera.Camera.WindowSize);
        }

        public abstract void Update();
        public abstract void OnEnter();
        public abstract void OnLeave();

        public virtual void Rescale()
        {
            renderTarget = GraphicsManager.CreateRenderTarget(Camera.Camera.WindowSize);
        }

        public abstract void PopUp(DialogueBox aBox);
        public abstract void RemovePopUp(DialogueBox aBox);

        public abstract bool Click(ClickEvent aClickEvent);
        public abstract bool Release(ReleaseEvent aReleaseEvent);
        public abstract bool Scroll(ScrollEvent aScrollEvent);
        public abstract RenderTarget2D Draw();

        public virtual void PrepRender(Color aClearColor, SpriteSortMode sortMode = SpriteSortMode.Deferred, BlendState blendState = null, SamplerState samplerState = null, DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null, Effect effect = null, Matrix? transformMatrix = null)
        {
            GraphicsManager.SetRenderTarget(renderTarget);
            
            spriteBatch.Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, transformMatrix);
            GraphicsManager.ClearScreen(aClearColor);

        }

        public virtual void CleanRender()
        {
            spriteBatch.End();
            GraphicsManager.SetRenderTarget(null);
        }

    }
}
