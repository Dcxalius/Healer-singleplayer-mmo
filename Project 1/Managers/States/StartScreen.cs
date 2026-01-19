using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Input;
using Project_1.UI;
using Project_1.UI.StartMenu;
using Project_1.UI.HUD.Managers;
using Project_1.UI.UIElements.Boxes;
using System;
using Project_1.Camera;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Managers.States
{
    internal class StartScreen : State
    {
        public override StateManager.States GetStateEnum => StateManager.States.StartScreen;
        MainMenu mainMenu;
        public StartScreen() : base()
        {
            mainMenu = new MainMenu();

        }

        public override void Update()
        {
        }

        public override void Rescale()
        {
            //throw new NotImplementedException();
        }

        public override bool Scroll(ScrollEvent aScrollEvent) => false;

        public override bool Click(ClickEvent aClickEvent)
        {
            return false;
        }


        public override void OnEnter()
        {
        }

        public override void OnLeave()
        {
        }


        

        public override bool Release(ReleaseEvent aReleaseEvent)
        {
            return false;
        }
        public override RenderTarget2D Draw()
        {
            PrepRender(Color.White);

            lock (HUDManager.UiLock)
            {
                mainMenu.Draw(spriteBatch);
            }

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

        internal bool UiClick(ClickEvent aClickEvent) => mainMenu.ClickedOn(aClickEvent);
        internal bool UiRelease(ReleaseEvent aReleaseEvent) => mainMenu.ReleasedOn(aReleaseEvent);
        internal bool UiScroll(ScrollEvent aScrollEvent) => mainMenu.ScrolledOn(aScrollEvent);
        internal void UiUpdate() => mainMenu.Update();
    }
}
