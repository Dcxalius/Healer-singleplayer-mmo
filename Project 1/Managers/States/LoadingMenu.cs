using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Input;
using Project_1.UI;
using Project_1.UI.HUD.Managers;
using Project_1.UI.LoadingMenu;
using Project_1.UI.UIElements.Boxes;
using Project_1.UI.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Managers.States
{
    internal class LoadingMenu : State
    {
        LoadingBox loadingBox;
        private RasterizerState rasterizerState;
        volatile UiElementDrawList drawList;

        public LoadingMenu() : base()
        {
            loadingBox = new LoadingBox(new Camera.RelativeScreenPosition(0.05f, 0.05f), new Camera.RelativeScreenPosition(0.9f, 0.9f));
            rasterizerState = new RasterizerState() { ScissorTestEnable = true };

        }

        public override StateManager.States GetStateEnum => StateManager.States.LoadingMenu;



        public override RenderTarget2D Draw()
        {
            if (!renderDirty || drawList == null) return renderTarget;
            renderDirty = false;
            PrepRender(Color.Lime, SpriteSortMode.Immediate, null, null, null, rasterizerState);
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

        #region NYI
        public override void PopUp(DialogueBox aBox) => throw new NotImplementedException();

        public override bool Release(ReleaseEvent aReleaseEvent) => false;

        public override void RemovePopUp(DialogueBox aBox) => throw new NotImplementedException();
        #endregion
        public override bool Scroll(ScrollEvent aScrollEvent) => false;

        public override void Update()
        {
        }

        internal bool UiClick(ClickEvent aClickEvent) => loadingBox.ClickedOn(aClickEvent);
        internal bool UiRelease(ReleaseEvent aReleaseEvent) => loadingBox.ReleasedOn(aReleaseEvent);
        internal bool UiScroll(ScrollEvent aScrollEvent) => loadingBox.ScrolledOn(aScrollEvent);
        internal bool UiEscapePressed() => false;
        internal void UiUpdate()
        {
            loadingBox.Update();
        }

        internal void UiOnEnter()
        {
            lock (HUDManager.UiLock)
            {
                loadingBox.Setup(SaveManager.Saves);
            }
            drawList = new UiElementDrawList(new UIElement[] { loadingBox });
            MarkUiDirty();
        }

        internal void UiOnLeave()
        {
            lock (HUDManager.UiLock)
            {
                loadingBox.Reset();
            }
            drawList = null;
            MarkUiDirty();
        }
    }
}
