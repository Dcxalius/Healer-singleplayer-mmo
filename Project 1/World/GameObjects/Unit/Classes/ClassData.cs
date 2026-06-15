using Newtonsoft.Json;
using Project_1.GameObjects.Entities.Friendlies.Players;
using Project_1.GameObjects.Unit.Resources;
using Project_1.Items.SubTypes;
using Project_1.UI.UIElements.Bars;
using Project_1.World.GameObjects.Unit.Stats.Primary;
using Project_1.World.GameObjects.Unit.Talents;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Unit.Classes
{
    internal class ClassData //TODO: Change class to be a class rathar than using string for id everywhere
    {
        public enum Type
        {
            None,
            Player,
            Ally,
            Mob
        }

        public enum MeleeAttackPowerBonus
        {
            None,
            Strength,
            Agility
        }

        public enum RangeAttackPowerBonus
        {
            None,
            One,
            Two//Q: Is there a better way to phrase this than one and two?
        }

        

        public bool WeaponUsuable(Weapon.WeaponType aType) => WeaponsAllowed.HasFlag(aType);
        
        [JsonIgnore]
        public bool[] skillAsBools
        {
            get
            {
                int maxIndex = Enum.GetValues<Weapon.WeaponType>().Max(x => (int)x);
                bool[] bools = new bool[maxIndex + 1];
                foreach (Weapon.WeaponType weaponType in Enum.GetValues<Weapon.WeaponType>())
                {
                    int index = (int)weaponType;
                    bools[index] = weaponType != Weapon.WeaponType.None && weaponsAllowed.HasFlag(weaponType);
                }

                return bools;
            }
        }


        public Weapon.WeaponType WeaponsAllowed => weaponsAllowed;
        Weapon.WeaponType weaponsAllowed;




        public string Name => name;
        readonly string name;

        public Resource.ResourceType Resource => resource;
        readonly Resource.ResourceType resource;


        //TODO: Base stats should also depend on race
        public PrimaryStats BaseStats => baseStats;
        readonly PrimaryStats baseStats;

        public PrimaryStats PerLevelStats => perLevelStats;
        readonly PrimaryStats perLevelStats;

        //Youtube vids + https://barrens.chat/viewtopic.php?t=1046
        //Confirmed ref point, a level 1 troll warrior should have 70 hp, level 1 gnome should have 50. With 23 and 21 base stam resp puts the base hp to 20 for warriors
        //Undead priest => 62 hp with 21 stam giving 32 hp
        //Nelf druid => 61 hp with 19 stam giving 42 hp
        //Nelf rogue => 45 hp with 20 stam giving 25 hp
        //gnome warlock => 43 hp with 20 stam giving 23 hp
        //undead mage => 52 hp with 21 stam giving 31 hp
        //troll hunter => 66 hp with 22 stam giving 24 hp
        //human paladin => 58 hp with 22 stam giving 18 hp
        //orc shaman => 77 hp with 23 stam giving 27 hp
        //NOTE ABOVE VALUES ARE ALL FOR LEVEL 1 SO EITHER CALC THE VALUE - PER LEVEL OR FIGURE OUT WHAT LEVEL 0 => 1 HP / LEVEL IS

        public int BaseHealth => baseHp;
        readonly int baseHp; 

        //TODO: Find what source we use
        public int PerLevelHp => perLevelHp; //TODO: This scales per level :3
        readonly int perLevelHp;

        public int BaseMana => baseMana; //TODO: Implement this in files. For classes that doesn't use mana it should be 0.
        int baseMana;

        public int PerLevelMana => perLevelMana; //TODO: Think about how this should be implemented. A table? A value and then a scalar?
        int perLevelMana;

        //Data from wowhead
        //Druid(caster) : Spirit/4.5 + 15   //Design: Do we want to implement form mp5 penalty? Cant find any info on boomkin regen, but assuming its the same as caster
        //Druid(feral), Hunter, Paladin, Warlock: Spirit/5 + 15
        //Mage, Priest: Spirit/4 + 12.5
        //Shaman: Spirit/5 + 17 

        //Data from warcrafttavern //TODO: Figure out why this site have different numbers from wowhead (WowHead has spr/4.5+15 for druids not in feral forms)
        //Druid, Hunter, Paladin, Shaman  Spirit/5 +15
        //Mage, Priest  Spirit/ 4 + 13
        //Warlock Spirit/4 + 8

        public float BaseMp5 => spiritMp5Constant; //Q: Unsure if the name should be base or spirit. Since the value is treated as Spirit mp5 and not raw mp5
        float spiritMp5Constant; //TODO: Add this value to class files

        public float SpiritMp5Scaling => spiritMp5Scaling;
        float spiritMp5Scaling; //TODO: Add this value to class files

        //Data from warcrafttavern
        public float BaseHp5 => spiritHp5Constant;
        readonly float spiritHp5Constant;

        public float SpiritHp5Scaling => spiritHp5Scaling;
        readonly float spiritHp5Scaling;

        public float FistAttackSpeed => fistAttackSpeed;
        readonly float fistAttackSpeed;

        public float FistMinAttackDamage => fistMinAttackDamage;
        readonly float fistMinAttackDamage;

        public float FistMaxAttackDamage => fistMaxAttackDamage;
        readonly float fistMaxAttackDamage;

        public Movement Movement => movementData;
        Movement movementData;
        
        //If set to agility, the class gains 1 ap per agi and 1 per str
        //If set to strength, the class gains 2 per str and 0 per agility
        public MeleeAttackPowerBonus MeleeAttackBonus => meleeAttackPowerBonus;
        MeleeAttackPowerBonus meleeAttackPowerBonus;

        //TODO: Implement ranged attacks
        //public RangedAttackPowerBonus RangedAttackBonus => rangedAttackBonus;
        //RangedAttackPowerBonus rangedAttackBonus;
        //Hunters gain 2 per level, warriors and rogue gain 1. Other classes don't interact with this at all atm, and probably shouldn't since we don't want wands to scale with AP

        public bool CanDualWield => canDualWield; //Q: Should this be a learnable spell like in wow?
        bool canDualWield;

        public bool IsCaster => isCaster;
        bool isCaster;

        public bool CanParry => canParry;
        bool canParry;

        //Data from Wowhead
        //Druids, Paladins, Shaman and Warriors receive 1% Critical Strike Chance for every 20 points of Agility.
        //Rogues receive 1% Critical Strike Chance for every 29 points of Agility.
        //Hunters receive 1% Critical Strike Chance for every 53 points of Agility.

        //Q: Isn't it scalar?
        public float AttackCritChanceScaler => attackCritChanceScaler;
        float attackCritChanceScaler;


        //Data from Wowhead
        //Warlocks receive 1% Spell Critical Strike chance for every 60.6 points of intellect.
        //Druids receive 1% Spell Critical Strike chance for every 60 points of intellect.
        //Shamans receive 1% Spell Critical Strike chance for every 59.5 points of intellect.
        //Mages receive 1% Spell Critical Strike chance for every 59.5 points of intellect.
        //Priests receive 1% Spell Critical Strike chance for every 59.2 points of intellect.
        //Paladins receive 1% Spell Critical Strike chance for every 54 points of intellect.


        public float SpellCritChanceScaler => spellCritChanceScaler;
        float spellCritChanceScaler = 0f;

        //Data from Wowhead
        //All classes but Hunters and Rogues receive 1% Dodge for every 20 points of Agility.
        //Rogues receive 1% Dodge for every 14.5 points of Agility.
        //Hunters receive 1% Dodge for every 26 points of Agility.
        public float AgilityDodgeChanceScaler => agilityDodgeChanceScaler;
        float agilityDodgeChanceScaler;

        public float BaseDodge => baseDodge;
        float baseDodge;

        //Class	    Base dodge	AGI:Dodge ratio
        //Druid	    0.9%	    20
        //Hunter	0.0%	    26.5
        //Mage	    3.2%	    19.444
        //Paladin	0.7%	    19.767
        //Priest	3.0%	    20
        //Rogue	    0.0%	    14.5
        //Shaman	1.7%	    19.697
        //Warlock	2.0%	    20
        //Warrior	0.0%	    20

        public int MaxTalents
        {
            get
            {
                int max = 0;
                foreach (TalentTree tree in TalentTrees)
                {
                    for (global::System.Int32 i = 0; i < tree.Talents.Length; i++)
                    {
                        max += tree.Talents[i].Length;
                    }
                }
                return max;
            }
        }

        public TalentTree[] TalentTrees => talentTrees;
        TalentTree[] talentTrees;

        public (int id, int rank)[] GenerateEmptyTalents
        {
            get
            {
                (int id, int rank)[] returnable = new (int, int)[MaxTalents];
                int index = 0;
                for (int i = 0; i < talentTrees.Length; i++)
                {
                    int[] talents = talentTrees[i].GetIds;
                    for (int j = 0; j < talents.Length; j++)
                    {
                        returnable[index].id = talents[j];
                        returnable[index].rank = 0;
                        index++;
                    }
                }
                return returnable;
            }
        }


        [JsonConstructor]
        public ClassData(string name, int[] talentTrees, Resource.ResourceType resource, int[] baseStats, int[] perLevelStats, int baseHp, int perLevelHp,
            float spiritHp5Constant, float spiritHp5Scaling,
            float fistAttackSpeed, float fistMinAttackDamage, float fistMaxAttackDamage, float speed, float maxSpeed, MeleeAttackPowerBonus meleeAttackPowerBonus,
            float agilityDodgeScaling = float.NaN, float baseDodge = 0f, float meleeCritScaling = 0f,
            Weapon.WeaponType weaponsAllowed = Weapon.WeaponType.None, bool canDualWield = false, bool isCaster = false, bool canParry = false, float spellCritScaling = 0f, float dodgeScaling = float.NaN)
        {
            talentTrees ??= Array.Empty<int>();
            this.name = name;
            this.talentTrees = talentTrees.Select(id => TalentFactory.GetTalentTree(id)).ToArray();
            this.resource = resource;
            this.baseStats = new PrimaryStats(baseStats);
            this.perLevelStats = new PrimaryStats(perLevelStats);
            this.baseHp = baseHp;
            this.perLevelHp = perLevelHp;
            this.spiritHp5Constant = spiritHp5Constant;
            this.spiritHp5Scaling = spiritHp5Scaling;
            this.fistMinAttackDamage = fistMinAttackDamage;
            this.fistMaxAttackDamage = fistMaxAttackDamage;
            this.fistAttackSpeed = fistAttackSpeed;
            this.meleeAttackPowerBonus = meleeAttackPowerBonus;
            movementData = new Movement(speed, maxSpeed);
            this.agilityDodgeChanceScaler = !float.IsNaN(agilityDodgeScaling)
                ? agilityDodgeScaling
                : !float.IsNaN(dodgeScaling)
                    ? dodgeScaling
                    : 0.01f / 20f;
            this.baseDodge = baseDodge;
            this.attackCritChanceScaler = meleeCritScaling;
            this.spellCritChanceScaler = spellCritScaling;
            this.weaponsAllowed = weaponsAllowed;
            this.canDualWield = canDualWield;
            this.isCaster = isCaster;
            this.canParry = canParry;
        }

       

        protected virtual void Assert()
        {
            Debug.Assert(name != null && baseHp > 0 && perLevelHp > 0 && baseStats != null && perLevelStats != null && fistMinAttackDamage > 0 && fistAttackSpeed > 0);
        }
    }
}
