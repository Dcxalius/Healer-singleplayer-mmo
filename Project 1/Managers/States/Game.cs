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
            UpdateVfx();
            PrepRender(Color.White, SpriteSortMode.Immediate);

            DrawList(spriteBatch);

            CleanRender();
            return renderTarget;
        }

        public override RenderTarget2D Draw()
        {
            UpdateVfx();
            UIDraw();
            Effect e = EffectManager.GetEffect("TestDarkness");
            PrepRender(Color.White, SpriteSortMode.FrontToBack, effect: e);
            EffectParameterCollection epc = e.Parameters;
            epc["minLength"].SetValue(500f);
            epc["maxBrightness"].SetValue(200f);
            Vector2 cameraTopLeft = Camera.Camera.CentreInWorldSpace.ToVector2()
                - Camera.Camera.CentrePointInScreenSpace.ToVector2() / Camera.Camera.Scale;
            epc["cameraWorldPos"].SetValue(cameraTopLeft);
            EffectParameter mapOriginParam = epc["transparentMapOriginTile"];
            if (mapOriginParam != null)
            {
                mapOriginParam.SetValue(TileRenderCache.GetTransparencyOriginTile());
            }
            EffectParameter cameraScaleParam = epc["cameraScale"];
            if (cameraScaleParam != null)
            {
                cameraScaleParam.SetValue(Camera.Camera.Scale);
            }
            EffectParameter cameraSizeParam = epc["cameraSize"];
            if (cameraSizeParam != null)
            {
                cameraSizeParam.SetValue(new Vector2(Camera.Camera.WorldRectangle.Size.X, Camera.Camera.WorldRectangle.Size.Y));
            }
            ObjectManager.PartyLightSnapshot lightSnapshot = ObjectManager.RenderLightSnapshot;
            WorldSpace[] partyPositions = lightSnapshot.Positions;
            Vector2[] v = new Vector2[5];
            for (int i = 0; i < v.Length; i++)
            {
                v[i] = i < partyPositions.Length ? partyPositions[i] : Vector2.Zero;
            }
            //epc["tileTransparent"].SetValue(TileManager.GetTransparent(ObjectManager.Player.FeetPosition));
            epc["lightPos"].SetValue(v);
            epc["transparentMap"].SetValue(TileRenderCache.GetTransparencyMapTexture());
            //GraphicsManager.SetTexture(1, TileManager.GetTransparent(ObjectManager.Player.FeetPosition));
            DrawList(spriteBatch);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred);
            spriteBatch.Draw(plateTarget, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            spriteBatch.Draw(uITarget, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);

            CleanRender();
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
