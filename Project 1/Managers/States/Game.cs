using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Project_1.GameObjects;
using Project_1.GameObjects.Entities.Corspes;
using Project_1.GameObjects.Entities.Friendlies.Players;
using Project_1.GameObjects.Entities.Projectiles;
using Project_1.GameObjects.FloatingTexts;
using Project_1.GameObjects.Spawners;
using Project_1.Particles;
using Project_1.Tiles;
using Project_1.UI;
using Project_1.UI.HUD.Managers;
using Project_1.UI.PauseMenu;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Project_1.Input;
using Project_1.Camera;
using System.Diagnostics;

namespace Project_1.Managers.States
{
    internal class Game : GameState
    {
        public override StateManager.States GetStateEnum => StateManager.States.Game;

        public Game() : base()
        {
            spriteBatch = GraphicsManager.CreateSpriteBatch();

            renderTarget = GraphicsManager.CreateRenderTarget(Camera.Camera.WindowSize);
        }

        public override void Update()
        {
            ThreadAffinity.AssertSimThread();
            Camera.Camera.Update();
            ObjectManager.Update();
            TileManager.Update();
            CorpseManager.Update();
            SpawnerManager.Update();
            ProjectileManager.Update();
            base.Update();

            ObjectManager.RefreshPlates();
            SpawnerManager.RefreshPlates();
            RenderSnapshotManager.BuildGameSnapshots();
        }
        //public override void Rescale()
        //{
        //    base.Rescale();
        //}

        public override void OnEnter() => TimeManager.StopPause(this);

        public override void OnLeave()
        {
            StateManager.FinalGameFrame = renderTarget;
            

            TimeManager.StartPause(this);
        }

        internal void UiOnLeave()
        {
            HUDManager.LeavingGameState();
        }

        public RenderTarget2D CleanGameDraw()
        {
            long frameStartTicks = Stopwatch.GetTimestamp();
            UpdateVfx();
            PrepRender(Color.White, SpriteSortMode.Immediate, samplerState: SamplerState.PointClamp); //TODO: Should this be immediate?

            long worldStartTicks = Stopwatch.GetTimestamp();
            DrawList(spriteBatch);
            long worldDrawTicks = Stopwatch.GetTimestamp() - worldStartTicks;

            spriteBatch.End();
            SuperSoftShadowRenderer.DrawAndComposite(renderTarget);
            spriteBatch.Begin(SpriteSortMode.Immediate, samplerState: SamplerState.PointClamp);

            CleanRender();
            long totalTicks = Stopwatch.GetTimestamp() - frameStartTicks;
            MainRenderTelemetry.RecordFrame(totalTicks, 0, worldDrawTicks, 0);
            return renderTarget;
        }

        public override RenderTarget2D Draw()
        {
            long frameStartTicks = Stopwatch.GetTimestamp();
            UpdateVfx();
            long uiBuildStartTicks = Stopwatch.GetTimestamp();
            UIDraw();
            long uiBuildTicks = Stopwatch.GetTimestamp() - uiBuildStartTicks;
            PrepRender(Color.White, SpriteSortMode.FrontToBack, samplerState: SamplerState.PointClamp);
            long worldStartTicks = Stopwatch.GetTimestamp();
            DrawList(spriteBatch);
            long worldDrawTicks = Stopwatch.GetTimestamp() - worldStartTicks;
            long compositeStartTicks = Stopwatch.GetTimestamp();
            spriteBatch.End();
            SuperSoftShadowRenderer.DrawAndComposite(renderTarget);
            spriteBatch.Begin(SpriteSortMode.Deferred);
            StateManager.DrawGroundSpellEffects(spriteBatch);
            StateManager.DrawGroundTargetPreview(spriteBatch);
            spriteBatch.Draw(plateTarget, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            spriteBatch.Draw(uITarget, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            long uiCompositeTicks = Stopwatch.GetTimestamp() - compositeStartTicks;

            CleanRender();
            long totalTicks = Stopwatch.GetTimestamp() - frameStartTicks;
            MainRenderTelemetry.RecordFrame(totalTicks, uiBuildTicks, worldDrawTicks, uiCompositeTicks);
            return renderTarget;
        }

        void UpdateVfx()
        {
            ThreadAffinity.AssertMainThread();
            ParticleManager.Update();
            FloatingTextManager.Update();
        }

        void DrawList(SpriteBatch aBatch)
        {
            ThreadAffinity.AssertMainThread();
            RenderSnapshotManager.DrawGameSnapshots(aBatch);

            ParticleManager.Draw(aBatch);
            FloatingTextManager.Draw(aBatch);
        }

    }
}
