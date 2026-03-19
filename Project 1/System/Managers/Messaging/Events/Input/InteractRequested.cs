using Project_1.GameObjects;

namespace Project_1.Messaging.Events
{
    internal readonly struct InteractRequested
    {
        public InteractRequested(int targetRenderId, ClickKind button)
        {
            TargetRenderId = targetRenderId;
            Button = button;
        }

        public int TargetRenderId { get; }
        public ClickKind Button { get; }
    }
}
