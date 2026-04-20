using Project_1.Camera;
using Project_1.GameObjects.Spells;
using Project_1.Managers;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.Tiles;
using System;

namespace Project_1.GameObjects.Entities
{
    internal class SpellCast //Should this be part of enitity instead?
    {
        readonly Entity owner;

        public bool OffGlobalCooldown => lastCastSpell + globalCooldown < TimeManager.TotalFrameTime;
        public double RatioOfGlobalCooldownDone => Math.Min((TimeManager.TotalFrameTime - lastCastSpell) / globalCooldown, 1);

        const double globalCooldown = 1500;
        double lastCastSpell;
        Spell channeledSpell;
        Entity channelTarget;
        WorldSpace channelGroundTarget;
        bool channelUsesGroundTarget;
        double startCastTime;

        public SpellCast(Entity aOwner)
        {
            ThreadAffinity.AssertSimThread();
            owner = aOwner;
        }

        bool CastSpeedCheck()
        {
            const float graceSpeedWindow = 0.1f;
            return owner.Momentum.ToVector2().Length() <= graceSpeedWindow;
        }

        public void UpdateSpellChannel()
        {
            ThreadAffinity.AssertSimThread();
            if (channeledSpell == null) return;
            if (!CastSpeedCheck())
            {
                CancelChannel();
                return;
            }

            if (FinishChannel()) return;
            MailboxManager.PublishUiEvent(new CastChannelProgress((float)((TimeManager.TotalFrameTime - startCastTime) / channeledSpell.CastTime)));
        }

        void CancelChannel()
        {
            MailboxManager.PublishUiEvent(new CastChannelCancelled());
            ClearChannelState();
        }

        bool FinishChannel()
        {
            if (channeledSpell == null) return true;
            if (channeledSpell.CastTime >= TimeManager.TotalFrameTime - startCastTime) return false;

            const float graceWidth = 5f;
            if (channelUsesGroundTarget)
            {
                if (!ValidateGroundTarget(channeledSpell, channelGroundTarget, graceWidth))
                {
                    CancelChannel();
                    return true;
                }

                CastSpellAt(channeledSpell, channelGroundTarget);
            }
            else
            {
                Entity target = channelTarget ?? owner;
                if (!ValidateEntityTarget(channeledSpell, target, graceWidth))
                {
                    CancelChannel();
                    return true;
                }

                CastSpell(channeledSpell, target);
            }

            MailboxManager.PublishUiEvent(new CastChannelFinished());
            ClearChannelState();
            return true;
        }

        void ClearChannelState()
        {
            channeledSpell = null;
            channelTarget = null;
            channelGroundTarget = WorldSpace.Zero;
            channelUsesGroundTarget = false;
            startCastTime = 0;
        }

        bool CanStartChannel(Spell aSpell)
        {
            if (channeledSpell != null) return false;
            if (aSpell == null) return false;
            if (aSpell.CastTime <= 0) return false;
            if (!aSpell.OffCooldown) return false;
            if (!CastSpeedCheck()) return false;
            return true;
        }

        bool StartChannelOnEntity(Spell aSpell, Entity aTarget)
        {
            if (!CanStartChannel(aSpell)) return false;
            channelTarget = aTarget ?? owner;
            channelGroundTarget = WorldSpace.Zero;
            channelUsesGroundTarget = false;
            return BeginChannel(aSpell);
        }

        bool StartChannelAtGround(Spell aSpell, WorldSpace aTargetPosition)
        {
            if (!CanStartChannel(aSpell)) return false;
            channelTarget = null;
            channelGroundTarget = aTargetPosition;
            channelUsesGroundTarget = true;
            return BeginChannel(aSpell);
        }

        bool BeginChannel(Spell aSpell)
        {
            lastCastSpell = TimeManager.TotalFrameTime;
            channeledSpell = aSpell;
            startCastTime = TimeManager.TotalFrameTime;
            MailboxManager.PublishUiEvent(new CastChannelStarted(owner.RenderId, channeledSpell.Name, channeledSpell.GfxPath, channeledSpell.CastTime));
            MailboxManager.PublishUiEvent(new CastChannelProgress(0));
            return true;
        }

        public bool StartCast(Spell aSpell)
        {
            ThreadAffinity.AssertSimThread();
            if (aSpell == null) return false;
            if (aSpell.RequiresGroundTarget) return false;
            if (!CommonCastChecks(aSpell)) return false;

            Entity target = owner.Target ?? owner;
            if (!ValidateEntityTarget(aSpell, target, 0f)) return false;

            if (aSpell.CastTime > 0)
            {
                return StartChannelOnEntity(aSpell, target);
            }

            lastCastSpell = TimeManager.TotalFrameTime;
            return CastSpell(aSpell, target);
        }

        public bool StartCastAt(Spell aSpell, WorldSpace aTargetPosition)
        {
            ThreadAffinity.AssertSimThread();
            if (aSpell == null) return false;
            if (!aSpell.RequiresGroundTarget) return StartCast(aSpell);
            if (!CommonCastChecks(aSpell)) return false;
            if (!ValidateGroundTarget(aSpell, aTargetPosition, 0f)) return false;

            if (aSpell.CastTime > 0)
            {
                return StartChannelAtGround(aSpell, aTargetPosition);
            }

            lastCastSpell = TimeManager.TotalFrameTime;
            return CastSpellAt(aSpell, aTargetPosition);
        }

        bool CommonCastChecks(Spell aSpell)
        {
            if (!owner.Resource.isCastable(aSpell.ResourceCost)) return false;
            if (!OffGlobalCooldown) return false;
            if (!aSpell.OffCooldown) return false;
            if (!MeetsCastCondition(aSpell)) return false;
            return true;
        }

        bool MeetsCastCondition(Spell aSpell) => aSpell.CastCondition switch
        {
            CastCondition.AfterDodgeOrParry => owner.InDodgeOrParryWindow,
            _ => true
        };

        bool ValidateEntityTarget(Spell aSpell, Entity aTarget, float graceDistance)
        {
            if (aTarget == null) return false;
            float d = (aTarget.FeetPosition - owner.FeetPosition).ToVector2().Length();
            if (d > aSpell.CastDistance + graceDistance) return false;
            if (!aSpell.Targetable(aTarget.RelationToPlayer)) return false;
            if (!TileManager.CheckLineOfSight(owner, aTarget.FeetPosition)) return false;
            return true;
        }

        bool ValidateGroundTarget(Spell aSpell, WorldSpace aTargetPosition, float graceDistance)
        {
            float d = (aTargetPosition - owner.FeetPosition).ToVector2().Length();
            if (d > aSpell.CastDistance + graceDistance) return false;
            if (!TileManager.CheckLineOfSight(owner, aTargetPosition)) return false;
            return true;
        }

        bool CastSpell(Spell aSpell, Entity aTarget)
        {
            if (!aSpell.Cast(aTarget, owner)) return false;
            owner.Resource.CastSpell(aSpell.ResourceCost);
            if (aSpell.CastCondition == CastCondition.AfterDodgeOrParry)
                owner.ConsumeReactiveWindow();
            return true;
        }

        bool CastSpellAt(Spell aSpell, WorldSpace aTargetPosition)
        {
            if (!aSpell.CastAt(aTargetPosition, owner)) return false;
            owner.Resource.CastSpell(aSpell.ResourceCost);
            return true;
        }
    }
}
