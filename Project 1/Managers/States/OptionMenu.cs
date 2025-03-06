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
        public override StateManager.States GetStateEnum => StateManager.States.OptionMenu;
        public OptionMenu() : base()
        {
            OptionManager.Init();
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
            base.Rescale();
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
            PrepRender(Color.Pink);

            OptionManager.Draw(spriteBatch);

            CleanRender();

            return renderTarget;
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
