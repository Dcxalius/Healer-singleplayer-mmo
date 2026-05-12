using Project_1.GameObjects.Entities;
using Project_1.Managers;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.Textures;
using Project_1.UI.UIElements;
using Project_1.World.GameObjects.Spells.SpellEffects;
using Project_1.GameObjects.Unit.Stats;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Spells.Buff
{
    class Buff
    {
        static void AssertSimThread() => ThreadAffinity.AssertSimThread();

        public int Id => id;
        int id;
        static int buffIdCounter = 0;
        protected Entity caster;

        protected SpellEffect effect;
        public int EffectId => effect.Id;

        public virtual GfxPath GfxPath { get; }

        protected double createTime;

        public virtual double Duration => effect is LastingEffect lasting ? lasting.Duration : 0d;
        public virtual double DurationRemaining { get => Duration - (TimeManager.TotalFrameTime - createTime); }

        public bool IsOver { get => Duration > 0 && createTime + Duration < TimeManager.TotalFrameTime; }

        public bool MultipleSourceStackable => effect.sourceStackable;
        public int MaxStackCount => effect.MaxStackCount;
        public int Count => count;
        int count;
        public int Rank => rank;
        int rank;

        public double Power => power;
        double power;

        public double MovementSpeedModifier => effect is StatusEffect status ? status.StatusMovementSpeedModifier : effect.MovementSpeedModifier;
        public bool HasControl => effect is not StatusEffect status || status.HasControl;
        public bool HasStatModifiers => effect is StatusEffect status && status.HasStatModifiers;
        public StatusModifier[] StatusModifiers => effect is StatusEffect status ? status.StatModifiers : Array.Empty<StatusModifier>();
        public string[] StatusTags => effect is StatusEffect status ? status.StatusTags : Array.Empty<string>();
        public bool HasVisualOpacity => effect is StatusEffect status && status.HasVisualOpacity;
        public float VisualOpacity => effect is StatusEffect status ? status.VisualOpacity : 1f;
        public bool ModifiersScaleWithStacks => effect is not StatusEffect status || status.ModifiersScaleWithStacks;
        public string StackingCategory => effect is StatusEffect status && !string.IsNullOrWhiteSpace(status.StackingCategory)
            ? status.StackingCategory
            : effect.Name;
        public virtual bool IsDepleted => false;

        public BuffUiSnapshot BuffUiSnapshot => new BuffUiSnapshot(Id, GfxPath, TimeManager.InstanceTotalFrameTime + DurationRemaining, count, MaxStackCount);



        public Buff(Entity aCaster, SpellEffect aEffect, Spell aSpell)
        {
            AssertSimThread();
            id = buffIdCounter++;
            effect = aEffect;
            caster = aCaster;
            rank = aSpell.Rank;
            createTime = TimeManager.TotalFrameTime;
            power = aSpell.GetPower(aEffect);
            count = aEffect is StatusEffect status ? status.InitialStackCount : 1;
        }

        protected Buff(Entity aCaster, SpellEffect aEffect)
        {
            AssertSimThread();
            id = buffIdCounter++;
            effect = aEffect;
            caster = aCaster;
            rank = 0;
            createTime = TimeManager.TotalFrameTime;
            power = 0;
            count = 1;
        }

        public virtual void Recast(Buff aBuff)
        {
            AssertSimThread();
            if (MaxStackCount > count) count = Math.Min(MaxStackCount, Math.Max(count + 1, aBuff.Count));
            createTime = TimeManager.TotalFrameTime /*TODO: + a remaider of time so a tick is not lost*/;
                                                    //This is currently handled by periodic reseting tickcounter but that feels clunky, but if above mentioned remainder is added that shouldn't reset anymore
            caster = aBuff.caster;
            power = aBuff.power;
        }

        public virtual void Update(Entity aEntity)
        {
            AssertSimThread();

        }

        public virtual void OnApplied(Entity aOwner)
        {
            AssertSimThread();
        }

        public virtual void OnDispelled(Entity aOwner, Entity aDispeller)
        {
            AssertSimThread();
            MailboxManager.PublishUiEvent(new BuffRemoved(aOwner.RenderId, id));
        }

        public virtual void OnRemoved(Entity aOwner)
        {
            AssertSimThread();
            MailboxManager.PublishUiEvent(new BuffRemoved(aOwner.RenderId, id));
        }

        public virtual double AbsorbDamage(double incoming, DamageType damageType)
        {
            AssertSimThread();
            return 0;
        }

        public bool ConsumeStack()
        {
            AssertSimThread();
            if (count <= 0)
            {
                return true;
            }

            count--;
            return count <= 0;
        }

        public bool SameCaster(Entity aCaster) => caster == aCaster;
        public bool SameCaster(Buff aBuff) => caster == aBuff.caster;
        public static bool operator ==(Buff aBuff, Buff bBuff)
        {
            if (ReferenceEquals(aBuff, bBuff)) return true;
            if (aBuff is null || bBuff is null) return false;
            return aBuff.Equals(bBuff);
        }

        public static bool operator !=(Buff aBuff, Buff bBuff)
        {
            return !(aBuff == bBuff);

        }

        public override bool Equals(object obj) => obj is Buff other && Equals(other);

        public bool Equals(Buff aBuff)
        {
            if (aBuff is null) return false;
            
            return effect.Id == aBuff.effect.Id;
        }

        public bool IsSourceStackable(Buff aBuff)
        {
            if (!Equals(aBuff)) return false;
            if (MultipleSourceStackable && aBuff.caster != caster)
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode() => effect?.Id.GetHashCode() ?? 0;
    }
}
