using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Project_1.GameObjects;
using Project_1.GameObjects.Doodads;
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
            DoodadManager.Update();
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
            DrawShadowOverlay(spriteBatch);
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

        static void DrawShadowOverlay(SpriteBatch batch)
        {
            ThreadAffinity.AssertMainThread();
            Texture2D shadowTexture = TileRenderCache.GetShadowMapTexture();
            if (shadowTexture == null) return;

            Vector2 originTile = TileRenderCache.GetShadowOriginTile();
            int shadowSize = shadowTexture.Width;
            int halfMap = shadowSize / 2;
            int minTileX = (int)originTile.X - halfMap;
            int minTileY = (int)originTile.Y - halfMap;

            WorldSpace worldTopLeft = new WorldSpace(minTileX * Tile.Size.X, minTileY * Tile.Size.Y);
            AbsoluteScreenPosition screenTopLeft = worldTopLeft.ToAbsoltueScreenPosition();
            Point scaledSize = new Point(
                Math.Max(1, (int)MathF.Round(shadowSize * Tile.Size.X * Camera.Camera.Scale)),
                Math.Max(1, (int)MathF.Round(shadowSize * Tile.Size.Y * Camera.Camera.Scale)));
            Rectangle destination = new Rectangle(screenTopLeft, scaledSize);
            if (!Camera.Camera.ScreenspaceBoundsCheck(destination)) return;

            Effect shadowBlend = EffectManager.GetEffect("ShadowTileBlend");
            EffectParameterCollection parameters = shadowBlend?.Parameters;
            parameters?["shadowTextureSize"]?.SetValue(new Vector2(shadowTexture.Width, shadowTexture.Height));
            parameters?["tilePixelSize"]?.SetValue(Math.Max(1f, Tile.Size.X * Camera.Camera.Scale));

            batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, effect: shadowBlend);
            batch.Draw(shadowTexture, destination, Color.White);
            batch.End();
        }

    }
}
