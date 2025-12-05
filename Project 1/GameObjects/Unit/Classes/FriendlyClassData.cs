using Project_1.GameObjects.Unit.Resources;
using Project_1.Items.SubTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Unit.Classes
{
    internal class FriendlyClassData : ClassData
    {

        public int[] GearAllowed => gearAllowed;
        readonly int[] gearAllowed;


        public string[] LearnableSpells => learnableSpells;
        string[] learnableSpells;

        public string[] LevelOneSpells => levelOneSpells;
        string[] levelOneSpells;

        public FriendlyClassData(string name, Resource.ResourceType resource, int[] baseStats, int[] perLevelStats, int baseHp, int perLevelHp, float baseHpPer5, float fistAttackSpeed, float fistMinAttackDamage, float fistMaxAttackDamage, float speed, float maxSpeed, MeleeAttackPowerBonus meleeAttackPowerBonus, float dodgeScaling, float baseDodge, float meleeCritScaling, Weapon.WeaponType weaponsAllowed, string[] learnableSpells, string[] levelOneSpells, int[] gearAllowed, bool canDualWield, bool isCaster, bool canParry)
            : base(name, resource, baseStats, perLevelStats, baseHp, perLevelHp, baseHpPer5, fistAttackSpeed, fistMinAttackDamage, fistMaxAttackDamage, speed, maxSpeed, meleeAttackPowerBonus, dodgeScaling, baseDodge, meleeCritScaling, weaponsAllowed, canDualWield, isCaster, canParry)
        {
            this.gearAllowed = gearAllowed;
            this.learnableSpells = learnableSpells;
            this.levelOneSpells = levelOneSpells;
            Assert();
        }

        protected override void Assert()
        {
            base.Assert();
            Debug.Assert(gearAllowed.Max() <= Equipment.MainSlotCount && gearAllowed.Length == (int)Items.SubTypes.Equipment.GearType.Count);
        }
    }
}
