using Project_1.GameObjects.Unit.Resources;
using Project_1.GameObjects.Entities;
using System;
using Newtonsoft.Json;
using Project_1.World.Items.Enchantments;

namespace Project_1.Items.SubTypes
{
    class Consumable : Item
    {
        [JsonIgnore]
        protected ConsumableData ItemData { get => itemData as ConsumableData; }
        ConsumableData.ConsumableType ConsumableType { get => ItemData.Consumable; }
        float Value { get => ItemData.Value; }
        public int EnchantmentId => ItemData.EnchantmentId;
        public bool RequiresItemTarget => ItemData.RequiresItemTarget;

        public Consumable(LootData aLoot) : base(aLoot)
        {
        }

        [JsonConstructor]
        Consumable(int id, int count) : this(ItemFactory.GetItemData<ConsumableData>(id), count) { }

        public Consumable(ConsumableData aData, int aCount) : base(aData, aCount)
        {

        }

        public bool TryGetEnchantmentData(out EnchantmentData enchantmentData)
        {
            enchantmentData = null;
            if (!RequiresItemTarget) return false;

            enchantmentData = EnchantmentFactory.GetData(EnchantmentId);
            return enchantmentData != null;
        }

        public bool Use(Entity aUser)
        {
            switch (ConsumableType)
            {
                case ConsumableData.ConsumableType.NONE:
                    throw new NotImplementedException();
                case ConsumableData.ConsumableType.Heal:
                    if (!aUser.TakeHealing(aUser, Value)) return false;

                    return true;
                case ConsumableData.ConsumableType.Mana:
                    if (!aUser.ResourceGain(aUser, Value, Resource.ResourceType.Mana)) return false;

                    return true;
                case ConsumableData.ConsumableType.Energy:
                    if (!aUser.ResourceGain(aUser, Value, Resource.ResourceType.Energy)) return false;

                    return true;
                case ConsumableData.ConsumableType.Food:
                    throw new NotImplementedException();
                case ConsumableData.ConsumableType.Drink:
                    throw new NotImplementedException();
                case ConsumableData.ConsumableType.EnchantScroll:
                    return false;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
