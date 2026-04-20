namespace Project_1.Messaging.Events
{
    internal readonly struct EquipmentEnchantRequested
    {
        public EquipmentEnchantRequested(int equipmentSlot)
        {
            EquipmentSlot = equipmentSlot;
        }

        public int EquipmentSlot { get; }
    }
}
