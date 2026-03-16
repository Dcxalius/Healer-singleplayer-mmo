using Microsoft.Xna.Framework;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Unit.Stats;
using Project_1.Managers;
using System;
using System.Collections.Generic;

namespace Project_1.GameObjects.Unit.Resources
{
    internal class Rage : Resource
    {
        static void AssertSimThread() => ThreadAffinity.AssertSimThread();

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
            AssertSimThread();
            ClearSubscriptions();
            owner = aOwner;
            if (owner == null) return;

            owner.Events.SubscribeTo<AttackHitEvent>(subscriptions, OnAttackHit);
            owner.Events.SubscribeTo<HitTakenEvent>(subscriptions, OnHitTaken);
        }

        void OnAttackHit(AttackHitEvent aEvent)
        {
            AssertSimThread();
            if (aEvent.Attacker != owner || aEvent.Attack == null) return;
            GainRage(flatRageOnAttackHit + aEvent.Attack.SecondsPerAttack * ragePerAttackSecond);
        }

        void OnHitTaken(HitTakenEvent aEvent)
        {
            AssertSimThread();
            if (aEvent.Defender != owner || owner.MaxHealth <= 0) return;

            float damagePercentOfMaxHealth = (float)Math.Max(0.0, aEvent.Damage.Sum / owner.MaxHealth);
            if (damagePercentOfMaxHealth <= 0f) return;

            GainRage(damagePercentOfMaxHealth * rageFromDamageTakenPercentScale);
        }

        void GainRage(float aAmount)
        {
            AssertSimThread();
            if (aAmount <= 0f) return;
            Value += aAmount;
        }

        public override void TickRegen(bool aInCombat)
        {
            AssertSimThread();
            if (aInCombat) return;
            Value -= rageDecayOutOfCombatPerTick;
        }

        public override void Update()
        {
            AssertSimThread();
        }

        public override void Refresh(TotalPrimaryStats aStats)
        {
            AssertSimThread();
        }

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
