using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Spells;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Entities.Projectiles
{
    internal class Projectile : MovingObject
    {
        Entity target;
        Entity caster;
        ProjectileData projectileData;
        Spell spell;

        public bool IsFinished { get => isFinished; }
        bool isFinished;
        public override float MaxSpeed => projectileData.MaxSpeed;

        public Projectile(Entity aCaster, WorldSpace aPos, ProjectileData aProjectileData, Spell aSpell, Entity aTarget) : base(new Texture(aProjectileData.GfxPath), aPos)
        {
            isFinished = false;

            caster = aCaster;
            target = aTarget;
            projectileData = aProjectileData;
            spell = aSpell;

            Debug.Assert(target != null);
            Debug.Assert(spell != null);

            WorldSpace dir = WorldSpace.Normalize(target.Centre - Centre);

            gfx.Rotation = (float)Math.Atan2(dir.Y, dir.X);
            Size = projectileData.Size;
            gfx.Offset = Size.ToVector2() / 2;

            ProjectileManager.AddProjectile(this);
        }

        public override void Update()
        {
            base.Update();

            UpdateFinishConditions();
        }

        void UpdateFinishConditions()
        {
            if (isFinished == true) return;

            if (Centre.DistanceTo(target.Centre) < Size.X / 2)
            {
                spell.Trigger(target, caster);
                isFinished = true;
            }

            if (target.Alive == false)
            {
                isFinished = true;
            }
        }

        protected override void SetMomentum()
        {
            WorldSpace dir = WorldSpace.Normalize(target.Centre - Centre);
            momentum = dir * MaxSpeed;
            gfx.Rotation = (float)Math.Atan2(dir.Y, dir.X);
        }

        protected override void FlipGfx()
        {
            //base.FlipGfx();
        }

    }
}
