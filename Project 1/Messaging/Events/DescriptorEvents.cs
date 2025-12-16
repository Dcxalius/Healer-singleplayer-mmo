using Project_1.Items;
using Project_1.UI.UIElements.SelectBoxes;

namespace Project_1.Messaging.Events
{
    internal readonly struct DescriptorBoxSet
    {
        public DescriptorBoxSet(Item item, Camera.AbsoluteScreenPosition? pos = null)
        {
            Item = item;
            Position = pos;
        }
        public Item Item { get; }
        public Camera.AbsoluteScreenPosition? Position { get; }
    }

    internal readonly struct DescriptorBoxClear
    {
    }
}
