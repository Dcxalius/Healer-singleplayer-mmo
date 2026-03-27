using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Messaging.Events;
using Project_1.Textures;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Boxes;
using System;

namespace Project_1.UI.LoadingMenu
{
    internal class ExistingSave : Box
    {
        readonly Label playerName;
        public new SaveUiSnapshot Save => save;
        readonly SaveUiSnapshot save;
        public static Action<ExistingSave> callAtClick;

        public ExistingSave(SaveUiSnapshot aSave, UIElement aParent) : base(new UITexture("WhiteBackground", Color.White), RelativeScreenPosition.Zero, RelativeScreenPosition.Zero, aParent)
        {
            save = aSave;
            playerName = new Label(aSave.SaveName, RelativeScreenPosition.Zero, RelativeScreenPosition.Zero, Label.TextAllignment.CentreLeft, Color.Black, aParent: this);
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
            callAtClick?.Invoke(this);
        }
    }
}
