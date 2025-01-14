using Project_1.GameObjects.Unit.Resources;
using Project_1.GameObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Items.SubTypes
{
    class Consumable : Item
    {
        protected ConsumableData ItemData { get => itemData as ConsumableData; }
        ConsumableData.ConsumableType ConsumableType { get => ItemData.Consumable; }
        float Value { get => ItemData.Value; }

        public Consumable(LootData aLoot) : base(aLoot)
        {
        }

        public Consumable(ConsumableData aData, int aCount) : base(aData, aCount)
        {

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
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
