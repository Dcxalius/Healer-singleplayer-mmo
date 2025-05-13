using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Input;
using Project_1.Textures;
using Project_1.UI.HUD;
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


        public MoveHUD() : base()
        {
            pauseBackground = new Textures.Texture(new GfxPath(GfxType.UI, "PauseBackground"));
            MoveHUDBox = new MoveHUDBox();
        }

        public override void OnEnter()
        {
            HUDManager.SetHudMoveable(true);
            cleanGame = StateManager.CleanGameTarget;
        }

        public override void OnLeave()
        {
            StateManager.RedrawGame(); //TODO: UGLY AF
        }

        public override void Update()
        {
            MoveHUDBox.Update();
            HUDManager.HudMovableUpdate();

        }

        public override void PopUp(DialogueBox aBox)
        {
        }

        public override void RemovePopUp(DialogueBox aBox)
        {
            
        }

        public override bool Release(ReleaseEvent aReleaseEvent)
        {
            throw new NotImplementedException();
        }

        public override bool Scroll(ScrollEvent aScrollEvent)
        {
            throw new NotImplementedException();
        }

        public override void Rescale()
        {
            renderTarget = GraphicsManager.CreateRenderTarget(Camera.Camera.WindowSize);
            base.Rescale();
        }

        public override bool Click(ClickEvent aClickEvent)
        {
            if (MoveHUDBox.ClickedOn(aClickEvent)) return true;

            return HUDManager.Click(aClickEvent);
        }

        public override RenderTarget2D Draw()
        {
            PrepRender(Color.White, SpriteSortMode.Immediate);

            spriteBatch.Draw(cleanGame, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9f); //draw game
            pauseBackground.Draw(spriteBatch, Vector2.Zero); //draw gray screen overlay
            HUDManager.HudMoveableDraw(spriteBatch);
            MoveHUDBox.Draw(spriteBatch);

            CleanRender();
            return renderTarget;
        }

    }
}
