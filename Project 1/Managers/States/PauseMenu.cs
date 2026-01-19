using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Input;
using Project_1.Textures;
using Project_1.UI;
using Project_1.UI.HUD.Managers;
using Project_1.UI.OptionMenu;
using Project_1.UI.PauseMenu;
using Project_1.UI.UIElements.Boxes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Managers.States
{
    internal class PauseMenu : State
    {
        public override StateManager.States GetStateEnum => StateManager.States.PauseMenu;
        PauseBox pauseBox;
        Textures.Texture pauseBackground;
        List<DialogueBox> dialogueBoxes;

        public PauseMenu() : base()
        {
            dialogueBoxes = new List<DialogueBox>();
            pauseBox = new PauseBox();
            pauseBackground = new Textures.Texture(new GfxPath(GfxType.UI, "PauseBackground"));
        }

        public override void Update()
        {
            if (dialogueBoxes.Count == 0 && KeyboardStateCache.GetPress(Microsoft.Xna.Framework.Input.Keys.Escape))
            {
                StateManager.RequestStateChange(StateManager.States.Game);
            }
        }
        public override void Rescale()
        {
            lock (HUDManager.UiLock)
            {
                pauseBox.Rescale();
            }
            base.Rescale();

        }

        public override void PopUp(DialogueBox aBox) => dialogueBoxes.Add(aBox);

        public override void RemovePopUp(DialogueBox aBox) => Debug.Assert(dialogueBoxes.Remove(aBox));

        public override bool Release(ReleaseEvent aReleaseEvent) => false;

        public override bool Scroll(ScrollEvent aScrollEvent) => false;

        public override bool Click(ClickEvent aClickEvent)
        {
            return false;
        }

        public override void OnEnter()
        {
            
        }

        public override void OnLeave()
        {
            
        }

        public override RenderTarget2D Draw()
        {
            PrepRender(Color.Purple);

            spriteBatch.Draw(StateManager.FinalGameFrame, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9f); //draw game
            pauseBackground.Draw(spriteBatch, Vector2.Zero); //draw gray screen overlay
            lock (HUDManager.UiLock)
            {
                pauseBox.Draw(spriteBatch); //draw pause menu
                for (int i = 0; i < dialogueBoxes.Count; i++)
                {
                    dialogueBoxes[i].Draw(spriteBatch);
                }
            }

            CleanRender();
            return renderTarget;
        }

        internal bool UiClick(ClickEvent aClickEvent)
        {
            if (dialogueBoxes.Count > 0)
            {
                for (int i = 0; i < dialogueBoxes.Count; i++)
                {
                    if (dialogueBoxes[i].ClickedOn(aClickEvent)) return true;
                }
                return false;
            }
            return pauseBox.ClickedOn(aClickEvent);
        }

        internal bool UiRelease(ReleaseEvent aReleaseEvent)
        {
            if (dialogueBoxes.Count > 0)
            {
                for (int i = 0; i < dialogueBoxes.Count; i++)
                {
                    if (dialogueBoxes[i].ReleasedOn(aReleaseEvent)) return true;
                }
                return false;
            }
            return pauseBox.ReleasedOn(aReleaseEvent);
        }

        internal bool UiScroll(ScrollEvent aScrollEvent)
        {
            if (dialogueBoxes.Count > 0)
            {
                for (int i = 0; i < dialogueBoxes.Count; i++)
                {
                    if (dialogueBoxes[i].ScrolledOn(aScrollEvent)) return true;
                }
                return false;
            }
            return pauseBox.ScrolledOn(aScrollEvent);
        }

        internal void UiUpdate()
        {
            pauseBox.Update();
            for (int i = 0; i < dialogueBoxes.Count; i++)
            {
                dialogueBoxes[i].Update();
            }
        }

    }
}
