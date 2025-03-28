using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Managers;
using Project_1.Textures;
using Project_1.UI.UIElements.Buttons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.Boxes
{
    internal class ABDialogueBox : DialogueBox
    {
        Button secondButton;
        //public ABDialogueBox(string aMessage, Color aMessageColor, LocationOfPopUp aLocation, PausesGame aPauses, List<Action> aAction, UITexture aGfx, RelativeScreenPosition aPos, RelativeScreenPosition aSize, string aButtonText = null) : base(aMessage, aMessageColor, aLocation, aPauses, aAction, aGfx, aPos, aSize, aButtonText)
        //{
        //}

        public ABDialogueBox(string aMessage, Color aMessageColor, LocationOfPopUp aLocation, PausesGame aPauses, List<Action> aAAction, List<Action> aBAction, UITexture aGfx, RelativeScreenPosition aPos, RelativeScreenPosition aSize, RelativeScreenPosition aAButtonPos, RelativeScreenPosition aBButtonPos, RelativeScreenPosition aButtonSize, Color aButtonColor, string aAButtonText = null, string aBButtonText = null, Color? aButtonTextColor = null) : base(aMessage, aMessageColor, aLocation, aPauses, aAAction, aGfx, aPos, aSize, aAButtonPos, aButtonSize, aButtonColor, aAButtonText, aButtonTextColor)
        {
            secondButton = new Button(aBAction, aBButtonPos, aButtonSize, aButtonColor, aBButtonText, aButtonTextColor);
            AddButton(secondButton);
        }
    }
}
