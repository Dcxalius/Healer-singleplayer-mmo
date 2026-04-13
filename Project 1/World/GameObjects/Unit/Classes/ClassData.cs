using Newtonsoft.Json;
using Project_1.GameObjects.Unit.Resources;
using Project_1.GameObjects.Unit.Stats;
using Project_1.Items.SubTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_1.GameObjects.Entities.Friendlies.Players;
using Project_1.World.GameObjects.Unit.Talents;

namespace Project_1.GameObjects.Unit.Classes
{
    internal class ClassData
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

        

        public bool WeaponUsuable(Weapon.WeaponType aType) => WeaponsAllowed.HasFlag(aType);
        
        public bool[] skillAsBools
        {
            get
            {
                bool[] bools = new bool[Enum.GetValues<Weapon.WeaponType>().Length];
                for (int i = 0; i < bools.Length; i++)
                {
                    bools[i] = (int)weaponsAllowed % (Math.Pow(2, i)) == 0;
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

        public PrimaryStats BaseStats => baseStats;
        readonly PrimaryStats baseStats;

        public PrimaryStats PerLevelStats => perLevelStats;
        readonly PrimaryStats perLevelStats;

        public int BaseHealth => baseHp;
        readonly int baseHp;

        public int PerLevelHp => perLevelHp;
        readonly int perLevelHp;

        public float SpiritHp5Constant => spiritHp5Constant;
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

        public MeleeAttackPowerBonus MeleeAttackBonus => meleeAttackPowerBonus;
        MeleeAttackPowerBonus meleeAttackPowerBonus;

        // Backward-compatible alias. Prefer AgilityDodgeChanceScaler.
        public float DodgeScaling => agilityDodgeChanceScaler;

        public bool CanDualWield => canDualWield;
        bool canDualWield;

        public bool IsCaster => isCaster;
        bool isCaster;

        public bool CanParry => canParry;
        bool canParry;

        //        Druids, Paladins, Shaman and Warriors receive 1% Critical Strike Chance for every 20 points of Agility.
        //Rogues receive 1% Critical Strike Chance for every 29 points of Agility.
        //Hunters receive 1% Critical Strike Chance for every 53 points of Agility.

        public float AttackCritChanceScaler => attackCritChanceScaler;
        float attackCritChanceScaler;


        //        You gain Critical Strike chance at varying points, depending on your class:
        //Warlocks receive 1% Spell Critical Strike chance for every 60.6 points of intellect.
        //Druids receive 1% Spell Critical Strike chance for every 60 points of intellect.
        //Shamans receive 1% Spell Critical Strike chance for every 59.5 points of intellect.
        //Mages receive 1% Spell Critical Strike chance for every 59.5 points of intellect.
        //Priests receive 1% Spell Critical Strike chance for every 59.2 points of intellect.
        //Paladins receive 1% Spell Critical Strike chance for every 54 points of intellect.


        public float SpellCritChanceScaler => spellCritChanceScaler;
        float spellCritChanceScaler = 0f;

        //All classes but Hunters and Rogues receive 1% Dodge for every 20 points of Agility.
        //Rogues receive 1% Dodge for every 14.5 points of Agility.
        //Hunters receive 1% Dodge for every 26 points of Agility.

        public float DodgeChanceScaler => agilityDodgeChanceScaler;
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
