using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Input;
using Project_1.UI;
using Project_1.UI.StartMenu;
using Project_1.UI.HUD.Managers;
using Project_1.UI.UIElements.Boxes;
using Project_1.UI.UIElements;
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
        readonly UiElementDrawList drawListA = new UiElementDrawList();
        readonly UiElementDrawList drawListB = new UiElementDrawList();
        volatile UiElementDrawList drawList;
        public StartScreen() : base()
        {
            mainMenu = new MainMenu();
            drawList = drawListA;
            BuildDrawList();

        }

        public override void Update()
        {
        }

        public override void Rescale()
        {
            base.Rescale();
        }

        public override bool Scroll(ScrollEvent aScrollEvent) => false;

        public override void OnEnter()
        {
            MarkUiDirty();
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
            if (!renderDirty || drawList == null) return renderTarget;
            renderDirty = false;
            PrepRender(Color.White);

            drawList?.Draw(spriteBatch);

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
        internal bool UiEscapePressed() => false;
        internal void UiUpdate()
        {
            mainMenu.Update();
        }

        void BuildDrawList()
        {
            UiElementDrawList buildTarget = ReferenceEquals(drawList, drawListA) ? drawListB : drawListA;
            buildTarget.SetSingle(mainMenu);
            drawList = buildTarget;
            MarkUiDirty();
        }

        internal void UiRescale()
        {
            if (mainMenu == null) return;
            mainMenu.Rescale();
            BuildDrawList();
        }
    }
}
