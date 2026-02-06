using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Input;
using Project_1.UI;
using Project_1.UI.HUD;
using Project_1.UI.HUD.Managers;
using Project_1.UI.OptionMenu;
using Project_1.UI.UIElements.Boxes;
using Project_1.UI.UIElements;
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
        volatile UiElementDrawList drawList;
        public OptionMenu() : base()
        {

        }

        public override void Update()
        {
        }

        public override bool Release(ReleaseEvent aReleaseEvent) => false;
        public override bool Scroll(ScrollEvent aScrollEvent) => false;

        public override void Rescale()
        {
            base.Rescale();
        }

        public override void OnEnter()
        {
        }

        public override void OnLeave()
        {
        }
        public override RenderTarget2D Draw()
        {
            if (!renderDirty || drawList == null) return renderTarget;
            renderDirty = false;
            PrepRender(Color.Pink, sortMode: SpriteSortMode.Immediate, rasterizerState: new RasterizerState() { ScissorTestEnable = true });

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

        internal bool UiClick(ClickEvent aClickEvent) => OptionManager.Click(aClickEvent);
        internal bool UiRelease(ReleaseEvent aReleaseEvent) => false;
        internal bool UiScroll(ScrollEvent aScrollEvent) => OptionManager.Scroll(aScrollEvent);
        internal bool UiEscapePressed() => false;
        internal void UiUpdate()
        {
            OptionManager.Update();
            if (drawList == null || OptionManager.ConsumeDrawListDirty())
            {
                drawList = new UiElementDrawList(OptionManager.BuildDrawList());
                MarkUiDirty();
            }
            if (OptionManager.ConsumeRenderDirty() || UiTextInputManager.IsActive)
            {
                MarkUiDirty();
            }
        }

        internal void UiOnEnter()
        {
            drawList = new UiElementDrawList(OptionManager.BuildDrawList());
            OptionManager.ConsumeDrawListDirty();
            OptionManager.ConsumeRenderDirty();
            MarkUiDirty();
        }

        internal void UiOnLeave()
        {
            OptionManager.ClearButtons();
            drawList = null;
            MarkUiDirty();
        }

        internal void UiRescale()
        {
            OptionManager.Rescale();
            drawList = new UiElementDrawList(OptionManager.BuildDrawList());
            OptionManager.ConsumeDrawListDirty();
            OptionManager.ConsumeRenderDirty();
            MarkUiDirty();
        }
    }
}
