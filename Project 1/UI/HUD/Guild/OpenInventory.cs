﻿using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Textures;
using Project_1.UI.UIElements.Buttons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.HUD.Guild
{
    internal class OpenInventory : GFXButton
    {
        public OpenInventory(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new GfxPath(GfxType.Item, "DefaultBag"), aPos, aSize, Color.White)
        {
        }
    }
}
