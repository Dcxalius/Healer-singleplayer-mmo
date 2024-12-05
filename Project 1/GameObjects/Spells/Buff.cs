using Project_1.GameObjects.Entities;
using Project_1.Managers;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Spells
{
    class Buff
    {
        protected Entity caster;

        protected SpellEffect effect;

        public virtual GfxPath GfxPath { get; }

        protected double createTime;

        public virtual double Duration { get; }
        public virtual double DurationRemaining { get => Duration - (TimeManager.TotalFrameTime - createTime); }

        public bool IsOver { get => createTime + Duration < TimeManager.TotalFrameTime; }



        public Buff(Entity aCaster, SpellEffect aEffet)
        {
            effect = aEffet;
            caster = aCaster;
            createTime = TimeManager.TotalFrameTime;
        }

        public virtual void Recast()
        {
            createTime = TimeManager.TotalFrameTime;
        }

        public virtual void Update(Entity aEntity)
        {

        }

        public static bool operator ==(Buff aBuff, Buff bBuff)
        {
            return aBuff.effect.Id == bBuff.effect.Id;
        }

        public static bool operator !=(Buff aBuff, Buff bBuff)
        {
            return aBuff.effect.Id != bBuff.effect.Id;

        }

        public override bool Equals(object obj)
        {
            if (obj is Buff)
            {
                Equals(obj as Buff);
            }

            return base.Equals(obj);
        }

        public bool Equals(Buff aBuff)
        {
            return effect.Id == aBuff.effect.Id;
        }
    }
}
