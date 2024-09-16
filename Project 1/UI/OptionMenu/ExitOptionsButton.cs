using Microsoft.Xna.Framework;
using Project_1.Managers;
using Project_1.Textures;
using Project_1.UI.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.OptionMenu
{
    internal class ExitOptionsButton : GFXButton
    {
        public ExitOptionsButton() : base(new GfxPath(GfxType.UI, "XButton"), new Vector2(0.9f), new Vector2(0.05f), Color.Beige)
        {

        }

        public override void HoldReleaseOnMe()
        {
            base.HoldReleaseOnMe();
         
            UIManager.CloseAllOptionMenuStuff();
            
            StateManager.SetState(State.Pause);

        }
    }
}
