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
    internal class Game : GameState
    {
        SpriteBatch gameDraw;
        RenderTarget2D gameTarget;



        public Game() : base()
        {
            HUDManager.Init();
            ObjectManager.Init();

            gameDraw = GraphicsManager.CreateSpriteBatch();

            gameTarget = GraphicsManager.CreateRenderTarget(Camera.Camera.ScreenSize);
        }

        public override void Update()
        {
            if (InputManager.GetPress(Microsoft.Xna.Framework.Input.Keys.Escape))
            {
                StateManager.SetState(StateManager.States.PauseMenu);
            }
            Camera.Camera.Update();
            ObjectManager.Update();
            SpawnerManager.Update();
            ParticleManager.Update();
            base.Update();
        }
        public override void Rescale()
        {
            gameTarget = GraphicsManager.CreateRenderTarget(Camera.Camera.ScreenSize);
            base.Rescale();
        }

        public override bool Click(ClickEvent aClickEvent)
        {
            if (base.Click(aClickEvent)) return true;

            return ObjectManager.Click(aClickEvent);
        }


        public override void OnEnter() => TimeManager.StopPause();

        public override void OnLeave()
        {
            StateManager.FinalGameFrame = gameTarget;
            TimeManager.StartPause();
            HUDManager.LeavingGameState();
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

        

    }
}
