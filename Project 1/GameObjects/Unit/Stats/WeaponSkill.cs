using Project_1.Items.SubTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Unit.Stats
{
    internal class WeaponSkill
    {
        Dictionary<Weapon.WeaponType, int> skills;

        public void LevelUpSkill(Weapon.WeaponType aType)
        {
            if (!skills.ContainsKey(aType)) throw new Exception("Player does not have skill for weapon type " + aType);

            skills[aType]++;
        }

        public int GetSkill(Weapon.WeaponType aType)
        {
            if (!skills.TryGetValue(aType, out int returnable)) returnable = 0;
            return returnable;
        }
    }
}
