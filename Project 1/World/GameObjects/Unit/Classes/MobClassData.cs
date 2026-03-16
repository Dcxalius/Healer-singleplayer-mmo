using Project_1.GameObjects.Unit.Resources;
using Project_1.Items.SubTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Unit.Classes
{
    internal class MobClassData : ClassData
    {
        public MobClassData(string name, Resource.ResourceType resource, int[] baseStats, int[] perLevelStats, int baseHp, int perLevelHp, string[] learnableSpells, string[] levelOneSpells,
            float spiritHp5Constant, float spiritHp5Scaling, float fistAttackSpeed, float fistMinAttackDamage, float fistMaxAttackDamage, float speed, float maxSpeed, MeleeAttackPowerBonus meleeAttackPowerBonus, float agilityDodgeScaling, float baseDodge, float meleeCritScaling,
            Weapon.WeaponType weaponsAllowed, bool canDualWield, bool isCaster, bool canParry, float spellCritScaling)
            : base(name, resource, baseStats, perLevelStats, baseHp, perLevelHp, spiritHp5Constant, spiritHp5Scaling, fistAttackSpeed, fistMinAttackDamage, fistMaxAttackDamage, speed, maxSpeed, meleeAttackPowerBonus, agilityDodgeScaling, baseDodge, meleeCritScaling, weaponsAllowed, canDualWield, isCaster, canParry, spellCritScaling)
        {
            Assert();
        }
    }
}
