using Project_1.Camera;
using Project_1.Textures;

namespace Project_1.UI.UIElements.Boxes
{
    internal class SquareImage : Square
    {
        GfxPath currentPath;

        public SquareImage(UIElement aParent, UITexture aGfx, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aParent, aGfx, aPos, aSize)
        {
            capturesClick = false;
            capturesRelease = false;
            capturesScroll = false;
        }

        public void SetImage(GfxPath aPath)
        {
            if (SamePath(currentPath, aPath)) return;
            currentPath = aPath;
            gfx.ChangeGfx(aPath);
            MarkRenderStale();
        }

        public void ClearImage()
        {
            if (SamePath(currentPath, GfxPath.NullPath)) return;
            currentPath = GfxPath.NullPath;
            gfx.ChangeGfx(GfxPath.NullPath);
            MarkRenderStale();
        }

        static bool SamePath(GfxPath left, GfxPath right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left == null || right == null) return false;
            return left.Type == right.Type && left.Name == right.Name;
        }
    }
}
