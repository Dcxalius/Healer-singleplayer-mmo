using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Input;
using Project_1.Textures;
using Project_1.UI.HUD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Project_1.GameObjects.Spells.AoE.AreaOfEffectData;

namespace Project_1.Managers.States
{
    internal class PausedGame : GameState
    {
        RenderTarget2D pausedTarget;
        SpriteBatch pausedGameDraw;
        Textures.Texture pauseBackground;



        public PausedGame()
        {
            pausedGameDraw = GraphicsManager.CreateSpriteBatch();
            pauseBackground = new Textures.Texture(new GfxPath(GfxType.UI, "PauseBackground"));
            pausedTarget = GraphicsManager.CreateRenderTarget(Camera.Camera.ScreenSize);

        }

        public override void Rescale()
        {
            pausedTarget = GraphicsManager.CreateRenderTarget(Camera.Camera.ScreenSize);
            base.Rescale();
        }


        public override RenderTarget2D Draw()
        {
            UIDraw();
            GraphicsManager.SetRenderTarget(pausedTarget);
            pausedGameDraw.Begin(SpriteSortMode.FrontToBack);
            GraphicsManager.ClearScreen(Color.White);

            pausedGameDraw.Draw(StateManager.FinalGameFrame, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9f); //draw game
            pauseBackground.Draw(pausedGameDraw, Vector2.Zero); //draw gray screen overlay

            pausedGameDraw.Draw(uITarget, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);


            pausedGameDraw.End();
            GraphicsManager.SetRenderTarget(null);
            return pausedTarget;
        }
    }
}
