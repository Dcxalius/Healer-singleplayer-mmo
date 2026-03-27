using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects.Entities.Corspes;
using Project_1.GameObjects.FloatingTexts;
using Project_1.GameObjects.Spells;
using Project_1.GameObjects.Unit;
using Project_1.GameObjects.Unit.Stats;
using Project_1.Managers;
using Project_1.Particles;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_1.GameObjects.Entities.Friendlies.Players;

namespace Project_1.GameObjects.Entities
{
    internal partial class Entity
    {
        public bool Selected => ObjectManager.Player.Target == this;
        public virtual Entity Target { get => target; }
        protected Entity target = null;
        public virtual bool InCombat => aggroTablesIAmOn.Count > 0;
        List<NonFriendly> aggroTablesIAmOn;

        protected abstract bool CheckForRelation();

        public void SetTarget(Entity aEntity)
        {
            ThreadAffinity.AssertSimThread();
            target = aEntity;
            MailboxManager.PublishUiEvent(new TargetChanged(RelationToPlayer.ToRelationToPlayerKind(), target?.BuildUiSnapshot()));
            if (target == null) return;
            List<Project_1.GameObjects.Spells.Buff.Buff> buffs = target.GetAllBuffs();
            for (int i = 0; i < buffs.Count; i++)
            {
                MailboxManager.PublishUiEvent(new BuffAdded(target.RenderId, buffs[i].BuffUiSnapshot));
            }
        }

        public void RemoveTarget()
        {
            ThreadAffinity.AssertSimThread();
            target = null;
            MailboxManager.PublishUiEvent(new TargetChanged(RelationToPlayer.ToRelationToPlayerKind(), null));
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
            WorldSpace tweenVector = (target.FeetPosition - FeetPosition);
            float lengthToTarget = tweenVector.ToVector2().Length();
            float sizeOffset = Size.X / 2 + target.Size.X / 2;
            if (lengthToTarget - sizeOffset >= GetMinAttackRange()) return;

            CheckAttackSpeed(ref unitData.NextAvailableMainHandAttack, a.MainHandAttack);
            if (target == null) return;
            CheckAttackSpeed(ref unitData.NextAvailableOffHandAttack, a.OffHandAttack);
        }

        void CheckAttackSpeed(ref TimeSpan aLockoutTime, Unit.Attack aAttack)
        {
            if (aAttack == null) return;
            if (aLockoutTime > TimeManager.TotalFrameTimeAsTimeSpan) return;

            aLockoutTime = TimeManager.TotalFrameTimeAsTimeSpan + TimeSpan.FromSeconds(aAttack.SecondsPerAttack);
            HitTarget(aAttack);
        }

        void HitTarget(Unit.Attack aAttack)
        {
            HitTable.HitResult hitResult = HitTable.GenerateTable(aAttack, this, target);

            //TODO: Proc onhits,
            Damage damage;
            if (hitResult == HitTable.HitResult.Miss || hitResult == HitTable.HitResult.Dodge || hitResult == HitTable.HitResult.Parry)
            {
                damage = new Damage(new double[] { 0 }, new DamageType[] { DamageType.True });
            }
            else
            {
                //Check if eq/talents/skills/buffs/spells procs
                damage = new Damage(new double[] { aAttack.GetAttackDamage }, new DamageType[] { DamageType.Physical }); //TODO: Get DamageType from weapon instead
            }
            target.RecieveAttack(hitResult, this, aAttack, damage);
            TargetAliveCheck();
        }

        public void AddedToAggroTable(NonFriendly aNonfriendly)
        {
            ThreadAffinity.AssertSimThread();
            if (aggroTablesIAmOn.Contains(aNonfriendly))
            {
                DebugManager.Print(aNonfriendly + " tried to add me to a table I thought I was on.");
                return;
            }
            aggroTablesIAmOn.Add(aNonfriendly);
        }

