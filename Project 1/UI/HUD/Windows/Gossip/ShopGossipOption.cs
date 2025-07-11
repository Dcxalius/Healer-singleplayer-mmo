using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.HUD.Windows.Gossip
{
    internal class ShopGossipOption : GossipOption
    {
        public static Action<ShopGossipOption> CloseGossipWindowAndOpenShop;
        public int[] ItemIDsInShop => itemIDsInShop;
        int[] itemIDsInShop;
        public ShopGossipOption(string aDescriptor, string aData) : base(aDescriptor)
        {
            string[] splitData = aData.Split(',');
            itemIDsInShop = new int[splitData.Length];

            for (int i = 0; i < splitData.Length; i++)
            {
                itemIDsInShop[i] = int.Parse(splitData[i]);
            }
            Actions.Add(() => CloseGossipWindowAndOpenShop.Invoke(this));

        }
    }
}
