using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects.Entities.Corspes;
using Project_1.GameObjects.Entities.Friendlies.Players;
using Project_1.GameObjects.FloatingTexts;
using Project_1.GameObjects.Unit;
using Project_1.GameObjects.Unit.Stats;
using Project_1.Managers;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.Particles;
using Project_1.World.GameObjects.Spells.SpellEffects;
using Project_1.World.GameObjects.Unit.Stats.Secondary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Project_1.GameObjects.Unit.Stats.HitTable;

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
            //TODO: Target should be defined. Probably as an interface. This allows for setting more things like doodads and corpses as targets.
            //TODO: Target =/= Attacking target, so that needs to be implemented somehow.
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
            MailboxManager.PublishUiEvent(new TargetChanged(RelationToPlayer.ToRelationToPlayerKind(), null)); //TODO: I'm assuming relation is used to figure out what unit frame is used. This should probably be changed to target the specific affected unitframes
        }


        float GetMinAttackRange() //Q: How should we handle attack range for different ranges? Currently it checks the smallest, and then internal checks in the attack patterns to see if the particular weapon is in range to be used.
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
            if (target == null) return; //TODO: Make this stop the unit from attacking everything non-friendly that it targets. There should probably be a bool somewhere here or the target interface.
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
            //TODO: Reaching the maximum accepted size, should be broken up.
            RemoveStatusBuffsByTag("Stealth"); //TODO: Make this general perhaps? Perhaps a event system for buffs? Allowing for the subsription to OnAttack events
            HitTable.HitResult hitResult = HitTable.GenerateTable(aAttack, this, target);

            //TODO: Proc onhits
            Damage damage;
            if (hitResult == HitTable.HitResult.Miss || hitResult == HitTable.HitResult.Dodge || hitResult == HitTable.HitResult.Parry)
            {
                damage = Damage.Zero;
            }
            else
            {
                //Check if eq/talents/skills/buffs/spells procs
                damage = new Damage(aAttack.GetAttackDamage, DamageType.Physical); //TODO: Get DamageType from weapon instead
                //TODO: Handle attacks from weapons that do multiple types of damage.
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
                //Q: This has never been printed, should this just be an Assert?
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
                //Q: This has never been printed, should this just be an Assert?
                return;
            }
            aggroTablesIAmOn.Remove(aNonfriendly);
        }

        bool CheckMiss(HitTable.HitResult aHitResult, Entity aAttacker, Unit.Attack aDamagingThing)
        {
            //aHitResult != HitResult.Miss ? return false : PublishMissEvent(aAttacker, aDamagingThing); is probably the final verison
            //At that point it might be worth breaking it down into simply mapping the HitResult to a PublishEvent, and then just calling that off a switch
            if (aHitResult != HitResult.Miss) return false;
            PublishMissEvent(aAttacker, aDamagingThing);
            if (this is NonFriendly nf) nf.AddToAggroTable(aAttacker, 1); //TODO: This should be moved into the damage recieved event system
            return true;
        }

        bool CheckDodge(HitTable.HitResult aHitResult, Entity aAttacker, Unit.Attack aDamagingThing)
        {
            if (aHitResult != HitResult.Dodge) return false;
            PublishDodgeEvent(aAttacker, aDamagingThing);
            if (this is NonFriendly nf) nf.AddToAggroTable(aAttacker, 1); //TODO: This should be moved into the damage recieved event system
            return true;
        }

        bool CheckParry(HitTable.HitResult aHitResult, Entity aAttacker, Unit.Attack aDamagingThing)
        {
            if (aHitResult != HitResult.Parry) return false;
            PublishParryEvent(aAttacker, aDamagingThing);
            if (this is NonFriendly nf) nf.AddToAggroTable(aAttacker, 1); //TODO: This should be moved into the damage recieved event system
            return true;
        }
        
        Color CheckCrit(Entity aAttacker, Damage aDamageTaken)
        {
            //PublishCritEvent();
            aDamageTaken.ApplyCriticalStrike(aAttacker, this); //TODO: Double check that this applies correctly
            return Color.Yellow;
        }

        Color CheckCrushing(Entity aAttacker, Damage aDamageTaken)
        {
            //PublishCrushingEvent();
            aDamageTaken.ApplyCrushingDamage(aAttacker, this);
            return Color.Orange;
        }

        Color CheckBlock(HitTable.HitResult aHitResult, Entity aAttacker, Unit.Attack aDamagingThing, Damage aDamageTaken)
        {
            if (aHitResult != HitTable.HitResult.Block) return Color.Black;
            aDamageTaken.ApplyBlocked(aAttacker, this);
            PublishBlockEvent(aAttacker, aDamagingThing, !aDamageTaken.ContainsDamage);
            
            return CheckFullBlock(aHitResult, aAttacker, aDamagingThing, aDamageTaken); //TODO: Decide if there should be a separate color here, or if current matching Hit is fine, then codify the HitColor somewhere //Not that it matters once the proper coloring system is in place.
        }

        Color CheckFullBlock(HitTable.HitResult aHitResult, Entity aAttacker, Unit.Attack aDamagingThing, Damage aDamageTaken)
        {
            if (!aDamageTaken.ContainsDamage)
            {
                SpawnFlyingText("Blocked", GetDirOfFloatingText(aAttacker.FeetPosition), Color.LightGray, Color.Black, 1f);
                PublishHitEvent(aAttacker, aDamagingThing, aHitResult, aDamageTaken);
                //TODO: A seperate FullBlock Event for this case? Possible not, either way, firing a hit even seems wrong.
                if (this is NonFriendly nf) nf.AddToAggroTable(aAttacker, 1);
                return Color.LightGray;
            }
            return Color.Gray;
        }

        Color CheckGlancing(Entity aAttacker, Unit.Attack aDamagingThing, Damage aDamageTaken)
        {
            Debug.Assert(UnitType != UnitType.Player); //TODO: More robust check? Also gm should probably also be able to glance
            aDamageTaken.ApplyGlancingBlowDamage(aAttacker, aDamagingThing, this);
            return Color.DimGray;
        }


        Color CheckHit(HitTable.HitResult aHitResult, Entity aAttacker, Unit.Attack aDamagingThing, Damage aDamageTaken)
        {
            switch (aHitResult)
            {
                case HitTable.HitResult.Glancing:
                    return CheckGlancing(aAttacker, aDamagingThing, aDamageTaken);
                case HitTable.HitResult.Block:
                    return CheckBlock(aHitResult, aAttacker, aDamagingThing, aDamageTaken);
                case HitTable.HitResult.Crit:
                    return CheckCrit(aAttacker, aDamageTaken);
                case HitTable.HitResult.Crushing:
                    return CheckCrushing(aAttacker, aDamageTaken);
                case HitTable.HitResult.Hit:
                    return Color.Black; 
                default:
                    throw new Exception("Missing Case");
            }
        }

        void ProcessHit(HitTable.HitResult aHitResult, Entity aAttacker, Unit.Attack aDamagingThing, Damage aDamageTaken, Color aResultColor)
        {
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
                ProcessDamage(aAttacker, causeName, damageValue, 1f, aDamageTaken.Types[i], aResultColor);
            }

            ParticleMovement bloodMovement = new ParticleMovement(GetDirOfFloatingText(aAttacker.FeetPosition), WorldSpace.Zero, 0.9f);
            ParticleManager.SpawnParticle(bloodsplatter, WorldRectangle, FeetPosition.Y, bloodMovement, (int)Math.Max(1, Math.Min((aDamageTaken.Sum / MaxHealth) * 100, 100)));
            FlagForRefresh();
            //TODO: Check death here?
        }

        bool CheckDamageless(HitTable.HitResult aHitResult, Entity aAttacker, Unit.Attack aDamagingThing)
        {
            if (CheckMiss(aHitResult, aAttacker, aDamagingThing)) return false;
            if (CheckDodge(aHitResult, aAttacker, aDamagingThing)) return false;
            if (CheckParry(aHitResult, aAttacker, aDamagingThing)) return false;
            return true;
        }

        public void RecieveAttack(HitTable.HitResult aHitResult, Entity aAttacker, Unit.Attack aDamagingThing, Damage aDamageTaken) //TODO: Events need to be checked, all attacks should fire an attack event, and then hit/miss/dodge/parry/block/glancing/crit/crushing events should be fired based on the result, and then a damage event should be fired if damage is actually taken, and then a death event should be fired if the attack killed the target. Also need to make sure that procs can subscribe to the correct events and that the events contain all necessary information for procs to determine whether they should proc or not
        {
            //TODO: Currently this is called by the attacker. Makes more sense to collect the attacks from all entities first, having them send an event, then processing all the damage taken after the attacks have been dealt with.
            //Then catch that event per entity and process it, allowing for cleaner update flow. Currently entities are updating each other.
            ThreadAffinity.AssertSimThread();

            //Order shouldn't matter, but should be in proper hittable order for clarity.
            if (CheckDamageless(aHitResult, aAttacker, aDamagingThing)) return;
            Damage preMitigation = new Damage(aDamageTaken); //Q: We need to track this for later im pretty sure
            Color resultColor = CheckHit(aHitResult, aAttacker, aDamagingThing, aDamageTaken); //TODO: Prehaps move the color to an out argument for cleanliness here? Prehaps even better, create a custom struct to hold the results and return that instead.
            Damage postMitigation = new Damage(aDamageTaken); //Q: We need to track this for later im pretty sure
            ProcessHit(aHitResult, aAttacker, aDamagingThing, aDamageTaken, resultColor);
        }

        public void RecieveSpellAttack(Entity aCaster, SpellEffect aSpellEffect, Damage aDamageTaken)
        {
            //TODO: Break up
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
                    SpawnFlyingText("Resist", GetDirOfFloatingText(aCaster.FeetPosition), Color.LightGray, Color.Black, 1f);
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
                SpawnFlyingText("Resist", GetDirOfFloatingText(aCaster.FeetPosition), Color.LightGray, Color.Black, 1f);
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
                        ProcessDamage(aCaster, aSpellEffect.Name, (float)aDamageTaken[DamageType.True], 1f, DamageType.True, Color.Black);
                        anyDamageApplied = true;
                        continue;
                    case DamageType.Physical:
                        float reduction = SecondaryStats.Defense.Armor.GetGetReductionPercentage(aCaster.Level.CurrentLevel);
                        float damageTaken = (float)(aDamageTaken[DamageType.Physical] * (1 - reduction));
                        resultString = damageTaken.ToString();
                        ProcessDamage(aCaster, aSpellEffect.Name, damageTaken, 1f, DamageType.Physical, Color.Black);
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
                            SpawnFlyingText(resultString, GetDirOfFloatingText(aCaster.FeetPosition), Color.LightGray, Color.Black, 1f);
                            immune = true;
                            continue;
                        }
                        ProcessDamage(aCaster, aSpellEffect.Name, damageTaken, 1f, damageType[i], Color.Black, preFix, suffix);
                        anyDamageApplied = true;
                        break;
                        
                }
                SpawnFlyingText(resultString, GetDirOfFloatingText(aCaster.FeetPosition), Color.Red, Color.Black, 1f);
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

        protected virtual void ProcessDamage(Entity aCause, string aCauseName, float aDamageTaken, float aThreatMod, DamageType aDamageType, Color aBorderColor, string aPrefix = "", string aSuffix = "")
        {
            //TODO: Break up
            Color textColor = aDamageType switch
            {
                DamageType.Physical => Color.Red,
                DamageType.Arcane => Color.LightBlue,
                DamageType.Fire => Color.OrangeRed,
                DamageType.Frost => Color.LightCyan,
                DamageType.Holy => Color.LightYellow,
                DamageType.Nature => Color.LightGreen,
                DamageType.Shadow => Color.MediumPurple,
                DamageType.True => Color.White,
                _ => Color.White
            };

            double afterAbsorb = buffList.ApplyAbsorb(aDamageTaken, aDamageType, this, out double absorbed);

            if (afterAbsorb <= 0)
            {
                SpawnFlyingText("Absorbed", GetDirOfFloatingText(aCause.FeetPosition), Color.LightBlue, Color.Black, 1f);
                return;
            }

            //TODO: aCause.DpsMeter.RegisterDamageDone(this, aCauseName, aDamageTaken, aDamageType);

            string absorbSuffix = absorbed > 0 ? $" ({FormatHealthDelta(absorbed)} Absorbed)" : string.Empty;
            double appliedDelta = ApplyHealthDelta(-afterAbsorb);
            if (appliedDelta < 0) //TODO: This should be handled better. Have the stealthspells instead subscribe to an OnDamageTaken event, and then have the spell remove itself when damage is taken
            {
                RemoveStatusBuffsByTag("Stealth");
            }
            SpawnFlyingText(aPrefix + FormatHealthDelta(-appliedDelta) + aSuffix + absorbSuffix, GetDirOfFloatingText(aCause.FeetPosition), textColor, aBorderColor, 1f);
        }

        void SpawnFlyingText(string aHealthChangeValue, WorldSpace aDirOfFlyingStuff, Color aTextColor, Color aBorderColor, float aBorderWidth) => FloatingTextManager.AddFloatingText(new FloatingText(aHealthChangeValue, aTextColor, FeetPosition, aDirOfFlyingStuff, aBorderColor: aBorderColor, aBorderWidth: aBorderWidth));


        WorldSpace GetDirOfFloatingText(WorldSpace aFeetPosOfTriggerer) //TODO: Generalize?
        {
            //TODO: Entire floating text system needs to be reworked. WorldSpace will be 3d eventually, and we need to determine if there should be floating text in the 3d space or just in screenspace.
            //2d is easier as it is just converting the worldspace position to screenspace and then doing the floating text pos and angle based on that, but 3d needs new objects.
            WorldSpace dirOfFlyingStuff = (FeetPosition - aFeetPosOfTriggerer);
            if (dirOfFlyingStuff == WorldSpace.Zero)
            {
                dirOfFlyingStuff = new WorldSpace(0, 1);
            }
            dirOfFlyingStuff.Normalize();
            return dirOfFlyingStuff;
        }
    }
}
