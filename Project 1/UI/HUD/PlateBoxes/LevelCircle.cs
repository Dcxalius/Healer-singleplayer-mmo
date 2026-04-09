using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Messaging.Events;
using Project_1.Textures;
using Project_1.UI.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.HUD.PlateBoxes
{
    internal class LevelCircle : UIElement
    {
        Label numberLabel;

        public LevelCircle(UIElement aParent, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aParent, new UITexture("LevelCircle", Color.White), aPos, aSize)
        {
            numberLabel = new Label(this, RelativeScreenPosition.Zero, RelativeScreenPosition.One, Label.TextAllignment.Centred, Color.Pink, aText: "1");
            capturesClick = false;

            AddChild(numberLabel);
        }

        public void Refresh(in EntityUiSnapshot snapshot)
        {
            numberLabel.Text = snapshot.Level.ToString();
        }
    }
}
