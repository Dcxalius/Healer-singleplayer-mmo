using Newtonsoft.Json;
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

        public string[][] Options => options;
        public int[][] LinkTree => linkTree;
        public int StartIndex => 0;
        string[][] options;
        int[][] linkTree;
        

        [JsonConstructor]
        GossipData(string[][] gossipOptions, int[][] linkTree) //TODO: Think if these two should be merge to a tuple with string, custom class for type, custom class for data, int[] for links
        {
            Asserts(gossipOptions, linkTree);
            options = gossipOptions;
            this.linkTree = linkTree;
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

    }
}
