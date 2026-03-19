namespace Project_1.Messaging.Events
{
    internal readonly struct EquipmentSlotChanged
    {
        public EquipmentSlotChanged(int ownerRenderId, RelationToPlayerKind ownerRelation, EquipmentSlotKind slot, ItemUiSnapshot itemSnapshot)
        {
            OwnerRenderId = ownerRenderId;
            OwnerRelation = ownerRelation;
            Slot = slot;
            ItemSnapshot = itemSnapshot;
        }

        public int OwnerRenderId { get; }
        public RelationToPlayerKind OwnerRelation { get; }
        public EquipmentSlotKind Slot { get; }
        public ItemUiSnapshot ItemSnapshot { get; }
    }

    internal readonly struct EquipmentSlotsRefreshed
    {
        public EquipmentSlotsRefreshed(int ownerRenderId, RelationToPlayerKind ownerRelation, ItemUiSnapshot[] itemSnapshots)
        {
            OwnerRenderId = ownerRenderId;
            OwnerRelation = ownerRelation;
            ItemSnapshots = itemSnapshots;
        }

        public int OwnerRenderId { get; }
        public RelationToPlayerKind OwnerRelation { get; }
        public ItemUiSnapshot[] ItemSnapshots { get; }
    }
}
