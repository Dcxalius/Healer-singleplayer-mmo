using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Input;
using Project_1.Textures;
using Project_1.UI;
using Project_1.UI.HUD.Managers;
using Project_1.UI.OptionMenu;
using Project_1.UI.PauseMenu;
using Project_1.UI.UIElements.Boxes;
using Project_1.UI.UIElements;
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
        readonly UiElementDrawList drawListA = new UiElementDrawList();
        readonly UiElementDrawList drawListB = new UiElementDrawList();
        volatile UiElementDrawList drawList;
        volatile bool drawListDirty = true;

        public PauseMenu() : base()
        {
            dialogueBoxes = new List<DialogueBox>();
            pauseBox = new PauseBox();
            pauseBackground = new Textures.Texture(new GfxPath(GfxType.UI, "PauseBackground"));
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

        public override void PopUp(DialogueBox aBox)
        {
            dialogueBoxes.Add(aBox);
            drawListDirty = true;
            MarkUiDirty();
        }

        public override void RemovePopUp(DialogueBox aBox)
        {
            Debug.Assert(dialogueBoxes.Remove(aBox));
            drawListDirty = true;
            MarkUiDirty();
        }

        public override bool Release(ReleaseEvent aReleaseEvent) => false;

        public override bool Scroll(ScrollEvent aScrollEvent) => false;

        public override void OnEnter()
        {
            MarkUiDirty();
        }

        public override void OnLeave()
        {
            
        }

        public override RenderTarget2D Draw()
        {
            if (!renderDirty || drawList == null) return renderTarget;
            renderDirty = false;
            PrepRender(Color.Purple);

            spriteBatch.Draw(StateManager.FinalGameFrame, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9f); //draw game
            pauseBackground.Draw(spriteBatch, Vector2.Zero); //draw gray screen overlay
            drawList?.Draw(spriteBatch);

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

        internal bool UiEscapePressed() => false;

        internal void UiUpdate()
        {
            pauseBox.Update();
            for (int i = 0; i < dialogueBoxes.Count; i++)
            {
                dialogueBoxes[i].Update();
            }
            if (drawListDirty)
            {
                drawListDirty = false;
                BuildDrawList();
            }
        }

        internal void HandleEscapePressed()
        {
            if (dialogueBoxes.Count == 0)
            {
                StateManager.RequestStateChange(StateManager.States.Game);
            }
        }

        void BuildDrawList()
        {
            UiElementDrawList buildTarget = ReferenceEquals(drawList, drawListA) ? drawListB : drawListA;
            buildTarget.Set(pauseBox, dialogueBoxes);
            drawList = buildTarget;
            MarkUiDirty();
        }

        internal void UiRescale()
        {
            pauseBox?.Rescale();
            for (int i = 0; i < dialogueBoxes.Count; i++)
            {
                dialogueBoxes[i].Rescale();
            }
            drawListDirty = true;
            BuildDrawList();
        }

    }
}
