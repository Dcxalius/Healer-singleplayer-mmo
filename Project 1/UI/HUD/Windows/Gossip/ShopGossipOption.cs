using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.HUD.Windows.Gossip
{
    internal class ShopGossipOption : GossipOption
    {
        public ShopGossipOption(string aDescriptor, string aData) : base(aDescriptor)
        {
            string[] splitData = aData.Split(',');
            int[] parsedData = new int[splitData.Length];

            for (int i = 0; i < splitData.Length; i++)
            {
                parsedData[i] = int.Parse(splitData[i]);
            }
        }
    }
}
