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

    internal readonly struct WorldReleaseRequested
    {
        public WorldReleaseRequested(RelativeScreenPosition relativePos, ClickKind button, byte modifiersMask)
        {
            RelativePos = relativePos;
            Button = button;
            ModifiersMask = modifiersMask;
        }

        public RelativeScreenPosition RelativePos { get; }
        public ClickKind Button { get; }
        public byte ModifiersMask { get; }

        public static WorldReleaseRequested FromReleaseEvent(ReleaseEvent releaseEvent)
        {
            return new WorldReleaseRequested(releaseEvent.RelativePos, releaseEvent.ButtonPressed.ToClickKind(), releaseEvent.ModifierMask);
        }
    }

    internal readonly struct WorldScrollRequested
    {
        public WorldScrollRequested(RelativeScreenPosition relativePos, int steps, bool up, byte modifiersMask)
        {
            RelativePos = relativePos;
            Steps = steps;
            Up = up;
            ModifiersMask = modifiersMask;
        }

        public RelativeScreenPosition RelativePos { get; }
        public int Steps { get; }
        public bool Up { get; }
        public byte ModifiersMask { get; }

        public bool Down => !Up;

        public static WorldScrollRequested FromScrollEvent(ScrollEvent scrollEvent)
        {
            return new WorldScrollRequested(scrollEvent.RelativePos, scrollEvent.Steps, scrollEvent.Up, scrollEvent.ModifierMask);
        }
    }

    internal readonly struct PlayerMovementRequested
    {
        public PlayerMovementRequested(bool left, bool right, bool up, bool down)
        {
            Left = left;
            Right = right;
            Up = up;
            Down = down;
        }

        public bool Left { get; }
        public bool Right { get; }
        public bool Up { get; }
        public bool Down { get; }
    }

    internal readonly struct MoveOrderRequested
    {
        public MoveOrderRequested(WorldSpace destination, bool append)
        {
            Destination = destination;
            Append = append;
        }

        public WorldSpace Destination { get; }
        public bool Append { get; }
    }

    internal readonly struct PartyTargetOrderRequested
    {
        public PartyTargetOrderRequested(int targetRenderId)
        {
            TargetRenderId = targetRenderId;
        }

        public int TargetRenderId { get; }
    }

    internal readonly struct TargetClearedRequested
    {
    }

    internal enum PartyCommandAction
    {
        Clear,
        Add,
        NeedyAdd
    }

    internal readonly struct PartyCommandRequested
    {
        public PartyCommandRequested(PartyCommandAction action, int? memberRenderId)
        {
            Action = action;
            MemberRenderId = memberRenderId;
        }

        public PartyCommandAction Action { get; }
        public int? MemberRenderId { get; }
    }

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

    /// <summary>
    /// Published to sim only after UI had a chance to consume Escape.
    /// </summary>
    internal readonly struct EscapeRequested
    {
    }
}
