using Project_1.Camera;
using Project_1.Textures;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Boxes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.LoadingMenu
{
    internal class ExistingSavesBox : Box, ScrollableBox
    {
        public ExistingSavesBox(UITexture aGfx, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aGfx, aPos, aSize)
        {
            SCroll
        }

        public List<UIElement> Children => throw new NotImplementedException();

        public RelativeScreenPosition ChildSize => throw new NotImplementedException();

        public RelativeScreenPosition Spacing => throw new NotImplementedException();

        public bool ToMuchForWindow => throw new NotImplementedException();

        public float ScrollValue { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public float[] OriginalYPos => throw new NotImplementedException();

        public float ScrollSpeed => throw new NotImplementedException();
    }
}
