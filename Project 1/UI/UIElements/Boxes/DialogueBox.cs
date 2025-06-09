using Project_1.Camera;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_1.Managers.States;
using Project_1.UI.UIElements.Buttons;
using System.Diagnostics.CodeAnalysis;
using Project_1.Managers;
using Project_1.UI.HUD.Managers;

namespace Project_1.UI.UIElements.Boxes
{
    internal class DialogueBox : ButtonBox
    {
        static RelativeScreenPosition DefaultButtonPos(RelativeScreenPosition aSize) => aSize - defaultButtonSize - defaultEdgeSpacing;
        static RelativeScreenPosition defaultButtonSize = new RelativeScreenPosition(0.05f, 0.03f);
        static RelativeScreenPosition defaultEdgeSpacing = new RelativeScreenPosition(0.025f, 0.025f);
        static Color defaultButtonColor = Color.LightGray;
        static Color defaultButtonTextColor = Color.Black;

        public enum PausesGame
        {
            Pauses,
            NoPause
        }

        public enum LocationOfPopUp
        {
            HUDManager,
            StateManager
        }

        Action unPauseGame => new Action(() => TimeManager.StopPause(this));


        Label textToDisplay;

        public DialogueBox(string aMessage, Color aMessageColor, LocationOfPopUp aLocation, PausesGame aPauses, List<Action> aAction, UITexture aGfx, RelativeScreenPosition aPos, RelativeScreenPosition aSize, string aButtonText = null) 
            : this(aMessage, aMessageColor, aLocation, aPauses, aAction, aGfx, aPos, aSize, DefaultButtonPos(aSize), defaultButtonSize, defaultButtonColor, aButtonText, defaultButtonTextColor) { }

        public DialogueBox(string aMessage, Color aMessageColor, LocationOfPopUp aLocation, PausesGame aPauses, List<Action> aAction, UITexture aGfx, 
            RelativeScreenPosition aPos, RelativeScreenPosition aSize, 
            RelativeScreenPosition aButtonPos, RelativeScreenPosition aButtonSize, Color aButtonColor, string aButtonText = null, Color? aButtonTextColor = null)
                : base(
                    new Button[] { new Button(aAction, aButtonPos, aButtonSize, aButtonColor, aButtonText, aButtonTextColor)}, aPauses, aGfx, aPos, aSize)
        {
            switch (aLocation)
            {
                case LocationOfPopUp.HUDManager:
                    buttons.First().AddAction(new Action(() => HUDManager.RemoveDialogueBox(this)));
                    break;
                case LocationOfPopUp.StateManager:
                    buttons.First().AddAction(new Action(() => StateManager.RemovePopUp(this)));
                    break;
                default:
                    throw new NotImplementedException();
            }


            textToDisplay = new Label(aMessage, defaultEdgeSpacing, RelativeScreenPosition.One - defaultEdgeSpacing * 2, Label.TextAllignment.TopCentre, aMessageColor);
            AddChild(textToDisplay);
        }
    }
}
