using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects.Entities.Corspes;
using Project_1.GameObjects.FloatingTexts;
using Project_1.GameObjects.Spells;
using Project_1.GameObjects.Unit;
using Project_1.GameObjects.Unit.Stats;
using Project_1.Managers;
using Project_1.Particles;
using Project_1.UI.HUD.Managers;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public void RecieveAttack(HitTable.HitResult aHitResult, Entity aAttacker, Unit.Attack aDamagingThing, Damage aDamageTaken)
        {
            string resultString = "";
            Color resultColor = Color.White;
            if (aHitResult <= HitTable.HitResult.Parry)
            {
                switch (aHitResult)
                {
                    case HitTable.HitResult.Miss:
                        resultString = "Miss";
                        resultColor = Color.Gray;
                        break;
                    case HitTable.HitResult.Dodge:
                        resultString = "Dodge";
                        resultColor = Color.DarkGray;
                        //TODO: Trigger dodge event
                        break;
                    case HitTable.HitResult.Parry:
                        resultString = "Parry";
                        resultColor = Color.DarkSlateGray;
                        //TODO: Trigger parry event
                        break;
                }

                SpawnFlyingText(resultString, GetDirOfFloatingText(aAttacker.FeetPosition), resultColor);
                return;
            }
            Damage premitigation = new Damage(aDamageTaken);
            switch (aHitResult)
            {
                case HitTable.HitResult.Glancing:
                    Debug.Assert(UnitType != UnitType.Player);
                    aDamageTaken.ApplyGlancingBlowDamage(aAttacker, aDamagingThing, this);
                    resultColor = Color.DimGray;
                    break;
                case HitTable.HitResult.Block:
                    aDamageTaken.ApplyBlocked(aAttacker, this);
                    resultColor = Color.LightGray;
                    //TODO: Trigger block event
                    if (!aDamageTaken.ContainsDamage)
                    {
                        resultString = "Blocked";
                        SpawnFlyingText(resultString, GetDirOfFloatingText(aAttacker.FeetPosition), resultColor);
                        return;
                    }
                    break;
                case HitTable.HitResult.Crit:
                    aDamageTaken.ApplyCriticalStrike(aAttacker, this);
                    resultColor = Color.Yellow;
                    break;
                case HitTable.HitResult.Crushing:
                    aDamageTaken.ApplyCrushingDamage(aAttacker, this);
                    resultColor = Color.Orange;
                    break;
                case HitTable.HitResult.Hit:
                    resultColor = Color.Red; //TODO: Instead of just using text color, have the text color depend on the damage type and glancing/blocked/crit/crushing/hit change the border color
                    break;
                default:
                    break;
            }
            aDamageTaken.ApplyDamageReduction(aAttacker, this, aDamagingThing);

            if (aDamageTaken.Sum <= 0) return;

            for (int i = 0; i < aDamageTaken.Count; i++)
            {
                //TODO: When different damage types are implemented, show different colors for different damage types
                // For example, physical damage could be red, fire damage orange, frost damage blue, etc.
                // And introduce a offset to the floating text position so that multiple damage types don't overlap
                unitData.Health.CurrentHealth -= aDamageTaken[aDamageTaken.Types[i]];

                WorldSpace dir = GetDirOfFloatingText(aAttacker.FeetPosition);
                SpawnFlyingText(resultString, dir, resultColor);
            }

            ParticleMovement bloodMovement = new ParticleMovement(GetDirOfFloatingText(aAttacker.FeetPosition), WorldSpace.Zero, 0.9f);
            ParticleManager.SpawnParticle(bloodsplatter, WorldRectangle, this, bloodMovement, (int)Math.Max(1, Math.Min((aDamageTaken.Sum / MaxHealth) * 100, 100)));
            FlagForRefresh(); //TODO: Check death here?
        }

        public void RecieveSpellAttack(Entity aCaster, string aCauseName, Damage aDamageTaken)
        {
            string resultString = "";
            var damageType = aDamageTaken.Types;
            
            for (int i = 0; i < damageType.Count; i++)
            {
                switch (damageType[i])
                {
                    case DamageType.True:
                        ProcessDamage(aCaster, aCauseName, (float)aDamageTaken[DamageType.True], DamageType.True, aDamageTaken[DamageType.True].ToString());
                        continue;
                    case DamageType.Physical:
                        float reduction = SecondaryStats.Defense.Armor.GetGetReductionPercentage(aCaster.Level.CurrentLevel);
                        float damageTaken = (float)(aDamageTaken[DamageType.Physical] * (1 - reduction));
                        resultString = damageTaken.ToString();
                        ProcessDamage(aCaster, aCauseName, damageTaken, DamageType.Physical);
                        break;
                    default:
                        
                        float damageBeforeResist = (float)(aDamageTaken[damageType[i]]);
                        double resistance = SpellResitance.CalculateDamageReductionNonBinary(this, aCaster, SpellDamage.DamageToSpellType(damageType[i]));
                        string preFix = "";
                        string suffix = "";
                        if (resistance == 0)
                        {
                            damageTaken = (float)(aDamageTaken[damageType[i]]);
                            resultString = damageTaken.ToString();
                        }
                        else if (resistance < 1)
                        {
                            damageTaken = (float)(aDamageTaken[damageType[i]] * (1 - resistance));
                            resultString = damageTaken.ToString();
                            suffix = " (" + (damageBeforeResist - damageTaken) + " Resisted)";
                        }
                        else
                        {
                            resultString = "Immune";
                            SpawnFlyingText(resultString, GetDirOfFloatingText(aCaster.FeetPosition), Color.Gray);
                            continue;
                        }
                        ProcessDamage(aCaster, aCauseName, damageTaken, damageType[i], preFix, suffix);
                        break;
                        
                }
            }
        }

        protected virtual void ProcessDamage(Entity aCause, string aCauseName, float aDamageTaken, DamageType aDamageType, string aPrefix = "", string aSuffix = "")
        {
            Color textColor = Color.Red; //TODO: Different colors for different damage types
            CurrentHealth -= aDamageTaken;

            //TODO: aCause.DpsMeter.RegisterDamageDone(aCauseName, aDamageTaken, aDamageType, this);

            SpawnFlyingText(aPrefix + aDamageTaken + aSuffix, GetDirOfFloatingText(aCause.FeetPosition), textColor);
        }

        void SpawnFlyingText(string aHealthChangeValue, WorldSpace aDirOfFlyingStuff, Color aTextColor) => FloatingTextManager.AddFloatingText(new FloatingText(aHealthChangeValue, aTextColor, FeetPosition, aDirOfFlyingStuff));


        WorldSpace GetDirOfFloatingText(WorldSpace aFeetPosOfTriggerer)
        {
            WorldSpace dirOfFlyingStuff = (FeetPosition - aFeetPosOfTriggerer);
            if (dirOfFlyingStuff == WorldSpace.Zero)
            {
                dirOfFlyingStuff.Y = 1;
            }
            dirOfFlyingStuff.Normalize();
            return dirOfFlyingStuff;
        }
    }
}
