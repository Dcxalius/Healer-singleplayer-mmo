using Project_1.GameObjects.Unit.Stats;

namespace Project_1.GameObjects.Entities
{
    internal abstract partial class Entity
    {
        public EntityEventBus Events { get; } = new EntityEventBus();

    }
}
