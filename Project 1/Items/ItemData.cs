using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Textures;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Items
{
    internal class ItemData
    {
        public int ID { get => id; }
        static int Id { get => nextId++; }
        static int nextId = 0;
        int id;
        string name;
        string description;

        public int MaxStack { get => maxStack; }
        int maxStack;

        UITexture gfx;

        public ItemData(UITexture aTexture, string aName, string aDescription, int aMaxStack)
        {
            id = Id;
            gfx = aTexture;
            name = aName;
            description = aDescription;
            maxStack = aMaxStack;
        }

        public void Draw(SpriteBatch aBatch, Rectangle aPos)
        {
            gfx.Draw(aBatch, aPos);
        }
    }
}
