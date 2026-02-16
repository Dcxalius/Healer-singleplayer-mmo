using Microsoft.Xna.Framework;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Unit.Stats;
using System;
using System.Collections.Generic;

namespace Project_1.GameObjects.Unit.Resources
{
    internal class Rage : Resource
    {
        const float maxRage = 100f;
        const float rageDecayOutOfCombatPerTick = 3f;
        const float ragePerAttackSecond = 2f;
        const float flatRageOnAttackHit = 1f;
        const float rageFromDamageTakenPercentScale = 100f;

        readonly List<IDisposable> subscriptions = new List<IDisposable>();
        Entity owner;

        public override float Value
        {
            get => rageValue;
            set => rageValue = Math.Clamp(value, 0f, maxValue);
        }
        float rageValue;

        public override float MaxValue
        {
            get => maxValue;
            protected set
            {
                maxValue = value;
                if (rageValue > maxValue)
                {
                    rageValue = maxValue;
                }
            }
        }
        float maxValue;

        public override float RegenValue => 0f;

        protected override float BaseMaxValue => maxRage;
        protected override float PerLevel => 0f;

        public Rage(float aCurrentResource) : base(ResourceType.Rage, Color.Red)
        {
            maxValue = maxRage;
            Value = aCurrentResource;
        }

        public override void SetOwner(Entity aOwner)
        {
            ClearSubscriptions();
            owner = aOwner;
            if (owner == null) return;

            owner.Events.SubscribeTo<AttackHitEvent>(subscriptions, OnAttackHit);
            owner.Events.SubscribeTo<HitTakenEvent>(subscriptions, OnHitTaken);
        }

        void OnAttackHit(AttackHitEvent aEvent)
        {
            if (aEvent.Attacker != owner || aEvent.Attack == null) return;
            GainRage(flatRageOnAttackHit + aEvent.Attack.SecondsPerAttack * ragePerAttackSecond);
        }

        void OnHitTaken(HitTakenEvent aEvent)
        {
            if (aEvent.Defender != owner || owner.MaxHealth <= 0) return;

            float damagePercentOfMaxHealth = (float)Math.Max(0.0, aEvent.Damage.Sum / owner.MaxHealth);
            if (damagePercentOfMaxHealth <= 0f) return;

            GainRage(damagePercentOfMaxHealth * rageFromDamageTakenPercentScale);
        }

        void GainRage(float aAmount)
        {
            if (aAmount <= 0f) return;
            Value += aAmount;
        }

        public override void TickRegen(bool aInCombat)
        {
            if (aInCombat) return;
            Value -= rageDecayOutOfCombatPerTick;
        }

        public override void Update() { }

        public override void Refresh(TotalPrimaryStats aStats) { }

        void ClearSubscriptions()
        {
            for (int i = 0; i < subscriptions.Count; i++)
            {
                subscriptions[i].Dispose();
            }
            subscriptions.Clear();
        }
    }
}
