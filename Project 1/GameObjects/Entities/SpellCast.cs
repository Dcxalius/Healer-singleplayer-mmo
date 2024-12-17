using Project_1.Camera;
using Project_1.GameObjects.Spells;
using Project_1.Managers;
using Project_1.Tiles;
using Project_1.UI.HUD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Project_1.GameObjects.Spells.AoE.AreaOfEffectData;

namespace Project_1.GameObjects.Entities
{
    internal class SpellCast
    {
        Entity owner;

        public bool OffGlobalCooldown { get => lastCastSpell + globalCooldown < TimeManager.TotalFrameTime; }
        public double RatioOfGlobalCooldownDone { get => Math.Min((TimeManager.TotalFrameTime - lastCastSpell) / globalCooldown, 1); }
        const double globalCooldown = 1500;
        double lastCastSpell;
        Spell channeledSpell;
        WorldSpace channeledSpellStartPosition;
        Entity channelTarget;
        double startCastTime;

        public SpellCast(Entity aOwner)
        {
            owner = aOwner;
        }
        bool CastSpeedCheck()
        {
            const float graceSpeedWindow = 0.1f;
            if (owner.Momentum.ToVector2().Length() < graceSpeedWindow)
            {
                return false;
            }
            return true;
        }
        public void UpdateSpellChannel()
        {
            if (channeledSpell == null) return;
            if (CastSpeedCheck())
            {
                CancelChannel();

                return;
            }

            if (FinishChannel()) return;

            HUDManager.UpdateChannelSpell((float)((TimeManager.TotalFrameTime - startCastTime) / channeledSpell.CastTime));
        }

        void CancelChannel()
        {
            HUDManager.CancelChannel();
            channelTarget = null;
            channeledSpell = null;
            channeledSpellStartPosition = WorldSpace.Zero;
        }

        bool FinishChannel()
        {
            if (channeledSpell.CastTime < TimeManager.TotalFrameTime - startCastTime)
            {
                const float graceWidth = 5;
                if (channeledSpell.CastDistance + graceWidth < (channelTarget.FeetPosition - owner.FeetPosition).ToVector2().Length())
                {
                    CancelChannel();
                    return true;

                }

                CastSpell(channeledSpell);

                HUDManager.FinishChannel();
                channeledSpell = null;
                channelTarget = null;
                channeledSpellStartPosition = WorldSpace.Zero;
                return true;
            }
            return false;
        }

        bool StartChannel(Spell aSpell)
        {
            if (ChannelChecks(aSpell)) return false; 
            channelTarget = owner.Target;
            if (channelTarget == null) { channelTarget = owner; }
            lastCastSpell = TimeManager.TotalFrameTime;
            channeledSpellStartPosition = owner.FeetPosition;
            channeledSpell = aSpell;
            startCastTime = TimeManager.TotalFrameTime;
            HUDManager.ChannelSpell(channeledSpell);
            HUDManager.UpdateChannelSpell(0);
            return true;
        }

        bool ChannelChecks(Spell aSpell)
        {
            if (channeledSpell != null) return false;
            if (aSpell == null) return false;
            if (aSpell.CastTime == 0) return false;
            if (!aSpell.OffCooldown) return false;
            if (CastSpeedCheck()) return false;
            return true;
        }

        public bool StartCast(Spell aSpell)
        {
            if (aSpell == null) return false;
            //if (!spellBook.HasSpell(aSpell)) return false;
            if (!owner.Resource.isCastable(aSpell.ResourceCost)) return false;
            if (!OffGlobalCooldown) return false;
            if (!aSpell.OffCooldown) return false;

            if (owner.Target != null)
            {
                float d = (owner.Target.FeetPosition - owner.FeetPosition).ToVector2().Length();
                if (d > aSpell.CastDistance) return false;

                if (!aSpell.Targetable(owner.Target.Relation)) return false;
                if (!TileManager.CheckLineOfSight(owner, owner.Target.FeetPosition)) return false;
            }
            else
            {
                if (!aSpell.Targetable(owner.Relation)) return false;

            }


            if (aSpell.CastTime > 0)
            {
                StartChannel(aSpell);
                return true;
            }
            lastCastSpell = TimeManager.TotalFrameTime;
            return CastSpell(aSpell);
        }

        bool CastSpell(Spell aSpell)
        {

            if (!aSpell.Cast(owner.Target)) return false;
            owner.Resource.CastSpell(aSpell.ResourceCost);

            return true;
        }
    }
}
