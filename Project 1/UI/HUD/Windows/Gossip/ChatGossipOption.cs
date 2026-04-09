using Project_1.UI.UIElements;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.HUD.Windows.Gossip
{
    internal class ChatGossipOption : GossipOption
    {
        public string IntroText => introText;
        string introText;
        public GossipOption[] GossipOptions => gossipOptions.ToArray();

        List<GossipOption> gossipOptions;

        public static Action<ChatGossipOption> SetGossipWindow;

        public ChatGossipOption(UIElement aParent, string aDescriptor, string aText) : base(aParent, aDescriptor)
        {
            gossipOptions = new List<GossipOption>();
            introText = aText;
            Actions.Add(() => SetGossipWindow.Invoke(this));
        }

        public void AddGossipOption(GossipOption aOption)
        {
            gossipOptions.Add(aOption);
        }
    }
}
