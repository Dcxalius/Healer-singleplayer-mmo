namespace Project_1.Messaging.Events
{
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
}
