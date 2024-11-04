using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Project_1.Textures;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Items
{
    internal struct ItemData
    {
        public int ID { get => id; /*set => id = value;*/ }
        int id;
        static int Id { get => nextId++; }
        static int nextId = 0;
        public string Name { get => name; }
        string name;
        string description;

        public int MaxStack { get => maxStack; }
        int maxStack;

        public GfxPath GfxPath { get => gfx; }
        GfxPath gfx;

        [JsonConstructor]
        public ItemData(int id, string gfxName, string name, string description, int maxStack)
        {
            this.id = Id;
            gfx = new GfxPath(GfxType.Item, gfxName);
            this.name = name;
            this.description = description;
            this.maxStack = maxStack;
        }

        void Assert()
        {
            Debug.Assert(name != null && description != null && maxStack > 0, "Itemdata not properly set");
        }

        //public void Draw(SpriteBatch aBatch, Rectangle aPos)
        //{
        //    gfx.Draw(aBatch, aPos);
        //}
    }
}
