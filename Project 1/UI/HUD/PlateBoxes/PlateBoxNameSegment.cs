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
        

        public PlateBoxNameSegment(string aName, Color aNamePlateColor, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aNamePlateColor, aPos, aSize)
        {
            name = new Label(aName, RelativeScreenPosition.Zero, RelativeScreenPosition.One, Label.TextAllignment.Centred, Color.White, "Comforaa-msdf", 13);
            AddChild(name);
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
