using Newtonsoft.Json;
using Project_1.UI.HUD.Windows.Gossip;
using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Entities.Npcs
{
    internal class GossipData
    {
        enum optionsContext
        {
            Type,
            GossipHeader,
            Data
        }

        public ChatGossipOption Start => gossipOptions[0] as ChatGossipOption;
        private GossipOption[] gossipOptions;
        

        [JsonConstructor]
        GossipData(string[][] gossipOptions, int[][] linkTree) //TODO: Think if these two should be merge to a tuple with string, custom class for type, custom class for data, int[] for links
        {
            Asserts(gossipOptions, linkTree);
            CreateGossipObjects(gossipOptions);
            LinkGossipObjects(linkTree);
        }
        
        void Asserts(string[][] aOptions, int[][] aLinkTree)
        {
            Debug.Assert(aOptions[0][(int)optionsContext.Type] == "C");
            Debug.Assert(aOptions.Length == aLinkTree.Length);

            int d = 1;

            for (int i = 0; i < aLinkTree.Length; i++)
            {
                if (aLinkTree[i].Contains(d))
                {
                    d++;
                    i = -1;
                }
            }

            Debug.Assert(aLinkTree.Length == d, "Either not enough links or to many links");
            for (int i = 0; i < aLinkTree.Length; i++)
            {
                Debug.Assert(aOptions[i][(int)optionsContext.Type] == "C" || (aOptions[i][(int)optionsContext.Type] != "C" && aLinkTree[i].Length == 0));
            }
        }

        void CreateGossipObjects(string[][] aOptions)
        {
            gossipOptions = new GossipOption[aOptions.Length];
            for (int i = 0; i < aOptions.Length; i++)
            {
                GossipOption go;

                switch (aOptions[i][(int)optionsContext.Type])
                {
                    case "C":
                        //Chat
                        go = new ChatGossipOption(aOptions[i][(int)optionsContext.GossipHeader], aOptions[i][(int)optionsContext.Data]);
                        break;
                    case "S":
                        //Shop
                        go = new ShopGossipOption(aOptions[i][(int)optionsContext.GossipHeader], aOptions[i][(int)optionsContext.Data]);
                        break;
                    default:
                        throw new NotImplementedException();
                }

                gossipOptions[i] = go;
            }
        }

        void LinkGossipObjects(int[][] aLinkTree)
        {
            for (int i = 0; i < aLinkTree.Length; i++)
            {
                if (gossipOptions[i] is not ChatGossipOption) continue;
                for (int j = 0; j < aLinkTree[i].Length; j++)
                {
                    (gossipOptions[i] as ChatGossipOption).AddGossipOption(gossipOptions[aLinkTree[i][j]]);
                }
            }
        }
    }
}
