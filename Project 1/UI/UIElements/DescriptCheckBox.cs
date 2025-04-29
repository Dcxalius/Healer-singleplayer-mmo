using Microsoft.Xna.Framework;
using Project_1.Camera;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements
{
    internal class DescriptCheckBox : CheckBox
    {
        Label description;

        static RelativeScreenPosition CalculateSize(RelativeScreenPosition aSize) => RelativeScreenPosition.GetSquareFromY(aSize.Y); //TODO: ???????????

        public DescriptCheckBox(string aDescription, Color aTextColor, bool aStartState, Action aTickedAction, Action aUntickedAction, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : this(aDescription, aTextColor, aStartState, new List<Action> { aTickedAction }, new List<Action> { aUntickedAction }, aPos, aSize) { }

        public DescriptCheckBox(string aDescription, Color aTextColor, bool aStartState, List<Action> aTickedActions, List<Action> aUntickedActions, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aStartState, aTickedActions, aUntickedActions, aPos, CalculateSize(aSize))
        {
            RelativeScreenPosition spacing = RelativeScreenPosition.GetSquareFromX(0.005f, Size);
            description = new Label(aDescription, CalculateSize(aSize).OnlyX + spacing.OnlyX, aSize - CalculateSize(aSize).OnlyX - spacing.OnlyX, Label.TextAllignment.CentreLeft, aTextColor);
            capturesClick = false;
            AddChild(description);
        }
    }
}
