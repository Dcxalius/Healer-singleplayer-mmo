using Project_1.GameObjects.Spells;
using Project_1.GameObjects.Spells.Buff;
using Project_1.Managers;
using System.Collections.Generic;

namespace Project_1.GameObjects.Entities
{
    internal abstract partial class Entity
    {
        const double dodgeParryWindowMs = 5000;
        double lastDodgeOrParryTime = double.MinValue;

        // Guards against double-subscribing if multiple spells share the same condition.
        readonly HashSet<CastCondition> registeredConditions = new HashSet<CastCondition>();

        public bool InDodgeOrParryWindow =>
            lastDodgeOrParryTime + dodgeParryWindowMs >= TimeManager.TotalFrameTime;

        internal void ConsumeReactiveWindow()
        {
            ThreadAffinity.AssertSimThread();
            lastDodgeOrParryTime = double.MinValue;
            RemoveFirstBuffOfType<ReactiveBuff>();
        }

        // Called by SpellBook when a spell with a non-None CastCondition is learned.
        // Subscribes to the relevant combat events the first time each condition is registered.
        internal void RegisterReactiveHook(CastCondition condition)
        {
            ThreadAffinity.AssertSimThread();
            if (!registeredConditions.Add(condition)) return;

            switch (condition)
            {
                case CastCondition.AfterDodgeOrParry:
                    Events.Subscribe<DodgeEvent>(_ => OnDodgeOrParry());
                    Events.Subscribe<ParryEvent>(_ => OnDodgeOrParry());
                    break;
            }
        }

        void OnDodgeOrParry()
        {
            lastDodgeOrParryTime = TimeManager.TotalFrameTime;
            buffList.AddBuff(new ReactiveBuff(this), this);
        }
    }
}
