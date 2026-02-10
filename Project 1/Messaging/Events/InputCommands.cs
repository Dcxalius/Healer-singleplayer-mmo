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
        public bool ModifiersOr(InputManager.HoldModifier[] modifiers)
        {
            for (int i = 0; i < modifiers.Length; i++)
            {
                if (Modifier(modifiers[i])) return true;
            }
            return false;
        }

        public bool[] ToModifiersArray() => ToModifiersArray(ModifiersMask);

        public static WorldClickRequested FromClickEvent(ClickEvent clickEvent)
        {
            return new WorldClickRequested(clickEvent.RelativePos, clickEvent.ButtonPressed.ToClickKind(), BuildModifiersMask(clickEvent.ModifiersSnapshot));
        }

        static byte BuildModifiersMask(bool[] modifiers)
        {
            byte mask = 0;
            if (modifiers == null) return mask;
            if (modifiers.Length > (int)InputManager.HoldModifier.Ctrl && modifiers[(int)InputManager.HoldModifier.Ctrl]) mask |= (byte)(1 << (int)InputManager.HoldModifier.Ctrl);
            if (modifiers.Length > (int)InputManager.HoldModifier.Alt && modifiers[(int)InputManager.HoldModifier.Alt]) mask |= (byte)(1 << (int)InputManager.HoldModifier.Alt);
            if (modifiers.Length > (int)InputManager.HoldModifier.Shift && modifiers[(int)InputManager.HoldModifier.Shift]) mask |= (byte)(1 << (int)InputManager.HoldModifier.Shift);
            return mask;
        }

        static bool[] ToModifiersArray(byte mask)
        {
            bool[] modifiers = new bool[(int)InputManager.HoldModifier.Count];
            modifiers[(int)InputManager.HoldModifier.Ctrl] = (mask & (1 << (int)InputManager.HoldModifier.Ctrl)) != 0;
            modifiers[(int)InputManager.HoldModifier.Alt] = (mask & (1 << (int)InputManager.HoldModifier.Alt)) != 0;
            modifiers[(int)InputManager.HoldModifier.Shift] = (mask & (1 << (int)InputManager.HoldModifier.Shift)) != 0;
            return modifiers;
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

        public bool[] ToModifiersArray()
        {
            bool[] modifiers = new bool[(int)InputManager.HoldModifier.Count];
            modifiers[(int)InputManager.HoldModifier.Ctrl] = (ModifiersMask & (1 << (int)InputManager.HoldModifier.Ctrl)) != 0;
            modifiers[(int)InputManager.HoldModifier.Alt] = (ModifiersMask & (1 << (int)InputManager.HoldModifier.Alt)) != 0;
            modifiers[(int)InputManager.HoldModifier.Shift] = (ModifiersMask & (1 << (int)InputManager.HoldModifier.Shift)) != 0;
            return modifiers;
        }

        public static WorldReleaseRequested FromReleaseEvent(ReleaseEvent releaseEvent)
        {
            return new WorldReleaseRequested(releaseEvent.RelativePos, releaseEvent.ButtonPressed.ToClickKind(), BuildModifiersMask(releaseEvent));
        }

        static byte BuildModifiersMask(ReleaseEvent releaseEvent)
        {
            byte mask = 0;
            if (releaseEvent.Modifier(InputManager.HoldModifier.Ctrl)) mask |= (byte)(1 << (int)InputManager.HoldModifier.Ctrl);
            if (releaseEvent.Modifier(InputManager.HoldModifier.Alt)) mask |= (byte)(1 << (int)InputManager.HoldModifier.Alt);
            if (releaseEvent.Modifier(InputManager.HoldModifier.Shift)) mask |= (byte)(1 << (int)InputManager.HoldModifier.Shift);
            return mask;
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

        public bool[] ToModifiersArray()
        {
            bool[] modifiers = new bool[(int)InputManager.HoldModifier.Count];
            modifiers[(int)InputManager.HoldModifier.Ctrl] = (ModifiersMask & (1 << (int)InputManager.HoldModifier.Ctrl)) != 0;
            modifiers[(int)InputManager.HoldModifier.Alt] = (ModifiersMask & (1 << (int)InputManager.HoldModifier.Alt)) != 0;
            modifiers[(int)InputManager.HoldModifier.Shift] = (ModifiersMask & (1 << (int)InputManager.HoldModifier.Shift)) != 0;
            return modifiers;
        }

        public static WorldScrollRequested FromScrollEvent(ScrollEvent scrollEvent)
        {
            return new WorldScrollRequested(scrollEvent.RelativePos, scrollEvent.Steps, scrollEvent.Up, BuildModifiersMask(scrollEvent));
        }

        static byte BuildModifiersMask(ScrollEvent scrollEvent)
        {
            byte mask = 0;
            if (scrollEvent.Modifier(InputManager.HoldModifier.Ctrl)) mask |= (byte)(1 << (int)InputManager.HoldModifier.Ctrl);
            if (scrollEvent.Modifier(InputManager.HoldModifier.Alt)) mask |= (byte)(1 << (int)InputManager.HoldModifier.Alt);
            if (scrollEvent.Modifier(InputManager.HoldModifier.Shift)) mask |= (byte)(1 << (int)InputManager.HoldModifier.Shift);
            return mask;
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
}
