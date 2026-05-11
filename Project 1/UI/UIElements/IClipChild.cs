using Project_1.Camera;
using Project_1.Textures;

namespace Project_1.UI.UIElements
{
    internal interface IClipChild
    {
    }

    internal abstract class ClipChild : UIElement, IClipChild
    {
        protected ClipChild(UIElement parent, UITexture gfx, RelativeScreenPosition pos, RelativeScreenPosition size)
            : base(parent, gfx, pos, size)
        {
        }
    }
}