        public void RemovedFromAggroTable(NonFriendly aNonfriendly)
        {
            ThreadAffinity.AssertSimThread();
            if (!aggroTablesIAmOn.Contains(aNonfriendly))
            {
                DebugManager.Print(aNonfriendly + " tried to remove me from a table I didn't know I was on.");
                return;
            }
            aggroTablesIAmOn.Remove(aNonfriendly);
        }
        public void RecieveAttack(HitTable.HitResult aHitResult, Entity aAttacker, Unit.Attack aDamagingThing, Damage aDamageTaken) //TODO: Events need to be checked, all attacks should fire an attack event, and then hit/miss/dodge/parry/block/glancing/crit/crushing events should be fired based on the result, and then a damage event should be fired if damage is actually taken, and then a death event should be fired if the attack killed the target. Also need to make sure that procs can subscribe to the correct events and that the events contain all necessary information for procs to determine whether they should proc or not
        {
            ThreadAffinity.AssertSimThread();
            string resultString = "";
            Color resultColor = Color.White;
            if (aHitResult <= HitTable.HitResult.Parry)
            {
                switch (aHitResult)
                {
                    case HitTable.HitResult.Miss:
                        resultString = "Miss";
                        resultColor = Color.Gray;
                        PublishMissEvent(aAttacker, aDamagingThing);
                        break;
                    case HitTable.HitResult.Dodge:
                        resultString = "Dodge";
                        resultColor = Color.DarkGray;
                        PublishDodgeEvent(aAttacker, aDamagingThing);
                        break;
                    case HitTable.HitResult.Parry:
                        resultString = "Parry";
                        resultColor = Color.DarkSlateGray;
                        PublishParryEvent(aAttacker, aDamagingThing);
                        break;
                }

                SpawnFlyingText(resultString, GetDirOfFloatingText(aAttacker.FeetPosition), resultColor);
                if (this is NonFriendly nf) nf.AddToAggroTable(aAttacker, 1);
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
                    bool fullyBlocked = !aDamageTaken.ContainsDamage;
                    PublishBlockEvent(aAttacker, aDamagingThing, fullyBlocked);
                    if (fullyBlocked)
                    {
                        resultString = "Blocked";
                        SpawnFlyingText(resultString, GetDirOfFloatingText(aAttacker.FeetPosition), resultColor);
                        PublishHitEvent(aAttacker, aDamagingThing, aHitResult, aDamageTaken);
                        if (this is NonFriendly nf) nf.AddToAggroTable(aAttacker, 1);
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
            PublishHitEvent(aAttacker, aDamagingThing, aHitResult, aDamageTaken);
            PublishOutcomeEvent(aAttacker, aDamagingThing, aHitResult, aDamageTaken);

            if (!aDamageTaken.ContainsDamage) return; //TODO: Spawn Miss or Immune instead of just returning
            string causeName = aDamagingThing != null ? aDamagingThing.WeaponType.ToString() : "Attack";
            for (int i = 0; i < aDamageTaken.Count; i++)
            {
                //TODO: When different damage types are implemented, show different colors for different damage types
                // For example, physical damage could be red, fire damage orange, frost damage blue, etc.
                // And introduce a offset to the floating text position so that multiple damage types don't overlap
                float damageValue = (float)aDamageTaken[aDamageTaken.Types[i]];
                if (damageValue <= 0) continue;
                ProcessDamage(aAttacker, causeName, damageValue, 1f, aDamageTaken.Types[i], resultColor);
            }

            ParticleMovement bloodMovement = new ParticleMovement(GetDirOfFloatingText(aAttacker.FeetPosition), WorldSpace.Zero, 0.9f);
            ParticleManager.SpawnParticle(bloodsplatter, WorldRectangle, FeetPosition.Y, bloodMovement, (int)Math.Max(1, Math.Min((aDamageTaken.Sum / MaxHealth) * 100, 100)));
            FlagForRefresh(); //TODO: Check death here?
        }

        public void RecieveSpellAttack(Entity aCaster, SpellEffect aSpellEffect, Damage aDamageTaken)
        {
            ThreadAffinity.AssertSimThread();
            if (aSpellEffect.StatSource == AbilityStatSource.Attack)
            {
                Unit.Attack attackSource = GetAttackSourceForAttackStatSpell(aCaster);
                HitTable.HitResult hitResult = HitTable.GenerateTable(attackSource, aCaster, this, false, false);
                RecieveAttack(hitResult, aCaster, attackSource, aDamageTaken);
                return;
            }

            string resultString = "";
            var damageType = aDamageTaken.Types;
            int leveldiff = CurrentLevel - aCaster.CurrentLevel;
            float levelHit = MathF.Max(0.01f, leveldiff >= 3 ? 0.96f - leveldiff * 0.01f : 0.83f - (leveldiff - 3) * 0.11f);
            float totalHit = MathF.Min(0.99f, levelHit + (float)aCaster.SecondaryStats.Spell.BonusHitChanceForSchools(aSpellEffect.SpellSchools));

            bool isCrit = false;
            if (RandomManager.RollDouble() <= aCaster.SecondaryStats.Spell.CriticalChanceForSchools(aSpellEffect.SpellSchools))
            {
                isCrit = true;
                double critMultiplier = Math.Max(aCaster.SecondaryStats.Spell.CriticalDamageForSchools(aSpellEffect.SpellSchools) - SecondaryStats.Defense.CriticalDamageReduction, 0);
                aDamageTaken.ApplyCriticalStrike(critMultiplier);
            }

            if (aSpellEffect.IsBinary)
            {
                totalHit = (float)SecondaryStats.Defense.SpellResistance.CalculateResistanceChanceBinary(this, aCaster, aSpellEffect.SpellSchools);

                if (RandomManager.RollDouble() > totalHit)
                {
                    SpawnFlyingText("Resist", GetDirOfFloatingText(aCaster.FeetPosition), Color.Gray);
                    PublishSpellResistEvent(aCaster, aSpellEffect, false);
                    PublishSpellResistedByTargetEvent(aCaster, aSpellEffect, false);
                    if (this is NonFriendly nf) nf.AddToAggroTable(aCaster, 1);
                    return;
                }
                PublishSpellHitEvent(aCaster, aSpellEffect);

                for (int i = 0; i < damageType.Count; i++)
                {
                    ProcessDamage(aCaster, aSpellEffect.Name, (float)aDamageTaken[damageType[i]], 1f, damageType[i], Color.Red);
                }
                PublishSpellHitTakenEvent(aCaster, aSpellEffect);
                if (isCrit)
                {
                    PublishSpellCritEvent(aCaster, aSpellEffect);
                    PublishSpellCritTakenEvent(aCaster, aSpellEffect);
                }
                return;
            }

            if (RandomManager.RollDouble() > totalHit)
            {
                SpawnFlyingText("Resist", GetDirOfFloatingText(aCaster.FeetPosition), Color.Gray);
                PublishSpellResistEvent(aCaster, aSpellEffect, false);
                PublishSpellResistedByTargetEvent(aCaster, aSpellEffect, false);
                if (this is NonFriendly nf) nf.AddToAggroTable(aCaster, 1);
                return;
            }

            bool anyDamageApplied = false;
            bool immune = false;
            for (int i = 0; i < damageType.Count; i++)
            {
                switch (damageType[i])
                {
                    case DamageType.True:
                        ProcessDamage(aCaster, aSpellEffect.Name, (float)aDamageTaken[DamageType.True], 1f, DamageType.True, Color.Red, aDamageTaken[DamageType.True].ToString());
                        anyDamageApplied = true;
                        continue;
                    case DamageType.Physical:
                        float reduction = SecondaryStats.Defense.Armor.GetGetReductionPercentage(aCaster.Level.CurrentLevel);
                        float damageTaken = (float)(aDamageTaken[DamageType.Physical] * (1 - reduction));
                        resultString = damageTaken.ToString();
                        ProcessDamage(aCaster, aSpellEffect.Name, damageTaken, 1f, DamageType.Physical, Color.Red);
                        anyDamageApplied = true;
                        break;
                    default:
                        //TODO: Implement spell color on damage text
                        float damageBeforeResist = (float)(aDamageTaken[damageType[i]]);
                        double resistance = SecondaryStats.Defense.SpellResistance.CalculateDamageReductionNonBinary(this, aCaster, SpellResitance.DamageToSpellType(damageType[i]));
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
                            immune = true;
                            continue;
                        }
                        ProcessDamage(aCaster, aSpellEffect.Name, damageTaken, 1f, damageType[i], Color.Red, preFix, suffix);
                        anyDamageApplied = true;
                        break;
                        
                }
                SpawnFlyingText(resultString, GetDirOfFloatingText(aCaster.FeetPosition), Color.Red);
            }

            if (anyDamageApplied)
            {
                PublishSpellHitEvent(aCaster, aSpellEffect);
                PublishSpellHitTakenEvent(aCaster, aSpellEffect);
                if (isCrit)
                {
                    PublishSpellCritEvent(aCaster, aSpellEffect);
                    PublishSpellCritTakenEvent(aCaster, aSpellEffect);
                }
            }
            else if (immune)
            {
                PublishSpellResistEvent(aCaster, aSpellEffect, true);
                PublishSpellResistedByTargetEvent(aCaster, aSpellEffect, true);
            }
        }

        static Unit.Attack GetAttackSourceForAttackStatSpell(Entity aCaster)
        {
            Unit.Attack attack = aCaster.unitData.AttackData.MainHandAttack ?? aCaster.unitData.AttackData.OffHandAttack;
            if (attack != null)
            {
                return attack;
            }

            throw new InvalidOperationException("No attack source available for attack-sourced spell.");
        }

        //TODO: aCauseName should probably not be a string, but rather some kind of reference to the spell/ability/item that caused the damage
        protected virtual void ProcessDamage(Entity aCause, string aCauseName, float aDamageTaken, float aThreatMod, DamageType aDamageType, Color aBorderColor, string aPrefix = "", string aSuffix = "")
        {
            Color textColor = Color.Red; //TODO: Different colors for different damage types
            double appliedDelta = ApplyHealthDelta(-aDamageTaken);

            //TODO: aCause.DpsMeter.RegisterDamageDone(this, aCauseName, aDamageTaken, aDamageType);

            SpawnFlyingText(aPrefix + FormatHealthDelta(-appliedDelta) + aSuffix, GetDirOfFloatingText(aCause.FeetPosition), textColor);
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

        void PublishDodgeEvent(Entity aAttacker, Unit.Attack aAttack)
        {
            PublishToDefender(new DodgeEvent(aAttacker, this, aAttack));
            PublishToAttacker(aAttacker, new AttackDodgedEvent(aAttacker, this, aAttack));
        }

        void PublishParryEvent(Entity aAttacker, Unit.Attack aAttack)
        {
            PublishToDefender(new ParryEvent(aAttacker, this, aAttack));
            PublishToAttacker(aAttacker, new AttackParriedEvent(aAttacker, this, aAttack));
        }

        void PublishBlockEvent(Entity aAttacker, Unit.Attack aAttack, bool aFullyBlocked)
        {
            PublishToDefender(new BlockEvent(aAttacker, this, aAttack, aFullyBlocked));
            PublishToAttacker(aAttacker, new AttackBlockedEvent(aAttacker, this, aAttack, aFullyBlocked));
        }

        void PublishMissEvent(Entity aAttacker, Unit.Attack aAttack)
        {
            PublishToAttacker(aAttacker, new AttackMissedEvent(aAttacker, this, aAttack));
            PublishToDefender(new MissedByEvent(aAttacker, this, aAttack));
        }

        void PublishHitEvent(Entity aAttacker, Unit.Attack aAttack, HitTable.HitResult aResult, Damage aDamageTaken)
        {
            var snapshot = new Damage(aDamageTaken);
            PublishToAttacker(aAttacker, new AttackHitEvent(aAttacker, this, aAttack, aResult, snapshot));
            PublishToDefender(new HitTakenEvent(aAttacker, this, aAttack, aResult, new Damage(snapshot)));
        }

        void PublishOutcomeEvent(Entity aAttacker, Unit.Attack aAttack, HitTable.HitResult aResult, Damage aDamageTaken)
        {
            var snapshot = new Damage(aDamageTaken);
            switch (aResult)
            {
                case HitTable.HitResult.Glancing:
                    PublishToAttacker(aAttacker, new AttackGlancedEvent(aAttacker, this, aAttack, snapshot));
                    PublishToDefender(new GlancingTakenEvent(aAttacker, this, aAttack, new Damage(snapshot)));
                    break;
                case HitTable.HitResult.Crit:
                    PublishToAttacker(aAttacker, new AttackCritEvent(aAttacker, this, aAttack, snapshot));
                    PublishToDefender(new CritTakenEvent(aAttacker, this, aAttack, new Damage(snapshot)));
                    break;
                case HitTable.HitResult.Crushing:
                    PublishToAttacker(aAttacker, new AttackCrushedEvent(aAttacker, this, aAttack, snapshot));
                    PublishToDefender(new CrushingTakenEvent(aAttacker, this, aAttack, new Damage(snapshot)));
                    break;
                default:
                    break;
            }
        }

        void PublishSpellHitEvent(Entity aCaster, SpellEffect aSpellEffect)
        {
            PublishToAttacker(aCaster, new SpellHitEvent(aCaster, this, aSpellEffect));
        }

        void PublishSpellCritEvent(Entity aCaster, SpellEffect aSpellEffect)
        {
            PublishToAttacker(aCaster, new SpellCritEvent(aCaster, this, aSpellEffect));
        }

        void PublishSpellResistEvent(Entity aCaster, SpellEffect aSpellEffect, bool aImmune)
        {
            PublishToAttacker(aCaster, new SpellResistEvent(aCaster, this, aSpellEffect, aImmune));
        }

        void PublishSpellHitTakenEvent(Entity aCaster, SpellEffect aSpellEffect)
        {
            var evt = new SpellHitTakenEvent(aCaster, this, aSpellEffect);
            Events.Publish(evt);
        }

        void PublishSpellCritTakenEvent(Entity aCaster, SpellEffect aSpellEffect)
        {
            var evt = new SpellCritTakenEvent(aCaster, this, aSpellEffect);
            Events.Publish(evt);
        }

        void PublishSpellResistedByTargetEvent(Entity aCaster, SpellEffect aSpellEffect, bool aImmune)
        {
            var evt = new SpellResistedByTargetEvent(aCaster, this, aSpellEffect, aImmune);
            Events.Publish(evt);
        }

        void PublishToDefender<T>(T aEvent)
        {
            Events.Publish(aEvent);
        }

        void PublishToAttacker<T>(Entity aAttacker, T aEvent)
        {
            if (aAttacker == null) return;
            aAttacker.Events.Publish(aEvent);
        }
    }
}
