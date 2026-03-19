using Project_1.Camera;

namespace Project_1.Messaging.Events
{
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
}
