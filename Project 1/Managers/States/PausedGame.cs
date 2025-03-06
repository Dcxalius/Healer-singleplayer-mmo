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
        public override StateManager.States GetStateEnum => StateManager.States.PausedGame;
        Textures.Texture pauseBackground;


        public PausedGame() : base()
        {
            pauseBackground = new Textures.Texture(new GfxPath(GfxType.UI, "PauseBackground"));

        }

        public override void Rescale()
        {
            renderTarget = GraphicsManager.CreateRenderTarget(Camera.Camera.ScreenSize);
            base.Rescale();
        }


        public override RenderTarget2D Draw()
        {
            UIDraw();
            GraphicsManager.ClearScreen(Color.White);

            spriteBatch.Begin(SpriteSortMode.FrontToBack);

            spriteBatch.Draw(StateManager.FinalGameFrame, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9f); //draw game
            pauseBackground.Draw(spriteBatch, Vector2.Zero); //draw gray screen overlay

            spriteBatch.Draw(uITarget, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);


            spriteBatch.End();
            GraphicsManager.SetRenderTarget(null);
            return renderTarget;
        }
    }
}
