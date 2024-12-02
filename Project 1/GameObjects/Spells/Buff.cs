using Project_1.GameObjects.Entities;
using Project_1.Managers;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Spells
{
    class Buff
    {
        protected Entity caster;
        
        public virtual GfxPath GfxPath { get; }

        protected double createTime;

        public virtual double Duration { get; }

        public bool IsOver { get => createTime + Duration < TimeManager.TotalFrameTime; }

        public Buff(Entity aCaster)
        {
            caster = aCaster;
            createTime = TimeManager.TotalFrameTime;
        }

        public virtual void Update(Entity aEntity)
        {

        }

        //public override bool Trigger(Entity aCaster, Entity aTarget)
        //{
        //    createTime = TimeManager.TotalFrameTime;
        //    caster = aCaster;
        //}
    }
}
