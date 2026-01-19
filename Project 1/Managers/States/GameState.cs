using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Input;
using Project_1.Managers;
using Project_1.UI;
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
            return false;
        }


        public override void OnEnter()
        {
            
        }

        public override void OnLeave()
        {
        }
        public override bool Release(ReleaseEvent aReleaseEvent) => false;


        public override void Rescale() //TODO: This is wrong, this should rescale everything
        {
            base.Rescale();
            lock (HUDManager.UiLock)
            {
                HUDManager.Rescale();
                MarkUiDirty();
                uITarget = GraphicsManager.CreateRenderTarget(Camera.Camera.WindowSize);
                plateTarget = GraphicsManager.CreateRenderTarget(Camera.Camera.WindowSize);
            }

        }

        public override bool Scroll(ScrollEvent aScrollEvent)
        {
            return false;
        }

        public override void Update()
        {
            lock (HUDManager.UiLock)
            {
                uiHeartbeatTimer += TimeManager.SecondsSinceLastFrame;
            }
        }
        protected void UIDraw()
        {
            UiDrawList uiDrawList;
            PlateDrawList plateDrawList;
            bool redrawPlates;

            GraphicsManager.AssertScissorStackEmpty();

            lock (HUDManager.UiLock)
            {
                if (!uiDirty && !plateDirty && uiHeartbeatTimer < uiHeartbeatSeconds) return;
                uiHeartbeatTimer = 0;
                redrawPlates = plateDirty;

                HUDManager.BuildDrawLists();
                uiDrawList = HUDManager.UiDrawListSnapshot;
                plateDrawList = HUDManager.PlateDrawListSnapshot;

                if (uiDrawList == null)
                {
                    uiDirty = true;
                    return;
                }

                uiDirty = false;
                plateDirty = false;
            }

            GraphicsManager.SetRenderTarget(uITarget);
            GraphicsManager.ClearScreen(Color.Transparent);
            uIDraw.Begin(SpriteSortMode.Immediate, null, null, null, rasterizerState);

            uiDrawList.Draw(uIDraw);
            DebugManager.Draw(uIDraw);

            uIDraw.End();
            GraphicsManager.SetRenderTarget(null);

            if (redrawPlates && plateDrawList != null)
            {
                GraphicsManager.SetRenderTarget(plateTarget);
                GraphicsManager.ClearScreen(Color.Transparent);
                uIDraw.Begin(SpriteSortMode.Immediate, null, null, null, rasterizerState);
                plateDrawList.Draw(uIDraw);
                uIDraw.End();
                GraphicsManager.SetRenderTarget(null);
            }
        }

        void MarkUiDirty()
        {
            lock (HUDManager.UiLock)
            {
                uiDirty = true;
                uiHeartbeatTimer = 0;
            }
        }

        void MarkPlatesDirty()
        {
            lock (HUDManager.UiLock)
            {
                plateDirty = true;
                uiHeartbeatTimer = 0;
            }
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
