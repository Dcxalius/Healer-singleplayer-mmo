using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Managers;
using Project_1.Managers.States;
using Project_1.Textures;
using Project_1.UI.HUD;
using Project_1.UI.UIElements.Boxes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements
{
    internal class MoveHUDBox : ABDialogueBox
    {
        static RelativeScreenPosition size => new RelativeScreenPosition(0.2f, 0.2f);
        static RelativeScreenPosition pos => new RelativeScreenPosition(0.5f, 0.2f) - size / 2;
        static RelativeScreenPosition spacing => RelativeScreenPosition.GetSquareFromX(0.005f);
        static RelativeScreenPosition buttonSize => new RelativeScreenPosition(0.07f, 0.04f);
        static RelativeScreenPosition aButtonPos => size.OnlyY + new RelativeScreenPosition(spacing.X, -spacing.Y) - buttonSize.OnlyY;
        static RelativeScreenPosition bButtonPos => size - spacing - buttonSize;

        public MoveHUDBox() : base("aMessage", Color.White, LocationOfPopUp.StateManager, PausesGame.NoPause, new List<Action> { HUDManager.DisableHUDMoveable, SaveManager.SaveHUD }, new List<Action> { HUDManager.ResetHUDMoveable },
            new UITexture("WhiteBackground", Color.Black), pos, size, aButtonPos, bButtonPos, buttonSize, Color.Gray, "Confirm", "Reset", Color.White)
        {
            Dragable = true;
            for (int i = 0; i < buttons.Count; i++)
            {
                buttons[i].AddAction(
                    () => 
                    StateManager.SetState(StateManager.States.PauseMenu));
            }
        }
    }
}
