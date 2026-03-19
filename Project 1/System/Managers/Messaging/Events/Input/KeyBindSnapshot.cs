namespace Project_1.Messaging.Events
{
    internal readonly struct KeyBindSnapshot
    {
        public KeyBindSnapshot(ulong pressedMask, ulong heldMask, ulong releasedMask)
        {
            PressedMask = pressedMask;
            HeldMask = heldMask;
            ReleasedMask = releasedMask;
        }

        public ulong PressedMask { get; }
        public ulong HeldMask { get; }
        public ulong ReleasedMask { get; }
    }
}
