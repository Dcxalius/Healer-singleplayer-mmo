using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.UI.UIElements.Buttons;
using System;
using System.Collections.Generic;

namespace Project_1.UI.CharacterCreator
{
    internal class FinalizeCharacterButton : Button
    {
        public FinalizeCharacterButton(List<Action> aActions, RelativeScreenPosition aPos, RelativeScreenPosition aSize, UI.UIElements.UIElement aParent = null) : base(aActions, aPos, aSize, Color.White, "Create Character", Color.Black, aParent)
        {
        }

        public override void ClickedOnAndReleasedOnMe()
        {
            base.ClickedOnAndReleasedOnMe();


        }

    }
}
