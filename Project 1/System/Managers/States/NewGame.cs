using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Input;
using Project_1.UI;
using Project_1.UI.CharacterCreator;
using Project_1.UI.UIElements.Boxes;
using System;

namespace Project_1.Managers.States
{
    internal class NewGame : State //Q: This should probably be merged into StartScreen and the seperate state turned into something callen Screen perhaps? A UIElement the size of the screen, and then just swap between those UIElements instead of statejumping
    {
        NewGameBox newGameBox;
        readonly UiElementDrawList drawListA = new UiElementDrawList();
        readonly UiElementDrawList drawListB = new UiElementDrawList();
        volatile UiElementDrawList drawList;
        public NewGame() : base() 
        {
            drawList = drawListA;
        }
        public override StateManager.States GetStateEnum => StateManager.States.NewGame;

        public override RenderTarget2D Draw()
        {
            if (!renderDirty || drawList == null) return renderTarget;
            renderDirty = false;
            PrepRender(Color.BlanchedAlmond, SpriteSortMode.Immediate);
            drawList?.Draw(spriteBatch);
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
        internal void UiUpdate()
        {
            newGameBox.Update();
            if (UiTextInputManager.IsActive)
            {
                MarkUiDirty();
            }
        }

        internal void UiOnEnter()
        {
            RelativeScreenPosition size = RelativeScreenPosition.GetSquareFromY(0.9f);
            newGameBox = new NewGameBox(new RelativeScreenPosition(0.05f), size);
            UiElementDrawList buildTarget = ReferenceEquals(drawList, drawListA) ? drawListB : drawListA;
            buildTarget.SetSingle(newGameBox);
            drawList = buildTarget;
            MarkUiDirty();
        }

        internal void UiOnLeave()
        {
            newGameBox = null;
            UiElementDrawList buildTarget = ReferenceEquals(drawList, drawListA) ? drawListB : drawListA;
            buildTarget.SetSingle(null);
            drawList = buildTarget;
            MarkUiDirty();
        }

        internal void UiRescale()
        {
            newGameBox?.Rescale();
            UiElementDrawList buildTarget = ReferenceEquals(drawList, drawListA) ? drawListB : drawListA;
            buildTarget.SetSingle(newGameBox);
            drawList = buildTarget;
            MarkUiDirty();
        }
    }
}
