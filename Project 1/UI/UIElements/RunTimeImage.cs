using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Textures;
using Project_1.UI.UIElements.Boxes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements
{
    internal class RuntimeImage : Box
    {
        public RuntimeImage(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(null, aPos, aSize)
        {
        }
        public RuntimeImage(string aPath, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(null, aPos, aSize)
        {
            SetImage(aPath);
        }

        public void SetImage(string aPath)
        {
            gfx = new AssignableImage(aPath);
        }

        public void Clear()
        {
            gfx = null;
        }
    }
}
