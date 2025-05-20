using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.HUD.Windows.Gossip
{
    internal class ChatGossipOption : GossipOption
    {
        public ChatGossipOption(string aDescriptor, string aText, GossipOption[] aGossipOptions) : base(aDescriptor)
        {
            Actions.Add(() => HUDManager.OpenGossipWindow(aText, aGossipOptions));
        }
    }
}
