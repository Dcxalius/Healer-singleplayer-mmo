using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Input;
using Project_1.Textures;
using Project_1.UI;
using Project_1.UI.OptionMenu;
using Project_1.UI.PauseMenu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Managers.States
{
    internal class Pause : State
    {
        static PauseBox pauseBox;
        RenderTarget2D pauseDraw;
        SpriteBatch pauseBatch;
        Textures.Texture pauseBackground;

        public Pause()
        {
            pauseDraw = GraphicsManager.CreateRenderTarget(Camera.Camera.ScreenSize);
            pauseBatch = GraphicsManager.CreateSpriteBatch();
            pauseBox = new PauseBox();
            pauseBackground = new Textures.Texture(new GfxPath(GfxType.UI, "PauseBackground"));
        }

        public override void Update()
        {
            if (InputManager.GetPress(Microsoft.Xna.Framework.Input.Keys.Escape))
            {
                StateManager.SetState(StateManager.States.Game);
            }
            pauseBox.Update(null);
        }
        public override void Rescale()
        {
            pauseDraw = GraphicsManager.CreateRenderTarget(Camera.Camera.ScreenSize);
            pauseBox.Rescale();

        }

        public override bool Release(ReleaseEvent aReleaseEvent)
        {
            throw new NotImplementedException();
        }

        public override bool Scroll(ScrollEvent aScrollEvent)
        {
            throw new NotImplementedException();
        }

        public override bool Click(ClickEvent aClickEvent) => pauseBox.ClickedOn(aClickEvent);

        public override void OnEnter()
        {
            
        }

        public override void OnLeave()
        {
            
        }

        public override RenderTarget2D Draw()
        {
            GraphicsManager.SetRenderTarget(pauseDraw);
            pauseBatch.Begin();
            GraphicsManager.ClearScreen(Color.Purple);


            pauseBatch.Draw(StateManager.FinalGameFrame, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9f); //draw game
            pauseBackground.Draw(pauseBatch, Vector2.Zero); //draw gray screen overlay
            pauseBox.Draw(pauseBatch, 1); //draw pause menu


            pauseBatch.End();
            GraphicsManager.SetRenderTarget(null);
            return pauseDraw;
        }
    }
}
