using Project_1.Camera;
using Project_1.Managers.States;
using Project_1.Managers;
using Project_1.Textures;
using Project_1.UI.HUD;
using Project_1.UI.UIElements.Buttons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Project_1.UI.UIElements.Boxes.DialogueBox;

namespace Project_1.UI.UIElements.Boxes
{
    internal class ButtonBox : Box //TODO: Improve this
    {
        protected List<Button> buttons;
        PausesGame pauses;

        public ButtonBox(Button[] aButtons, PausesGame aPauses, UITexture aGfx, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aGfx, aPos, aSize) 
        {
            buttons = aButtons.ToList();

            pauses = aPauses;
            if (aPauses == PausesGame.Pauses) 
            {
                TimeManager.StartPause(this);
                buttons.First().AddAction(new Action(() => TimeManager.StopPause(this)));

            }



            AddChildren(buttons);
        }

        public void AddButton(Button aButton)
        {
            if (pauses == PausesGame.Pauses)
            {
                aButton.AddAction(new Action(() => TimeManager.StopPause(this)));
            }

            buttons.Add(aButton);
            AddChild(buttons.Last());
        }
    }
}
