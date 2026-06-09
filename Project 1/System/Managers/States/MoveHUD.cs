using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Input;
using Project_1.Textures;
using Project_1.UI;
using Project_1.UI.HUD;
using Project_1.UI.HUD.Managers;
using Project_1.UI.UIElements.Boxes;
using System;

namespace Project_1.Managers.States
{
    internal class MoveHUD : State
    {
        public override StateManager.States GetStateEnum => StateManager.States.MoveHUD;
        Textures.Texture pauseBackground;
        MoveHUDBox MoveHUDBox;
        RenderTarget2D cleanGame;
        bool needsCleanGame;
        readonly UiElementDrawList moveHudDrawListA = new UiElementDrawList();
        readonly UiElementDrawList moveHudDrawListB = new UiElementDrawList();
        volatile UiElementDrawList moveHudDrawList;


        public MoveHUD() : base()
        {
            pauseBackground = new Textures.Texture(new GfxPath(GfxType.UI, "PauseBackground"));
            MoveHUDBox = new MoveHUDBox(null);
            moveHudDrawList = moveHudDrawListA;
        }

        public override void OnEnter()
        {
        }

        public override void OnLeave()
        {
        }

        public override void Update()
        {
        }

        public override void PopUp(DialogueBox aBox)
        {
        }

        public override void RemovePopUp(DialogueBox aBox)
        {
            
        }

        public override bool Release(ReleaseEvent aReleaseEvent)
        {
            return false;
        }

        public override bool Scroll(ScrollEvent aScrollEvent)
        {
            return false;
        }

        public override void Rescale()
        {
            base.Rescale();
            needsCleanGame = true;
        }

        public override RenderTarget2D Draw()
        {
            if (!renderDirty && !needsCleanGame) return renderTarget;
            renderDirty = false;
            PrepRender(Color.White, SpriteSortMode.Immediate);
            if (cleanGame == null || needsCleanGame)
            {
                cleanGame = StateManager.CleanGameTarget;
                needsCleanGame = false;
            }

            spriteBatch.Draw(cleanGame, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9f); //draw game
            pauseBackground.Draw(spriteBatch, Vector2.Zero); //draw gray screen overlay
            HUDManager.HudMoveDrawListSnapshot?.Draw(spriteBatch);
            moveHudDrawList?.Draw(spriteBatch);

            CleanRender();
            return renderTarget;
        }

        internal bool UiClick(ClickEvent aClickEvent) => MoveHUDBox.ClickedOn(aClickEvent);
        internal bool UiRelease(ReleaseEvent aReleaseEvent) => MoveHUDBox.ReleasedOn(aReleaseEvent);
        internal bool UiScroll(ScrollEvent aScrollEvent) => MoveHUDBox.ScrolledOn(aScrollEvent);
        internal bool UiEscapePressed() => false;
        internal void UiUpdate()
        {
            MoveHUDBox.Update();
            HUDManager.HudMovableUpdate();
            UiElementDrawList buildTarget = ReferenceEquals(moveHudDrawList, moveHudDrawListA) ? moveHudDrawListB : moveHudDrawListA;
            buildTarget.SetSingle(MoveHUDBox);
            moveHudDrawList = buildTarget;
            MarkUiDirty();
        }

        internal void UiOnEnter()
        {
            needsCleanGame = true;
            HUDManager.SetHudMoveable(true);
            HUDManager.InvalidateUi();
            UiElementDrawList buildTarget = ReferenceEquals(moveHudDrawList, moveHudDrawListA) ? moveHudDrawListB : moveHudDrawListA;
            buildTarget.SetSingle(MoveHUDBox);
            moveHudDrawList = buildTarget;
            MarkUiDirty();
        }

        internal void UiOnLeave()
        {
            StateManager.RedrawGame();
            HUDManager.SetHudMoveable(false);
            HUDManager.InvalidateUi();
            UiElementDrawList buildTarget = ReferenceEquals(moveHudDrawList, moveHudDrawListA) ? moveHudDrawListB : moveHudDrawListA;
            buildTarget.SetSingle(null);
            moveHudDrawList = buildTarget;
            MarkUiDirty();
        }

        internal void UiRescale()
        {
            MoveHUDBox?.Rescale();
            UiElementDrawList buildTarget = ReferenceEquals(moveHudDrawList, moveHudDrawListA) ? moveHudDrawListB : moveHudDrawListA;
            buildTarget.SetSingle(MoveHUDBox);
            moveHudDrawList = buildTarget;
            MarkUiDirty();
        }

    }
}
