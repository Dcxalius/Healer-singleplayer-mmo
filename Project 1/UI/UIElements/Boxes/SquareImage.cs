using Project_1.Camera;
using Project_1.Textures;

namespace Project_1.UI.UIElements.Boxes
{
    internal class SquareImage : Square
    {
        public SquareImage(UIElement aParent, UITexture aGfx, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aParent, aGfx, aPos, aSize)
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
