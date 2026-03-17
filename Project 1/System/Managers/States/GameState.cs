using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Input;
using Project_1.Managers;
using Project_1.UI;
using Project_1.UI.HUD.Managers;
using Project_1.UI.UIElements.Boxes;
using System;
using System.Threading;

namespace Project_1.Managers.States
{
    internal abstract class GameState : State
    {
        protected SpriteBatch uIDraw;
        protected RenderTarget2D uITarget;
        protected RenderTarget2D plateTarget;
        RasterizerState rasterizerState;
        volatile bool uiDirty = true;
        volatile bool plateDirty = true;
        long uiHeartbeatTicks;
        static readonly long uiHeartbeatThresholdTicks = TimeSpan.TicksPerSecond;



        public GameState() : base() 
        {
            ThreadAffinity.AssertMainThread();
            rasterizerState = new RasterizerState() { ScissorTestEnable = true };
            uIDraw = GraphicsManager.CreateSpriteBatch();
            uITarget = GraphicsManager.CreateRenderTarget(Camera.Camera.WindowSize);
            plateTarget = GraphicsManager.CreateRenderTarget(Camera.Camera.WindowSize);
            HUDManager.UiInvalidated += MarkUiDirty;
            HUDManager.PlatesInvalidated += MarkPlatesDirty;
        }
        public override void OnEnter()
        {
            
        }

        public override void OnLeave()
        {
        }
        public override bool Release(ReleaseEvent aReleaseEvent) => false;


        public override void Rescale()
        {
            ThreadAffinity.AssertMainThread();
            base.Rescale();
            uITarget = GraphicsManager.CreateRenderTarget(Camera.Camera.WindowSize);
            plateTarget = GraphicsManager.CreateRenderTarget(Camera.Camera.WindowSize);
            MarkUiDirty();
            MarkPlatesDirty();

        }

        public override bool Scroll(ScrollEvent aScrollEvent)
        {
            return false;
        }

        public override void Update()
        {
            ThreadAffinity.AssertSimThread();
            long deltaTicks = (long)(TimeManager.SecondsSinceLastFrame * TimeSpan.TicksPerSecond);
            Interlocked.Add(ref uiHeartbeatTicks, deltaTicks);
        }
        protected void UIDraw()
        {
            ThreadAffinity.AssertMainThread();
            UiDrawList uiDrawList;
            PlateDrawList plateDrawList;
            bool redrawPlates;

            GraphicsManager.AssertScissorStackEmpty();

            lock (HUDManager.UiLock)
            {
                if (!uiDirty && !plateDirty && Interlocked.Read(ref uiHeartbeatTicks) < uiHeartbeatThresholdTicks) return;
                Interlocked.Exchange(ref uiHeartbeatTicks, 0);
                redrawPlates = plateDirty;

                if (!UiThread.IsRunning)
                {
                    // Single-thread fallback: no UI worker owns draw-list generation.
                    HUDManager.BuildDrawLists();
                }
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

        internal override void MarkUiDirty()
        {
            AssertUiInvalidationThread();
            uiDirty = true;
            Interlocked.Exchange(ref uiHeartbeatTicks, 0);
        }

        void MarkPlatesDirty()
        {
            AssertUiInvalidationThread();
            plateDirty = true;
            Interlocked.Exchange(ref uiHeartbeatTicks, 0);
        }

        static void AssertUiInvalidationThread()
        {
            if (ThreadAffinity.IsSimThread)
            {
                throw new InvalidOperationException("UI/plate invalidation cannot run on the simulation thread.");
            }

            if (ThreadAffinity.IsMainThread || ThreadAffinity.IsUiThread)
            {
                return;
            }

            throw new InvalidOperationException("UI/plate invalidation must run on the main or UI thread.");
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
