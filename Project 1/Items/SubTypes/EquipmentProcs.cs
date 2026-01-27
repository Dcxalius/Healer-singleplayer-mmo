using Newtonsoft.Json;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Spells;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Items.SubTypes
{
    internal class EquipmentProcs
    {
        static readonly Dictionary<CombatEventType, Action<EquipmentProcs>> subscriptionMap = new Dictionary<CombatEventType, Action<EquipmentProcs>>
        {
            { CombatEventType.Dodge, proc => proc.SubscribeWhenOwner<DodgeEvent>(e => e.Defender, e => e.Attacker) },
            { CombatEventType.Parry, proc => proc.SubscribeWhenOwner<ParryEvent>(e => e.Defender, e => e.Attacker) },
            { CombatEventType.Block, proc => proc.SubscribeWhenOwner<BlockEvent>(e => e.Defender, e => e.Attacker) },
            { CombatEventType.AttackDodged, proc => proc.SubscribeWhenOwner<AttackDodgedEvent>(e => e.Attacker, e => e.Defender) },
            { CombatEventType.AttackParried, proc => proc.SubscribeWhenOwner<AttackParriedEvent>(e => e.Attacker, e => e.Defender) },
            { CombatEventType.AttackBlocked, proc => proc.SubscribeWhenOwner<AttackBlockedEvent>(e => e.Attacker, e => e.Defender) },
            { CombatEventType.AttackMissed, proc => proc.SubscribeWhenOwner<AttackMissedEvent>(e => e.Attacker, e => e.Defender) },
            { CombatEventType.AttackGlanced, proc => proc.SubscribeWhenOwner<AttackGlancedEvent>(e => e.Attacker, e => e.Defender) },
            { CombatEventType.AttackCrit, proc => proc.SubscribeWhenOwner<AttackCritEvent>(e => e.Attacker, e => e.Defender) },
            { CombatEventType.AttackCrushed, proc => proc.SubscribeWhenOwner<AttackCrushedEvent>(e => e.Attacker, e => e.Defender) },
            { CombatEventType.AttackHit, proc => proc.SubscribeWhenOwner<AttackHitEvent>(e => e.Attacker, e => e.Defender) },
            { CombatEventType.MissedBy, proc => proc.SubscribeWhenOwner<MissedByEvent>(e => e.Defender, e => e.Attacker) },
            { CombatEventType.HitTaken, proc => proc.SubscribeWhenOwner<HitTakenEvent>(e => e.Defender, e => e.Attacker) },
            { CombatEventType.CritTaken, proc => proc.SubscribeWhenOwner<CritTakenEvent>(e => e.Defender, e => e.Attacker) },
            { CombatEventType.GlancingTaken, proc => proc.SubscribeWhenOwner<GlancingTakenEvent>(e => e.Defender, e => e.Attacker) },
            { CombatEventType.CrushingTaken, proc => proc.SubscribeWhenOwner<CrushingTakenEvent>(e => e.Defender, e => e.Attacker) },
            { CombatEventType.SpellHit, proc => proc.SubscribeWhenOwner<SpellHitEvent>(e => e.Caster, e => e.Target) },
            { CombatEventType.SpellCrit, proc => proc.SubscribeWhenOwner<SpellCritEvent>(e => e.Caster, e => e.Target) },
            { CombatEventType.SpellResist, proc => proc.SubscribeWhenOwner<SpellResistEvent>(e => e.Caster, e => e.Target) },
            { CombatEventType.SpellHitTaken, proc => proc.SubscribeWhenOwner<SpellHitTakenEvent>(e => e.Target, e => e.Caster) },
            { CombatEventType.SpellCritTaken, proc => proc.SubscribeWhenOwner<SpellCritTakenEvent>(e => e.Target, e => e.Caster) },
            { CombatEventType.SpellResistedByTarget, proc => proc.SubscribeWhenOwner<SpellResistedByTargetEvent>(e => e.Target, e => e.Caster) }
        };

        CombatEventType[] triggerEvent;
        double procChance;
        List<EquipmentProcEffect> effects;
        Entity owner;
        readonly List<IDisposable> subscriptions = new List<IDisposable>();

        [JsonConstructor]
        public EquipmentProcs(CombatEventType[] triggerEvent, double procChance, List<int> effectIds)
        {
            this.triggerEvent = triggerEvent;
            this.procChance = procChance;
            effects = new List<EquipmentProcEffect>();
            foreach (int id in effectIds)
            {
                var effect = ItemFactory.GetEquipmentProcEffect(id);
                Debug.Assert(effect != null, $"Effect with ID {id} not found in Factory.");
                effects.Add(effect);
            }
        }

        public bool TryTriggerProc(Entity source, Entity target)
        {
            double roll = RandomManager.RollDouble();
            if (roll <= procChance)
            {
                foreach (var effect in effects)
                {
                }
                return true;
            }
            return false;
        }

        public void Equip(Entity source)
        {
            Unequip();
            owner = source;
            if (owner == null) return;
            if (triggerEvent == null || triggerEvent.Length == 0) return;

            var uniqueEvents = new HashSet<CombatEventType>(triggerEvent);
            foreach (var combatEvent in uniqueEvents)
            {
                if (subscriptionMap.TryGetValue(combatEvent, out var subscribe))
                {
                    subscribe(this);
                }
            }
        }

        public void Unequip()
        {
            for (int i = 0; i < subscriptions.Count; i++)
            {
                subscriptions[i].Dispose();
            }
            subscriptions.Clear();
            owner = null;
        }

        void SubscribeWhenOwner<TEvent>(Func<TEvent, Entity> ownerSelector, Func<TEvent, Entity> otherSelector)
        {
            owner.Events.SubscribeTo<TEvent>(subscriptions, e =>
            {
                if (ownerSelector(e) != owner) return;
                TryTriggerProc(owner, otherSelector(e));
            });
        }
    }
}
