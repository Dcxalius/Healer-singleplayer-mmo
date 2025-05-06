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

        static RelativeScreenPosition CalculateSize(float aSizeY, AbsoluteScreenPosition aParentSize) => RelativeScreenPosition.GetSquareFromY(aSizeY, aParentSize);

        public DescriptCheckBox(string aDescription, Color aTextColor, bool aStartState, Action aTickedAction, Action aUntickedAction, RelativeScreenPosition aPos, RelativeScreenPosition aSize, AbsoluteScreenPosition aParentSize) : this(aDescription, aTextColor, aStartState, new List<Action> { aTickedAction }, new List<Action> { aUntickedAction }, aPos, aSize, aParentSize) { }

        public DescriptCheckBox(string aDescription, Color aTextColor, bool aStartState, List<Action> aTickedActions, List<Action> aUntickedActions, RelativeScreenPosition aPos, RelativeScreenPosition aSize, AbsoluteScreenPosition aParentSize) : base(aStartState, aTickedActions, aUntickedActions, aPos, CalculateSize(aSize.Y, aParentSize))
        {
            RelativeScreenPosition spacing = RelativeScreenPosition.GetSquareFromX(0.05f, Size);
            description = new Label(aDescription, RelativeScreenPosition.One.OnlyX + spacing.OnlyX, RelativeScreenPosition.One - CalculateSize(aSize.Y, aParentSize).OnlyX - spacing.OnlyX, Label.TextAllignment.CentreLeft, aTextColor);
            capturesClick = false;
            AddChild(description);
        }
    }
}
