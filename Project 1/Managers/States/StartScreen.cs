using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Input;
using Project_1.UI.StartMenu;
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
            mainMenu.Update();
        }

        public override void Rescale()
        {
            //throw new NotImplementedException();
        }

        public override bool Scroll(ScrollEvent aScrollEvent) => false;

        public override bool Click(ClickEvent aClickEvent)
        {
            if (mainMenu.ClickedOn(aClickEvent)) return true;
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

            mainMenu.Draw(spriteBatch);

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
