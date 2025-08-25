using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Project_1.GameObjects;
using Project_1.GameObjects.Doodads;
using Project_1.GameObjects.Entities.Corspes;
using Project_1.GameObjects.Entities.Players;
using Project_1.GameObjects.Entities.Projectiles;
using Project_1.GameObjects.FloatingTexts;
using Project_1.GameObjects.Spawners;
using Project_1.Input;
using Project_1.Particles;
using Project_1.Tiles;
using Project_1.UI;
using Project_1.UI.HUD.Managers;
using Project_1.UI.PauseMenu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

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
            if (InputManager.GetPress(Microsoft.Xna.Framework.Input.Keys.Escape))
            {
                StateManager.SetState(StateManager.States.PauseMenu);
                return;
            }
            Camera.Camera.Update();
            ObjectManager.Update();
            TileManager.Update();
            FloatingTextManager.Update();
            CorpseManager.Update();
            DoodadManager.Update();
            SpawnerManager.Update();
            ProjectileManager.Update();
            ParticleManager.Update();
            base.Update();

            ObjectManager.RefreshPlates();
            SpawnerManager.RefreshPlates();
        }
        //public override void Rescale()
        //{
        //    base.Rescale();
        //}

        public override bool Click(ClickEvent aClickEvent)
        {
            if (base.Click(aClickEvent)) return true;
            if (ObjectManager.Click(aClickEvent)) return true;
            if (SpawnerManager.Click(aClickEvent)) return true;
            if (CorpseManager.Click(aClickEvent)) return true;
            if (DoodadManager.Click(aClickEvent)) return true;
            return ObjectManager.ClickGround(aClickEvent);
        }


        public override void OnEnter() => TimeManager.StopPause(this);

        public override void OnLeave()
        {
            StateManager.FinalGameFrame = renderTarget;
            

            TimeManager.StartPause(this);
            HUDManager.LeavingGameState();
        }

        public RenderTarget2D CleanGameDraw()
        {            
            PrepRender(Color.White, SpriteSortMode.Immediate);

            DrawList(spriteBatch);

            CleanRender();
            return renderTarget;
        }

        public override RenderTarget2D Draw()
        {
            UIDraw();
            Effect e = EffectManager.GetEffect("TestDarkness");
            PrepRender(Color.White, SpriteSortMode.FrontToBack, effect: e, blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointWrap);
            EffectParameterCollection epc = e.Parameters;
            epc["minLength"].SetValue(500f);
            epc["maxBrightness"].SetValue(200f);
            epc["cameraWorldPos"].SetValue(new Vector2(Camera.Camera.WorldRectangle.Location.X, Camera.Camera.WorldRectangle.Location.Y));
            epc["cameraSize"].SetValue(new Vector2(Camera.Camera.WorldRectangle.Size.X, Camera.Camera.WorldRectangle.Size.Y));
            Vector2[] v = new Vector2[5];
            for (int i = 0; i < v.Length; i++)
            {
                v[i] = ObjectManager.Player.Party.GetPositions[i];
            }
            epc["lightPos"].SetValue(v);
            

            DrawList(spriteBatch);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred);
            spriteBatch.Draw(uITarget, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);

            CleanRender();
            return renderTarget;
        }

        void DrawList(SpriteBatch aBatch)
        {
            TileManager.Draw(aBatch);

            ProjectileManager.Draw(aBatch);
            ObjectManager.Draw(aBatch);
            DoodadManager.Draw(aBatch);
            CorpseManager.Draw(aBatch);
            SpawnerManager.Draw(aBatch);

            ParticleManager.Draw(aBatch);
            FloatingTextManager.Draw(aBatch);
        }

        

    }
}
