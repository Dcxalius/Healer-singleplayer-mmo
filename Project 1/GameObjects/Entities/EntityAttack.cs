using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects.Entities.Corspes;
using Project_1.GameObjects.Unit;
using Project_1.GameObjects.Unit.Stats;
using Project_1.Managers;
using Project_1.UI.HUD.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Entities
{
    internal partial class Entity
    {
        public bool Selected => ObjectManager.Player.Target == this;
        public virtual Entity Target { get => target; }
        protected Entity target = null;
        public virtual bool InCombat => aggroTablesIAmOn.Count > 0;
        List<NonFriendly> aggroTablesIAmOn;

        float timeSinceLastAttack = 0;
        protected abstract bool CheckForRelation();

        public void SetTarget(Entity aEntity)
        {
            target = aEntity;
            HUDManager.plateBoxHandler.SetNewTarget(this, target);
        }

        public void RemoveTarget()
        {
            target = null;
            HUDManager.plateBoxHandler.SetNewTarget(this, null);
        }


        float GetMinAttackRange()
        {
            float minAttackRange;
            if (unitData.AttackData.OffHandAttack != null && unitData.AttackData.MainHandAttack != null)
            {
                minAttackRange = Math.Min(unitData.AttackData.MainHandAttack.Range, unitData.AttackData.OffHandAttack.Range);
            }
            else if (unitData.AttackData.MainHandAttack == null)
            {
                minAttackRange = unitData.AttackData.OffHandAttack.Range;
            }
            else
            {
                minAttackRange = unitData.AttackData.MainHandAttack.Range;
            }
            return minAttackRange;
        }


        void AttackTarget()
        {
            if (target == null) return;
            if (!CheckForRelation()) return;

            AttackData a = unitData.AttackData;
            if ((target.FeetPosition - FeetPosition).ToVector2().Length() >= GetMinAttackRange() - Size.X / 2 - target.Size.X / 2) return;

            CheckAttackSpeed(ref unitData.NextAvailableMainHandAttack, a.MainHandAttack);
            if (target == null) return;
            CheckAttackSpeed(ref unitData.NextAvailableOffHandAttack, a.OffHandAttack);
        }

        void CheckAttackSpeed(ref TimeSpan aLockoutTime, Unit.Attack aAttack)
        {
            if (aAttack == null) return;
            if (!CheckIfInRange(aAttack)) return;
            if (aLockoutTime > TimeManager.TotalFrameTimeAsTimeSpan) return;

            aLockoutTime = TimeManager.TotalFrameTimeAsTimeSpan + TimeSpan.FromSeconds(aAttack.SecondsPerAttack);
            HitTarget(aAttack);
        }

        bool CheckIfInRange(Unit.Attack aAttack) => aAttack.Range <= (target.FeetPosition - FeetPosition).ToVector2().Length();

        void HitTarget(Unit.Attack aAttack)
        {
            HitTable.HitResult hitResult = HitTable.GenerateTable(aAttack, this, target);

            //TODO: Proc onhits,
            Damage damage;

            if (hitResult == HitTable.HitResult.Miss || hitResult == HitTable.HitResult.Dodge)
            {
                damage = new Damage(new double[] { 0 }, new DamageType[] { DamageType.True });
            }
            else
            {
                //Check if eq/talents/skills/buffs/spells procs
                damage = new Damage(new double[] { aAttack.GetAttackDamage }, new DamageType[] { DamageType.Physical });
            }
            target.RecieveAttack(hitResult, this, aAttack, damage);
            TargetAliveCheck();
        }

        public void AddedToAggroTable(NonFriendly aNonfriendly)
        {
            if (aggroTablesIAmOn.Contains(aNonfriendly))
            {
                DebugManager.Print(GetType(), aNonfriendly + " tried to add me to a table I thought I was on.");
                return;
            }
            aggroTablesIAmOn.Add(aNonfriendly);
        }

        public void RemovedFromAggroTable(NonFriendly aNonfriendly)
        {
            if (!aggroTablesIAmOn.Contains(aNonfriendly))
            {
                DebugManager.Print(GetType(), aNonfriendly + " tried to remove me from a table I didn't know I was on.");
                return;
            }
            aggroTablesIAmOn.Remove(aNonfriendly);
        }
    }
}
