using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Input;
using Project_1.UI;
using Project_1.UI.HUD.Managers;
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

        public override RenderTarget2D Draw()
        {
            PrepRender(Color.BlanchedAlmond, SpriteSortMode.Immediate);
            lock (HUDManager.UiLock)
            {
                newGameBox.Draw(spriteBatch);
            }
            CleanRender();
            return renderTarget;
        }

        public override void OnEnter()
        {
        }

        public override void OnLeave()
        {
        }

        public override void PopUp(DialogueBox aBox)
        {
            throw new NotImplementedException();

        }

        public override bool Release(ReleaseEvent aReleaseEvent)
        {
            return false;
        }

        public override void RemovePopUp(DialogueBox aBox)
        {
            throw new NotImplementedException();
        }

        public override bool Scroll(ScrollEvent aScrollEvent)
        {
            return false;
        }

        public override void Update()
        {
        }

        internal bool UiClick(ClickEvent aClickEvent) => newGameBox.ClickedOn(aClickEvent);
        internal bool UiRelease(ReleaseEvent aReleaseEvent) => newGameBox.ReleasedOn(aReleaseEvent);
        internal bool UiScroll(ScrollEvent aScrollEvent) => newGameBox.ScrolledOn(aScrollEvent);
        internal bool UiEscapePressed() => false;
        internal void UiUpdate() => newGameBox.Update();

        internal void UiOnEnter()
        {
            lock (HUDManager.UiLock)
            {
                RelativeScreenPosition size = RelativeScreenPosition.GetSquareFromY(0.9f);
                newGameBox = new NewGameBox(new RelativeScreenPosition(0.05f), size);
            }
        }

        internal void UiOnLeave()
        {
            lock (HUDManager.UiLock)
            {
                newGameBox = null;
            }
        }
    }
}
