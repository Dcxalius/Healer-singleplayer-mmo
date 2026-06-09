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
        string currentPath;

        public RuntimeImage(UIElement aParent, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aParent, null, aPos, aSize)
        {
        }
        public RuntimeImage(UIElement aParent, string aPath, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aParent, null, aPos, aSize)
        {
            SetImage(aPath);
        }

        public void SetImage(string aPath)
        {
            if (currentPath == aPath) return;
            currentPath = aPath;
            gfx = new AssignableImage(aPath);
            MarkRenderStale();
        }

        public void Clear()
        {
            if (currentPath == null && gfx == null) return;
            currentPath = null;
            gfx = null;
            MarkRenderStale();
        }
    }
}
