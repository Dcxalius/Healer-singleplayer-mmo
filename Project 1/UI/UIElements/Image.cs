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
    internal class Image : Box
    {
        public Image(UITexture aGfx, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aGfx, aPos, aSize)
        {
            capturesClick = false;
            capturesRelease = false;
            capturesScroll = false;
        }

        public void SetImage(GfxPath aPath)
        {
            gfx.ChangeGfx(aPath);
        }
        public void ClearImage()
        {
            gfx.ChangeGfx(GfxPath.NullPath);
        }
    }
}
