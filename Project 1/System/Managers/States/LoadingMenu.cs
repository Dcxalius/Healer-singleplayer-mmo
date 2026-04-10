using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Input;
using Project_1.Messaging.Events;
using Project_1.UI;
using Project_1.UI.LoadingMenu;
using Project_1.UI.UIElements.Boxes;
using System;

namespace Project_1.Managers.States
{
    internal class LoadingMenu : State
    {
        readonly LoadingBox loadingBox;
        readonly RasterizerState rasterizerState;
        readonly UiElementDrawList drawListA = new UiElementDrawList();
        readonly UiElementDrawList drawListB = new UiElementDrawList();
        volatile UiElementDrawList drawList;

        public LoadingMenu() : base()
        {
            loadingBox = new LoadingBox(new Camera.RelativeScreenPosition(0.05f, 0.05f), new Camera.RelativeScreenPosition(0.9f, 0.9f));
            rasterizerState = new RasterizerState { ScissorTestEnable = true };
            drawList = drawListA;
        }

        public override StateManager.States GetStateEnum => StateManager.States.LoadingMenu;

        public override RenderTarget2D Draw()
        {
            if (!renderDirty || drawList == null) return renderTarget;
            renderDirty = false;
            PrepRender(Color.Lime, SpriteSortMode.Immediate, null, null, null, rasterizerState);
            drawList.Draw(spriteBatch);
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
        public override void RemovePopUp(DialogueBox aBox) => throw new NotImplementedException();
        #endregion

        public override bool Release(ReleaseEvent aReleaseEvent) => false;
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
            var saves = SaveManager.Saves;
            SaveUiSnapshot[] snapshots = new SaveUiSnapshot[saves.Length];
            for (int i = 0; i < saves.Length; i++)
            {
                snapshots[i] = new SaveUiSnapshot(
                    saves[i].Name,
                    saves[i].SaveDetails?.Stringify ?? string.Empty,
                    saves[i].ImagePath);
            }

            loadingBox.Setup(snapshots);
            UiElementDrawList buildTarget = ReferenceEquals(drawList, drawListA) ? drawListB : drawListA;
            buildTarget.SetSingle(loadingBox);
            drawList = buildTarget;
            MarkUiDirty();
        }

        internal void UiOnLeave()
        {
            loadingBox.Reset();
            UiElementDrawList buildTarget = ReferenceEquals(drawList, drawListA) ? drawListB : drawListA;
            buildTarget.SetSingle(null);
            drawList = buildTarget;
            MarkUiDirty();
        }

        internal void UiRescale()
        {
            loadingBox.Rescale();
            UiElementDrawList buildTarget = ReferenceEquals(drawList, drawListA) ? drawListB : drawListA;
            buildTarget.SetSingle(loadingBox);
            drawList = buildTarget;
            MarkUiDirty();
        }
    }
}
