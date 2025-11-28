using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_1.GameObjects.Spells;
using Project_1.GameObjects.Spells.Buff;

namespace Project_1.GameObjects.Entities
{
    internal partial class Entity
    {
        public bool OffGlobalCooldown => spellCast.OffGlobalCooldown;
        public double RatioOfGlobalCooldownDone => spellCast.RatioOfGlobalCooldownDone;
        SpellCast spellCast;
        BuffList buffList;

        public bool StartCast(Spell aSpell) => spellCast.StartCast(aSpell);

        public void AddBuff(Buff aBuff) => buffList.AddBuff(aBuff, this);

        public List<Buff> GetAllBuffs() => buffList.GetAllBuffs();
    }
}
