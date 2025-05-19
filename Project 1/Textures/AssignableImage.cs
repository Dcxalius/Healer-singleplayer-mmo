using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Textures
{
    internal class AssignableImage : UITexture
    {
        public AssignableImage() : base(GfxPath.NullPath, Color.White)
        {

        }
        public AssignableImage(string aPath) : base(GfxPath.NullPath, Color.White)
        {
            NewImage(aPath);
        }

        public void NewImage(string aPath)
        {
            if (!File.Exists(aPath))
            {
                //TODO: FNF gfx
                return;
            }
            gfx = GraphicsManager.CreateTextureFromFile(aPath);
        }
    }
}
