using Project_1.GameObjects.Entities;
using Project_1.Managers;
using Project_1.Textures;
using Project_1.UI.UIElements;
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
        protected Entity caster;

        protected SpellEffect effect;
        public int EffectId => effect.Id;

        public virtual GfxPath GfxPath { get; }

        protected double createTime;

        public virtual double Duration { get; }
        public virtual double DurationRemaining { get => Duration - (TimeManager.TotalFrameTime - createTime); }

        public bool IsOver { get => createTime + Duration < TimeManager.TotalFrameTime; }

        public bool MultipleSourceStackable => effect.sourceStackable;
        public int MaxStackCount => effect.MaxStackCount;
        public int Count => count;
        int count;
        public int Rank => rank;
        int rank;

        public double Power => power;
        double power;

        public Buff(Entity aCaster, SpellEffect aEffect, int aRank)
        {
            AssertSimThread();
            effect = aEffect;
            caster = aCaster;
            rank = aRank;
            createTime = TimeManager.TotalFrameTime;
        }

        public virtual void Recast(Entity aCaster)
        {
            AssertSimThread();
            createTime = TimeManager.TotalFrameTime;
        }

        public virtual void Update(Entity aEntity)
        {
            AssertSimThread();

        }

        public virtual void OnApplied(Entity aOwner)
        {
            AssertSimThread();
        }

        public virtual void OnRemoved(Entity aOwner)
        {
            AssertSimThread();
        }

        public bool SameCaster(Entity aCaster) => caster == aCaster;

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
