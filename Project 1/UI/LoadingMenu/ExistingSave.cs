using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Managers.Saves;
using Project_1.Textures;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Boxes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.LoadingMenu
{
    internal class ExistingSave : Box
    {
        Label playerName;
        public Save Save => save;
        Save save;
        public static Action<ExistingSave> callAtClick;
        public ExistingSave(Save aSave) : base(new UITexture("WhiteBackground", Color.White), RelativeScreenPosition.Zero, RelativeScreenPosition.Zero)
        { 
            save = aSave;
            playerName = new Label(aSave.Name, RelativeScreenPosition.Zero, RelativeScreenPosition.Zero, Label.TextAllignment.CentreLeft, Color.Black);
            AddChild(playerName);
        }

        public override void Resize(RelativeScreenPosition aSize)
        {
            base.Resize(aSize);

            playerName.Resize(aSize);
        }

        public override void ClickedOnAndReleasedOnMe()
        {
            base.ClickedOnAndReleasedOnMe();
            callAtClick.Invoke(this);
        }
    }
}
