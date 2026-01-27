using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.GameObjects.Entities;
using Project_1.Input;
using Project_1.GameObjects.Entities.Friendlies.GuildMembers;

namespace Project_1.Messaging.Events
{
    internal readonly struct WorldClickRequested
    {
        public WorldClickRequested(ClickEvent clickEvent)
        {
            ClickEvent = clickEvent;
        }

        public ClickEvent ClickEvent { get; }
    }

    internal readonly struct WorldReleaseRequested
    {
        public WorldReleaseRequested(ReleaseEvent releaseEvent)
        {
            ReleaseEvent = releaseEvent;
        }

        public ReleaseEvent ReleaseEvent { get; }
    }

    internal readonly struct WorldScrollRequested
    {
        public WorldScrollRequested(ScrollEvent scrollEvent)
        {
            ScrollEvent = scrollEvent;
        }

        public ScrollEvent ScrollEvent { get; }
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
        public PartyTargetOrderRequested(Entity target)
        {
            Target = target;
        }

        public Entity Target { get; }
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
        public PartyCommandRequested(PartyCommandAction action, GuildMember member)
        {
            Action = action;
            Member = member;
        }

        public PartyCommandAction Action { get; }
        public GuildMember Member { get; }
    }

    internal readonly struct InteractRequested
    {
        public InteractRequested(WorldObject target, InputManager.ClickType button)
        {
            Target = target;
            Button = button;
        }

        public WorldObject Target { get; }
        public InputManager.ClickType Button { get; }
    }
}
