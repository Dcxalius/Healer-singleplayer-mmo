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
        public MobClassData(string name, Resource.ResourceType resource, int[] baseStats, int[] perLevelStats, int baseHp, int perLevelHp, float baseHpPer5, string[] learnableSpells, string[] levelOneSpells,
            float fistAttackSpeed, float fistMinAttackDamage, float fistMaxAttackDamage, float speed, float maxSpeed, MeleeAttackPowerBonus meleeAttackPowerBonus, float dodgeScaling, float meleeCritScaling, 
            Weapon.WeaponType weaponsAllowed, bool canDualWield) 
            : base(name, resource, baseStats, perLevelStats, baseHp, perLevelHp, baseHpPer5, fistAttackSpeed, fistMinAttackDamage, fistMaxAttackDamage, speed, maxSpeed, meleeAttackPowerBonus, dodgeScaling, meleeCritScaling, weaponsAllowed, canDualWield)
        {
            Assert();
        }
    }
}
