using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Input;
using Project_1.Textures;
using Project_1.UI;
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
        static PauseBox pauseBox;
        RenderTarget2D pauseTarget;
        SpriteBatch pauseDraw;
        Textures.Texture pauseBackground;
        List<DialogueBox> dialogueBoxes;

        public PauseMenu()
        {
            dialogueBoxes = new List<DialogueBox>();
            pauseTarget = GraphicsManager.CreateRenderTarget(Camera.Camera.ScreenSize);
            pauseDraw = GraphicsManager.CreateSpriteBatch();
            pauseBox = new PauseBox();
            pauseBackground = new Textures.Texture(new GfxPath(GfxType.UI, "PauseBackground"));
        }

        public override void Update()
        {
            pauseBox.Update(null);
            if (dialogueBoxes.Count > 0)
            {
                for (int i = 0; i < dialogueBoxes.Count; i++)
                {
                    dialogueBoxes[i].Update(null);
                }
                return;
            }
            if (InputManager.GetPress(Microsoft.Xna.Framework.Input.Keys.Escape))
            {
                StateManager.SetState(StateManager.States.Game);
            }
        }
        public override void Rescale()
        {
            pauseTarget = GraphicsManager.CreateRenderTarget(Camera.Camera.ScreenSize);
            pauseBox.Rescale();

        }

        public override void PopUp(DialogueBox aBox) => dialogueBoxes.Add(aBox);

        public override void RemovePopUp(DialogueBox aBox) => Debug.Assert(dialogueBoxes.Remove(aBox));

        public override bool Release(ReleaseEvent aReleaseEvent)
        {
            throw new NotImplementedException();
        }

        public override bool Scroll(ScrollEvent aScrollEvent)
        {
            throw new NotImplementedException();
        }

        public override bool Click(ClickEvent aClickEvent)
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

        public override void OnEnter()
        {
            
        }

        public override void OnLeave()
        {
            
        }

        public override RenderTarget2D Draw()
        {
            GraphicsManager.SetRenderTarget(pauseTarget);
            pauseDraw.Begin();
            GraphicsManager.ClearScreen(Color.Purple);


            pauseDraw.Draw(StateManager.FinalGameFrame, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9f); //draw game
            pauseBackground.Draw(pauseDraw, Vector2.Zero); //draw gray screen overlay
            pauseBox.Draw(pauseDraw); //draw pause menu
            for (int i = 0; i < dialogueBoxes.Count; i++)
            {
                dialogueBoxes[i].Draw(pauseDraw);
            }

            pauseDraw.End();
            GraphicsManager.SetRenderTarget(null);
            return pauseTarget;
        }

    }
}
