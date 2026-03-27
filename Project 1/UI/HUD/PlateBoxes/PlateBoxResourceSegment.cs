using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Messaging.Events;
using Project_1.Textures;
using Project_1.UI.UIElements.Bars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.HUD.PlateBoxes
{
    internal class PlateBoxResourceSegment : PlateBoxSegment
    {
        public PlateBoxResourceSegment(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new Color(255, 211, 211, 120), aPos, aSize)
        {
            bar.Visible = true;
            bar.Color = Color.Transparent;
        }

        public override void Refresh(in EntityUiSnapshot snapshot)
        {
            bar.Value = snapshot.CurrentResource;
            bar.MaxValue = snapshot.MaxResource;
        }

        public void SetTarget(in EntityUiSnapshot snapshot)
        {
            bar.MaxValue = snapshot.MaxResource;
            bar.Value = snapshot.CurrentResource;
            bar.Color = snapshot.ResourceColor;
        }

        public override void Clear()
        {
            bar.Value = 0;
            bar.MaxValue = 0;
            bar.Color = Color.Transparent;
        }
    }
}
