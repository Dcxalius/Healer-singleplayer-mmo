using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Project_1.GameObjects;
using Project_1.GameObjects.Spawners;
using Project_1.Input;
using Project_1.Particles;
using Project_1.Tiles;
using Project_1.UI;
using Project_1.UI.HUD;
using Project_1.UI.PauseMenu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using static Project_1.GameObjects.Spells.AoE.AreaOfEffectData;

namespace Project_1.Managers.States
{
    internal class Game : State
    {
        SpriteBatch gameDraw;
        SpriteBatch uIDraw;

        RenderTarget2D gameTarget;
        RenderTarget2D uITarget;

        RasterizerState rasterizerState = new RasterizerState() { ScissorTestEnable = true };

        public Game()
        {
            HUDManager.Init();
            ObjectManager.Init();

            gameDraw = GraphicsManager.CreateSpriteBatch();
            uIDraw = GraphicsManager.CreateSpriteBatch();

            gameTarget = GraphicsManager.CreateRenderTarget(Camera.Camera.ScreenSize);
            uITarget = GraphicsManager.CreateRenderTarget(Camera.Camera.ScreenSize);
        }

        public override void Update()
        {
            if (InputManager.GetPress(Microsoft.Xna.Framework.Input.Keys.Escape))
            {
                StateManager.SetState(StateManager.States.Pause);
            }
            Camera.Camera.Update();
            ObjectManager.Update();
            SpawnerManager.Update();
            HUDManager.Update();
            ParticleManager.Update();
        }
        public override void Rescale()
        {
            gameTarget = GraphicsManager.CreateRenderTarget(Camera.Camera.ScreenSize);
            uITarget = GraphicsManager.CreateRenderTarget(Camera.Camera.ScreenSize);
            HUDManager.Rescale();
        }

        public override bool Click(ClickEvent aClickEvent)
        {
            if (HUDManager.Click(aClickEvent)) return true;

            return ObjectManager.Click(aClickEvent);
        }

        public override bool Release(ReleaseEvent aReleaseEvent) => HUDManager.Release(aReleaseEvent);

        public override void OnEnter() => TimeManager.StopPause();

        public override void OnLeave()
        {
            StateManager.FinalGameFrame = gameTarget;
            TimeManager.StartPause();
            HUDManager.PauseMenuActivated();
        }

        public override RenderTarget2D Draw()
        {
            UIDraw();
            GraphicsManager.SetRenderTarget(gameTarget);
            gameDraw.Begin(SpriteSortMode.FrontToBack);
            GraphicsManager.ClearScreen(Color.White);


            DrawList(gameDraw);
            gameDraw.Draw(uITarget, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);


            gameDraw.End();
            GraphicsManager.SetRenderTarget(null);
            return gameTarget;
        }

        void DrawList(SpriteBatch aBatch)
        {

            TileManager.Draw(aBatch);
            ObjectManager.Draw(aBatch);
            SpawnerManager.Draw(aBatch);
            ParticleManager.Draw(aBatch);
        }

        void UIDraw()
        {
            GraphicsManager.SetRenderTarget(uITarget);
            uIDraw.Begin(SpriteSortMode.Immediate, null, null, null, rasterizerState);
            
            GraphicsManager.ClearScreen(Color.Transparent);
            HUDManager.Draw(uIDraw);
            DebugManager.Draw(uIDraw);

            uIDraw.End();
            GraphicsManager.SetRenderTarget(null);
        }

        public override bool Scroll(ScrollEvent aScrollEvent)
        {
            return HUDManager.Scroll(aScrollEvent);

        }
    }
}
