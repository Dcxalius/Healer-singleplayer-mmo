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
        public static Action<ExistingSave> callAtClick; //TODO: ECH public field, use a setter method/ctor instead

        public ExistingSave(UIElement aParent, SaveUiSnapshot aSave) : base(aParent, new UITexture("WhiteBackground", Color.White), RelativeScreenPosition.Zero, RelativeScreenPosition.Zero)
        {
            save = aSave;
            playerName = new Label(this, RelativeScreenPosition.Zero, RelativeScreenPosition.Zero, Label.TextAllignment.CentreLeft, Color.Black, aText: aSave.SaveName);
            //Q: Why is this size 0? If the name is handled by the savedetails portion this label should be removed unless we wanna do something fancier
        }

        public override void ClickedOnAndReleasedOnMe()
        {
            base.ClickedOnAndReleasedOnMe();
            callAtClick?.Invoke(this);
        }
    }
}
