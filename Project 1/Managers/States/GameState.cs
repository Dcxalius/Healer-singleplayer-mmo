using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Input;
using Project_1.UI.HUD;
using Project_1.UI.UIElements.Boxes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Managers.States
{
    internal class GameState : State
    {
        protected SpriteBatch uIDraw;
        protected RenderTarget2D uITarget;
        RasterizerState rasterizerState = new RasterizerState() { ScissorTestEnable = true };


        public GameState()
        {
            uIDraw = GraphicsManager.CreateSpriteBatch();
            uITarget = GraphicsManager.CreateRenderTarget(Camera.Camera.ScreenSize);

        }
        public override bool Click(ClickEvent aClickEvent)
        {
            return HUDManager.Click(aClickEvent);
        }


        public override void OnEnter()
        {
            
        }

        public override void OnLeave()
        {
        }
        public override bool Release(ReleaseEvent aReleaseEvent) => HUDManager.Release(aReleaseEvent);


        public override void Rescale()
        {
            uITarget = GraphicsManager.CreateRenderTarget(Camera.Camera.ScreenSize);
            HUDManager.Rescale();

        }

        public override bool Scroll(ScrollEvent aScrollEvent)
        {
            return HUDManager.Scroll(aScrollEvent);
        }

        public override void Update()
        {
            HUDManager.Update();

        }
        protected void UIDraw()
        {
            GraphicsManager.SetRenderTarget(uITarget);
            uIDraw.Begin(SpriteSortMode.Immediate, null, null, null, rasterizerState);

            GraphicsManager.ClearScreen(Color.Transparent);
            HUDManager.Draw(uIDraw);
            DebugManager.Draw(uIDraw);

            uIDraw.End();
            GraphicsManager.SetRenderTarget(null);
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
