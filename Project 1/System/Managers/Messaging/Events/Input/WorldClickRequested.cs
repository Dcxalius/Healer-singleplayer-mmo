using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.Input;

namespace Project_1.Messaging.Events
{
    internal readonly struct WorldClickRequested
    {
        public WorldClickRequested(RelativeScreenPosition relativePos, ClickKind button, byte modifiersMask)
        {
            RelativePos = relativePos;
            Button = button;
            ModifiersMask = modifiersMask;
        }

        public RelativeScreenPosition RelativePos { get; }
        public ClickKind Button { get; }
        public byte ModifiersMask { get; }

        public bool NoModifiers() => ModifiersMask == 0;
        public bool Modifier(InputManager.HoldModifier modifier) => (ModifiersMask & (1 << (int)modifier)) != 0;

        public static WorldClickRequested FromClickEvent(ClickEvent clickEvent)
        {
            return new WorldClickRequested(clickEvent.RelativePos, clickEvent.ButtonPressed.ToClickKind(), clickEvent.ModifierMask);
        }
    }
}
