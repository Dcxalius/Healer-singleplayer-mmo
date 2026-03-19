using Project_1.Camera;

namespace Project_1.Messaging.Events
{
    internal readonly struct LootContext
    {
        public LootContext(int id, WorldSpace position, float allowedDistance, bool despawned)
        {
            Id = id;
            Position = position;
            AllowedDistance = allowedDistance;
            Despawned = despawned;
        }

        public int Id { get; }
        public WorldSpace Position { get; }
        public float AllowedDistance { get; }
        public bool Despawned { get; }
    }
}
