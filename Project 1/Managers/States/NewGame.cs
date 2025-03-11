using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Input;
using Project_1.UI.CharacterCreator;
using Project_1.UI.UIElements.Boxes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Managers.States
{
    internal class NewGame : State
    {
        NewGameBox newGameBox;
        public NewGame() : base() 
        {
        }
        public override StateManager.States GetStateEnum => StateManager.States.NewGame;

        public override bool Click(ClickEvent aClickEvent)
        {
            return newGameBox.ClickedOn(aClickEvent);
        }

        public override RenderTarget2D Draw()
        {
            PrepRender(Color.BlanchedAlmond, SpriteSortMode.Immediate);
            newGameBox.Draw(spriteBatch);
            CleanRender();
            return renderTarget;
        }

        public override void OnEnter()
        {
            RelativeScreenPosition size = RelativeScreenPosition.GetSquareFromY(0.9f);
            newGameBox = new NewGameBox(new RelativeScreenPosition(0.05f), size);
        }

        public override void OnLeave()
        {
            newGameBox = null;
        }

        public override void PopUp(DialogueBox aBox)
        {
            throw new NotImplementedException();

        }

        public override bool Release(ReleaseEvent aReleaseEvent)
        {
            return newGameBox.ReleasedOn(aReleaseEvent);
        }

        public override void RemovePopUp(DialogueBox aBox)
        {
            throw new NotImplementedException();
        }

        public override bool Scroll(ScrollEvent aScrollEvent)
        {
            return newGameBox.ScrolledOn(aScrollEvent);
        }

        public override void Update()
        {
            newGameBox.Update();
        }
    }
}
