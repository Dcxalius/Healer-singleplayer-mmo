using Project_1.GameObjects.Unit.Stats;

namespace Project_1.GameObjects.Entities
{
    internal abstract partial class Entity
    {
        //TODO: This seems a bit to thin, move all event related things into here
        public EntityEventBus Events { get; } = new EntityEventBus();

    }
}
