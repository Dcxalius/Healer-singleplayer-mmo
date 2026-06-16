using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_1.Camera;
using Project_1.GameObjects.Spells;
using Project_1.GameObjects.Spells.Buff;
using Project_1.Managers;
using Project_1.World.GameObjects.Unit.Talents;

namespace Project_1.GameObjects.Entities
{
    internal partial class Entity
    {
        public bool OffGlobalCooldown => spellCast.OffGlobalCooldown;
        public double RatioOfGlobalCooldownDone => spellCast.RatioOfGlobalCooldownDone;
        SpellCast spellCast;
        BuffList buffList;

        public bool StartCast(Spell aSpell)
        {
            ThreadAffinity.AssertSimThread();
            return spellCast.StartCast(aSpell);
        }

        public bool StartCastAt(Spell aSpell, WorldSpace aTargetPosition)
        {
            ThreadAffinity.AssertSimThread();
            return spellCast.StartCastAt(aSpell, aTargetPosition);
        }

        public void AddBuff(Buff aBuff)
        {
            ThreadAffinity.AssertSimThread();
            buffList.AddBuff(aBuff, this);
        }

        public List<Buff> GetAllBuffs()
        {
            ThreadAffinity.AssertSimThread();
            return buffList.GetAllBuffs();
        }

        public void RemoveFirstBuffOfType<T>() where T : Buff
        {
            ThreadAffinity.AssertSimThread();
            buffList.RemoveFirstBuffOfType<T>(this);
        }

        //TODO: Destring
        public void RemoveStatusBuffsByTag(string aStatusTag)
        {
            ThreadAffinity.AssertSimThread();
            buffList?.RemoveBuffsWithStatusTag(this, aStatusTag);
            FlagForRefresh();
        }

        //TODO: Destring
        public bool HasStatusTag(string aStatusTag)
        {
            ThreadAffinity.AssertSimThread();
            return buffList?.HasStatusTag(aStatusTag) ?? false;
        }

        public float VisualOpacity
        {
            get
            {
                ThreadAffinity.AssertSimThread();
                return buffList?.GetVisualOpacity() ?? 1f;
            }
        }

        //TODO: Destring

        public double GetStatusStatFlat(string aStat)
        {
            ThreadAffinity.AssertSimThread();
            return buffList?.GetStatusFlat(aStat) ?? 0d;
        }

        //TODO: Destring
        public double GetStatusStatPercent(string aStat)
        {
            ThreadAffinity.AssertSimThread();
            return buffList?.GetStatusPercent(aStat) ?? 0d;
        }
        //TODO: Destring
        public void ConsumeBuffStack(Buff aBuff)
        {
            ThreadAffinity.AssertSimThread();
            buffList?.ConsumeStack(aBuff, this);
        }

        public void RefreshStatsFromStatusChange()
        {
            ThreadAffinity.AssertSimThread();
            unitData.BaseStats.RefreshStats();
            FlagForRefresh();
        }

        //Q: Why does this need a separate method from RefreshStatsFromStatusChange? Rather than just calling FlagForRefresh?
        public void RefreshVisualsFromStatusChange()
        {
            ThreadAffinity.AssertSimThread();
            FlagForRefresh();
        }
    }
}
