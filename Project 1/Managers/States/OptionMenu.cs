using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Input;
using Project_1.UI;
using Project_1.UI.HUD;
using Project_1.UI.OptionMenu;
using Project_1.UI.UIElements.Boxes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Managers.States
{
    internal class OptionMenu : State
    {
        RenderTarget2D optionDraw;
        SpriteBatch optionBatch;
        public OptionMenu()
        {
            OptionManager.Init();
            optionBatch = GraphicsManager.CreateSpriteBatch();
            optionDraw = GraphicsManager.CreateRenderTarget(Camera.Camera.ScreenSize);
        }

        public override void Update() => OptionManager.Update();

        public override bool Click(ClickEvent aClickEvent) => OptionManager.Click(aClickEvent);

        public override bool Release(ReleaseEvent aReleaseEvent)
        {
            throw new NotImplementedException();
        }
        public override bool Scroll(ScrollEvent aScrollEvent)
        {
            throw new NotImplementedException();
        }

        public override void Rescale()
        {
            optionDraw = GraphicsManager.CreateRenderTarget(Camera.Camera.ScreenSize);
            OptionManager.Rescale();
        }

        public override void OnEnter()
        {
        }

        public override void OnLeave()
        {
        }
        public override RenderTarget2D Draw()
        {
            GraphicsManager.ClearScreen(Color.LightGray);
            GraphicsManager.SetRenderTarget(optionDraw);
            optionBatch.Begin();

            OptionManager.Draw(optionBatch);

            optionBatch.End();
            GraphicsManager.SetRenderTarget(null);

            return optionDraw;
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
