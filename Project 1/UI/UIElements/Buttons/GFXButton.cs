using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Input;
using Project_1.Managers;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.Buttons
{
    internal class GFXButton : Button
    {
        public UITexture GfxOnButton { get => imageOnButton.Gfx; }

        protected Image imageOnButton;


        public GFXButton(UIElement aParent, GfxPath aPath, RelativeScreenPosition aPos, RelativeScreenPosition aSize, Color aColorOfBorder) : base(aParent, aPos, aSize, aColorOfBorder)
        {
            imageOnButton = new Image(this, new UITexture(aPath, Color.White), new RelativeScreenPosition(0.1f), new RelativeScreenPosition(0.8f));
            AddChild(imageOnButton);
        }

        public GFXButton(UIElement aParent, List<Action> aActions, GfxPath aPath, RelativeScreenPosition aPos, RelativeScreenPosition aSize, Color aColor, string aText = null, Color? aTextColor = null) : base(aParent, aActions, aPos, aSize, aColor, aText, aTextColor)
        {
            imageOnButton = new Image(this, new UITexture(aPath, Color.White), new RelativeScreenPosition(0.1f), new RelativeScreenPosition(0.8f));
            AddChild(imageOnButton);
        }
        protected override void ClickedOnMe(ClickEvent aClick)
        {
            base.ClickedOnMe(aClick);

            imageOnButton.Color = Color.Gray;
        }
        protected override void Released()
        {
            base.Released();

            imageOnButton.Color = Color.White;
        }
    }
}
