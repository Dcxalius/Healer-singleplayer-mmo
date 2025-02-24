using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects;
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
        public SaveButton(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aPos, aSize, Color.AliceBlue, "Save")
        {

        }

        public override void HoldReleaseOnMe()
        {
            ObjectFactory.SaveData();

            base.HoldReleaseOnMe();

        }
    }
}
