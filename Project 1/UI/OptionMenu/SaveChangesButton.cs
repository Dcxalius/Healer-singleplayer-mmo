using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Managers;
using Project_1.UI.UIElements.Buttons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.OptionMenu
{
    internal class SaveChangesButton : Button
    {
        List<Action> finalActions;

        public SaveChangesButton(RelativeScreenPosition aPos, RelativeScreenPosition aSize, Color aColor, Color aTextColor) : base(aPos, aSize, aColor, "Save Changes", aTextColor)
        {
            Visible = false;
            finalActions = new List<Action>();
        }

        public void AddFinalActions(Action aAction)
        {
            if (finalActions.Contains(aAction)) return;

            finalActions.Add(aAction);
        }

        public override void ClickedOnAndReleasedOnMe()
        {
            Actions.AddRange(finalActions);

            base.ClickedOnAndReleasedOnMe();

            OptionManager.ClearButtons();
        }

        public void ClearActions()
        {
            Visible = false;
            finalActions.Clear();
            Actions.Clear();

        }

    }
}
