using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.UI.UIElements.Buttons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.CharacterCreator
{
    internal class FinalizeCharacterButton : Button
    {
        public FinalizeCharacterButton(List<Action> aActions, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aActions, aPos, aSize, Color.White, "Create Character", Color.Black)
        {
        }

        public override void ClickedOnAndReleasedOnMe()
        {
            base.ClickedOnAndReleasedOnMe();


        }

    }
}
