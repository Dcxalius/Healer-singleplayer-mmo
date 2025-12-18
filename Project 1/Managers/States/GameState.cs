using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Input;
using Project_1.Managers;
using Project_1.UI.HUD.Managers;
using Project_1.UI.UIElements.Boxes;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Managers.States
{
    internal abstract class GameState : State
    {
        protected SpriteBatch uIDraw;
        protected RenderTarget2D uITarget;
        protected RenderTarget2D plateTarget;
        RasterizerState rasterizerState;
        bool uiDirty = true;
        bool plateDirty = true;
        double uiHeartbeatTimer;
        const double uiHeartbeatSeconds = 1d;



        public GameState() : base() 
        {
            rasterizerState = new RasterizerState() { ScissorTestEnable = true };
            uIDraw = GraphicsManager.CreateSpriteBatch();
            uITarget = GraphicsManager.CreateRenderTarget(Camera.Camera.WindowSize);
            plateTarget = GraphicsManager.CreateRenderTarget(Camera.Camera.WindowSize);
            HUDManager.UiInvalidated += MarkUiDirty;
            HUDManager.PlatesInvalidated += MarkPlatesDirty;
        }
        public override bool Click(ClickEvent aClickEvent)
        {
            return HUDManager.Click(aClickEvent);
        }


        public override void OnEnter()
        {
            
        }

        public override void OnLeave()
        {
        }
        public override bool Release(ReleaseEvent aReleaseEvent) => HUDManager.Release(aReleaseEvent);


        public override void Rescale() //TODO: This is wrong, this should rescale everything
        {
            base.Rescale();
            HUDManager.Rescale();
            MarkUiDirty();
            uITarget = GraphicsManager.CreateRenderTarget(Camera.Camera.WindowSize);
            plateTarget = GraphicsManager.CreateRenderTarget(Camera.Camera.WindowSize);

        }

        public override bool Scroll(ScrollEvent aScrollEvent)
        {
            return HUDManager.Scroll(aScrollEvent);
        }

        public override void Update()
        {
            HUDManager.Update();
            uiHeartbeatTimer += TimeManager.SecondsSinceLastFrame;
        }
        protected void UIDraw()
        {
            if (!uiDirty && !plateDirty && uiHeartbeatTimer < uiHeartbeatSeconds) return;
            uiHeartbeatTimer = 0;
            uiDirty = false;
            plateDirty = false;

            GraphicsManager.SetRenderTarget(uITarget);
            GraphicsManager.ClearScreen(Color.Transparent);
            uIDraw.Begin(SpriteSortMode.Immediate, null, null, null, rasterizerState);

            HUDManager.DrawUi(uIDraw);
            DebugManager.Draw(uIDraw);

            uIDraw.End();
            GraphicsManager.SetRenderTarget(null);

            if (plateDirty)
            {
                GraphicsManager.SetRenderTarget(plateTarget);
                GraphicsManager.ClearScreen(Color.Transparent);
                uIDraw.Begin(SpriteSortMode.Immediate, null, null, null, rasterizerState);
                HUDManager.DrawPlates(uIDraw);
                uIDraw.End();
                GraphicsManager.SetRenderTarget(null);
            }
        }

        void MarkUiDirty()
        {
            uiDirty = true;
            uiHeartbeatTimer = 0;
        }

        void MarkPlatesDirty()
        {
            plateDirty = true;
            uiHeartbeatTimer = 0;
        }

        public override RenderTarget2D Draw()
        {
            throw new NotImplementedException();
        }

        public override void PopUp(DialogueBox aBox)
        {
            throw new NotImplementedException();
        }

        public override void RemovePopUp(DialogueBox aBox)
        {
            throw new NotImplementedException();
        }
    }
}
