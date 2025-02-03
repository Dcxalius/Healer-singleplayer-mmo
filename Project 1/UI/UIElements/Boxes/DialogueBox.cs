using Project_1.Camera;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_1.Managers.States;
using Project_1.UI.HUD;

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

        static Action unPauseGame => new Action(() => StateManager.SetState(StateManager.States.Game));


        Label textToDisplay;

        public DialogueBox(string aMessage, Color aMessageColor, PausesGame aPauses, Action aAction, UITexture aGfx, RelativeScreenPosition aPos, RelativeScreenPosition aSize, string aButtonText = null) 
            : this(aMessage, aMessageColor, aPauses, aAction, aGfx, aPos, aSize, DefaultButtonPos(aSize), defaultButtonSize, defaultButtonColor, aButtonText, defaultButtonTextColor) { }

        public DialogueBox(string aMessage, Color aMessageColor, PausesGame aPauses, Action aAction, UITexture aGfx, 
            RelativeScreenPosition aPos, RelativeScreenPosition aSize, 
            RelativeScreenPosition aButtonPos, RelativeScreenPosition aButtonSize, Color aButtonColor, string aButtonText = null, Color? aButtonTextColor = null)
                : base(
                    new Button[] { new Button(aPauses == PausesGame.Pauses ? new List<Action> { unPauseGame, aAction } : new List<Action> { aAction }, 
                    aButtonPos, aButtonSize, aButtonColor, aButtonText, aButtonTextColor)}, aGfx, aPos, aSize)
        {
            if (aPauses == PausesGame.Pauses) StateManager.SetState(StateManager.States.PausedGame);
            buttons[0].AddAction(new Action(() => HUDManager.RemoveDialogueBox(this)));
            
            textToDisplay = new Label(aMessage, defaultEdgeSpacing, aSize - defaultEdgeSpacing * 2, Label.TextAllignment.TopCentre, aMessageColor);
            AddChild(textToDisplay);
        }
    }
}
