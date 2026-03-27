using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Managers.States;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.UI;
using Project_1.UI.HUD;
using Project_1.UI.UIElements.Buttons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.PauseMenu
{
    internal class SaveButton : Button
    {
        public SaveButton(RelativeScreenPosition aPos, RelativeScreenPosition aSize, UI.UIElements.UIElement aParent = null) : base(aPos, aSize, Color.AliceBlue, "Save", Color.Black, aParent)
        {

        }

        public override void ClickedOnAndReleasedOnMe()
        {
            if (UiPlayerStateCache.Valid && UiPlayerStateCache.InCombatOrPartyInCombat)
            {
                StateManager.PopUp(new UIElements.Boxes.DialogueBox("Cannot save while in combat.", Color.Black, UIElements.Boxes.DialogueBox.LocationOfPopUp.StateManager, UIElements.Boxes.DialogueBox.PausesGame.NoPause, null, new Textures.UITexture("GrayBackground", Color.White), new RelativeScreenPosition(0.4f, 0.5f), new RelativeScreenPosition(0.2f, 0.1f), "Okay"));
                base.ClickedOnAndReleasedOnMe();
                return;
            }
            MailboxManager.PublishSimCommand(new SaveDataRequested());

            base.ClickedOnAndReleasedOnMe();

        }
    }
}
