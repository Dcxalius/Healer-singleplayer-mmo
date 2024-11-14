using Microsoft.VisualBasic.Devices;
using Microsoft.Xna.Framework;
using Project_1.Input;
using Project_1.Textures;
using Project_1.UI.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.HUD
{
    internal class DescriptorBox : Box
    {
        Text descriptedName;
        Text description;

        public DescriptorBox() : base(new UITexture("GrayBackground", Color.Transparent), Vector2.Zero, Vector2.Zero)
        {
            visible = false;
            descriptedName = new Text("Gloryse");
            description = new Text("Gloryse");
        }

        public override void Update(in UIElement aParent)
        {
            base.Update(aParent);

            if (visible)
            {
                Move(InputManager.GetMousePosRelative());
            }
        }
    }
}
