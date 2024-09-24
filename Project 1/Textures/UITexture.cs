using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Textures
{
    internal class UITexture : Texture
    {

        public UITexture(string aPath, Color aColor) : base( new GfxPath(GfxType.UI, aPath), aColor)
        {

        }

    }
}
