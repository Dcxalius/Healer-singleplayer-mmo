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
    internal class PlateBoxNameSegment : PlateBoxSegment
    {
        public Label Name => name;
        Label name;
        

        public PlateBoxNameSegment(UIElement aParent, Color aNamePlateColor, Color aTextColor, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aParent, aNamePlateColor, aPos, aSize)
        {
            name = new Label(this, RelativeScreenPosition.Zero, RelativeScreenPosition.One, Label.TextAllignment.Centred, aTextColor, "Comforaa-msdf", 13, "Name");
        }

        public override void Clear()
        {
            BackgroundColor = Color.Magenta;
            name.Text = null;
        }

        public override void Refresh(in EntityUiSnapshot snapshot)
        {
            name.Text = snapshot.Name;
            gfx.Color = snapshot.RelationColor;
        }
    }
}
