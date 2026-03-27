using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
    internal class PlateBoxHealthSegment : PlateBoxSegment
    {
        public PlateBoxHealthSegment(RelativeScreenPosition aPos, RelativeScreenPosition aSize, UI.UIElements.UIElement aParent = null) : base(new Color(255, 211, 211, 120), aPos, aSize, aParent: aParent)
        {
            bar.Visible = true;
            bar.Color = Color.Red;
        }

        public void SetTextSize(ResourceBar.Labels aLabel, float textSize) => bar.SetLabelTextSize(aLabel, textSize);

        public override void Refresh(in EntityUiSnapshot snapshot)
        {
            bar.MaxValue = (float)snapshot.MaxHealth;
            bar.Value = (float)snapshot.CurrentHealth;
        }

        public void SetTarget(in EntityUiSnapshot snapshot)
        {
            bar.MaxValue = (float)snapshot.MaxHealth;
            bar.Value = (float)snapshot.CurrentHealth;
        }

        public override void Clear()
        {
            bar.Value = 0;
            bar.MaxValue = 0;
        }
    }
}
