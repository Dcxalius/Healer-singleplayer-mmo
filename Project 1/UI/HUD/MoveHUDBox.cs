using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Managers.States;
using Project_1.Textures;
using Project_1.UI.HUD.Managers;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Boxes;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.HUD
{
    internal class MoveHUDBox : ABDialogueBox
    {
        static RelativeScreenPosition statSize => new RelativeScreenPosition(0.2f, 0.2f);
        static RelativeScreenPosition Pos => new RelativeScreenPosition(0.5f, 0.2f) - statSize / 2;
        static RelativeScreenPosition Spacing => RelativeScreenPosition.GetSquareFromX(0.005f, statSize.ToAbsoluteScreenPos());
        static RelativeScreenPosition ButtonSize => new RelativeScreenPosition(0.3f, 0.2f);
        static RelativeScreenPosition AButtonPos => RelativeScreenPosition.One.OnlyY + new RelativeScreenPosition(Spacing.X, -Spacing.Y) - ButtonSize.OnlyY;
        static RelativeScreenPosition BButtonPos => RelativeScreenPosition.One - Spacing - ButtonSize;

        DescriptCheckBox sizeChangeCheckBox;
        public MoveHUDBox(UIElement aParent) : base(aParent, "HUD Changer", Color.White, LocationOfPopUp.StateManager, PausesGame.NoPause, new List<Action> { HUDManager.DisableHudMoveable, HUDManager.Save }, new List<Action> { HUDManager.ResetHudMoveable },
            new UITexture("WhiteBackground", Color.Black), Pos, statSize, AButtonPos, BButtonPos, ButtonSize, Color.Gray, "Confirm", "Reset", Color.White)
        {
            Dragable = true; //TODO: Should this be movable?
            for (int i = 0; i < buttons.Count; i++) buttons[i].AddAction(() => StateManager.RequestStateChange(StateManager.States.PauseMenu)); //TODO: Formalize this

            sizeChangeCheckBox = new DescriptCheckBox(this, "Size change", Color.White, false, () => MailboxManager.PublishUiEvent(new HudSizeChangeRequested(true)), () => MailboxManager.PublishUiEvent(new HudSizeChangeRequested(false)), Spacing + new RelativeScreenPosition(0.05f).OnlyY, new RelativeScreenPosition(1f, 0.15f), Size);
            
            AddChild(sizeChangeCheckBox);
        }
    }
}
