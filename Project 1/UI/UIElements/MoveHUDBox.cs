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
        static RelativeScreenPosition Size => new RelativeScreenPosition(0.2f, 0.2f);
        static RelativeScreenPosition Pos => new RelativeScreenPosition(0.5f, 0.2f) - Size / 2;
        static RelativeScreenPosition Spacing => RelativeScreenPosition.GetSquareFromX(0.005f);
        static RelativeScreenPosition ButtonSize => new RelativeScreenPosition(0.07f, 0.04f);
        static RelativeScreenPosition AButtonPos => Size.OnlyY + new RelativeScreenPosition(Spacing.X, -Spacing.Y) - ButtonSize.OnlyY;
        static RelativeScreenPosition BButtonPos => Size - Spacing - ButtonSize;

        DescriptCheckBox sizeChangeCheckBox;
        public MoveHUDBox() : base("HUD Changer", Color.White, LocationOfPopUp.StateManager, PausesGame.NoPause, new List<Action> { HUDManager.DisableHudMoveable, SaveManager.SaveHUD }, new List<Action> { HUDManager.ResetHudMoveable },
            new UITexture("WhiteBackground", Color.Black), Pos, Size, AButtonPos, BButtonPos, ButtonSize, Color.Gray, "Confirm", "Reset", Color.White)
        {
            Dragable = true;
            for (int i = 0; i < buttons.Count; i++) buttons[i].AddAction(() => StateManager.SetState(StateManager.States.PauseMenu)); //TODO: Formalize this

            sizeChangeCheckBox = new DescriptCheckBox("Size change", Color.White, false, HUDManager.ChangeSizes, HUDManager.DisableSizeChanges, Spacing + new RelativeScreenPosition(0.05f).OnlyY, new RelativeScreenPosition(0.4f, 0.03f));

            AddChild(sizeChangeCheckBox);
        }
    }
}
