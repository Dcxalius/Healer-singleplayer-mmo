using Project_1.GameObjects.Entities;
using Project_1.Items.SubTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.World.Items.Enchantments
{
    internal class DynamicEnchantData : EnchantmentData
    {
        //Has a chance to do thing on trigger

        public enum Trigger
        {
            OnHit,
            OnCrit,
            OnBeingHit,
            OnKill
        }

        Entity owner;

        EnchantmentEffect enchantmentEffect;
        double triggerChance;

        public DynamicEnchantData(
            Entity owner,
            int id,
            string name,
            Trigger trigger,
            double triggerChance,
            string description = null,
            int[] primaryStats = null,
            SecondayStatBonus<int>[] secondaryStatsInt = null,
            SecondayStatBonus<float>[] secondaryStatsFloat = null
            ) : base(id, name, description, primaryStats, secondaryStatsInt, secondaryStatsFloat)
        {
            this.triggerChance = triggerChance;
            switch (trigger)
            {
                case DynamicEnchantData.Trigger.OnHit:
                    owner.Events.Subscribe<AttackHitEvent>(enchantmentEffect.TriggerAttackHit);
                    break;
                case DynamicEnchantData.Trigger.OnCrit:
                    owner.Events.Subscribe<AttackCritEvent>(enchantmentEffect.TriggerAttackCrit);
                    break;
                case DynamicEnchantData.Trigger.OnBeingHit:
                    owner.Events.Subscribe<HitTakenEvent>(enchantmentEffect.TriggerBeingHit);
                    break;
                case DynamicEnchantData.Trigger.OnKill:
                    owner.Events.Subscribe<KillEvent>(enchantmentEffect.TriggerKill);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(trigger), trigger, null);
            }
        }

        public void a()
        {

        }
    }
}
