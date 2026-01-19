using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Input;
using Project_1.Textures;
using Project_1.UI;
using Project_1.UI.HUD;
using Project_1.UI.HUD.Managers;
using Project_1.UI.UIElements.Boxes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Project_1.GameObjects.Spells.AoE.AreaOfEffectData;

namespace Project_1.Managers.States
{
    internal class MoveHUD : State
    {
        public override StateManager.States GetStateEnum => StateManager.States.MoveHUD;
        Textures.Texture pauseBackground;
        MoveHUDBox MoveHUDBox;
        RenderTarget2D cleanGame;
        bool needsCleanGame;


        public MoveHUD() : base()
        {
            pauseBackground = new Textures.Texture(new GfxPath(GfxType.UI, "PauseBackground"));
            MoveHUDBox = new MoveHUDBox();
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
            renderTarget = GraphicsManager.CreateRenderTarget(Camera.Camera.WindowSize);
            base.Rescale();
        }

        public override bool Click(ClickEvent aClickEvent)
        {
            return false;
        }

        public override RenderTarget2D Draw()
        {
            PrepRender(Color.White, SpriteSortMode.Immediate);
            if (cleanGame == null || needsCleanGame)
            {
                cleanGame = StateManager.CleanGameTarget;
                needsCleanGame = false;
            }

            spriteBatch.Draw(cleanGame, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9f); //draw game
            pauseBackground.Draw(spriteBatch, Vector2.Zero); //draw gray screen overlay
            lock (HUDManager.UiLock)
            {
                HUDManager.HudMoveableDraw(spriteBatch);
                MoveHUDBox.Draw(spriteBatch);
            }

            CleanRender();
            return renderTarget;
        }

        internal bool UiClick(ClickEvent aClickEvent) => MoveHUDBox.ClickedOn(aClickEvent);
        internal bool UiRelease(ReleaseEvent aReleaseEvent) => MoveHUDBox.ReleasedOn(aReleaseEvent);
        internal bool UiScroll(ScrollEvent aScrollEvent) => MoveHUDBox.ScrolledOn(aScrollEvent);
        internal void UiUpdate()
        {
            MoveHUDBox.Update();
            HUDManager.HudMovableUpdate();
        }

        internal void UiOnEnter()
        {
            lock (HUDManager.UiLock)
            {
                needsCleanGame = true;
                HUDManager.SetHudMoveable(true);
                HUDManager.InvalidateUi();
            }
        }

        internal void UiOnLeave()
        {
            lock (HUDManager.UiLock)
            {
                StateManager.RedrawGame();
                HUDManager.ResetHudMoveable();
                HUDManager.InvalidateUi();
            }
        }

    }
}
