using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Textures;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Boxes;
using Project_1.UI.UIElements.Buttons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.HUD.Windows.Gossip
{
    internal class GossipOption : Button
    {
        //Label text;

        public GossipOption(string aDescriptor) : base(RelativeScreenPosition.Zero, RelativeScreenPosition.Zero, Color.Lime, aDescriptor)
        {
            //text = new Label(aDescriptor, RelativeScreenPosition.Zero, RelativeScreenPosition.Zero, Label.TextAllignment.Centred, Color.Black);
            //AddChild(text);
        }
    }
}
