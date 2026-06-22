using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects.Entities;
using Project_1.Textures;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Boxes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project_1.UI.HUD
{
    internal class DPSMeter : Box
    {
        //D: Create a combat history that lives on the sim thread.

        //D: Settable target = Any entity the player of the party has interacted(attacked, healed, cleansed, etc) in combat history.
        //D: Settable windows = Since party start, since encounter enter.
        //D: All?

        //D: DPS meter should have a set of bars it increases or decreases depending on the metric given (Damage, dps, healing, etc)

        public DPSMeter(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(null, new UITexture("WhiteBackground", Color.White), aPos, aSize)
        {

        }
    }

    internal class MeterEntry : UIElement
    {
        public MeterEntry(UIElement aParent, UITexture aGfx, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aParent, aGfx, aPos, aSize)
        {
        }
    }
}
