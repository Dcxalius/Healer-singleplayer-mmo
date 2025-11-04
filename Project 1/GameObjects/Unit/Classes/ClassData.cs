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
        
        public bool[] w
        {
            get
            {
                bool[] ww = new bool[Enum.GetValues<Weapon.WeaponType>().Length];
                for (int i = 0; i < ww.Length; i++)
                {
                    ww[i] = (int)weaponsAllowed % (2 ^ (i + 1)) == 0;
                }

                return ww;
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

        public float HpPer5 => baseHpPer5;
        readonly float baseHpPer5;

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

        public float DodgeScaling => dodgeScaling;
        float dodgeScaling;

        public bool CanDualWield => canDualWield;
        bool canDualWield;

        public bool IsCaster => isCaster;
        bool isCaster;

//        Druids, Paladins, Shaman and Warriors receive 1% Critical Strike Chance for every 20 points of Agility.
//Rogues receive 1% Critical Strike Chance for every 29 points of Agility.
//Hunters receive 1% Critical Strike Chance for every 53 points of Agility.

        public double AttackCritChanceScaler => attackCritChanceScaler;
        double attackCritChanceScaler;


        //        You gain Critical Strike chance at varying points, depending on your class:
        //Warlocks receive 1% Spell Critical Strike chance for every 60.6 points of intellect.
        //Druids receive 1% Spell Critical Strike chance for every 60 points of intellect.
        //Shamans receive 1% Spell Critical Strike chance for every 59.5 points of intellect.
        //Mages receive 1% Spell Critical Strike chance for every 59.5 points of intellect.
        //Priests receive 1% Spell Critical Strike chance for every 59.2 points of intellect.
        //Paladins receive 1% Spell Critical Strike chance for every 54 points of intellect.


        public double SpellCritChanceScaler => spellCritChanceScaler;
        double spellCritChanceScaler;

        //All classes but Hunters and Rogues receive 1% Dodge for every 20 points of Agility.
        //Rogues receive 1% Dodge for every 14.5 points of Agility.
        //Hunters receive 1% Dodge for every 26 points of Agility.

        public double DodgeChanceScaler => dodgeChanceScaler;
        double dodgeChanceScaler;

        [JsonConstructor]
        public ClassData(string name, Resource.ResourceType resource, int[] baseStats, int[] perLevelStats, int baseHp, int perLevelHp, float baseHpPer5,
            float fistAttackSpeed, float fistMinAttackDamage, float fistMaxAttackDamage, float speed, float maxSpeed, MeleeAttackPowerBonus meleeAttackPowerBonus, float dodgeScaling, float meleeCritScaling,
            Weapon.WeaponType weaponsAllowed, bool canDualWield, bool isCaster)
        {
            this.name = name;
            this.resource = resource;
            this.baseStats = new PrimaryStats(baseStats);
            this.perLevelStats = new PrimaryStats(perLevelStats);
            this.baseHp = baseHp;
            this.perLevelHp = perLevelHp;
            this.baseHpPer5 = baseHpPer5;
            this.fistMinAttackDamage = fistMinAttackDamage;
            this.fistMaxAttackDamage = fistMaxAttackDamage;
            this.fistAttackSpeed = fistAttackSpeed;
            this.meleeAttackPowerBonus = meleeAttackPowerBonus;
            movementData = new Movement(speed, maxSpeed);
            this.dodgeScaling = dodgeScaling == 0 ? 0.01f / 20f : dodgeScaling;
            this.meleeCritScaling = meleeCritScaling == 0 ? 0.01f / 20f : meleeCritScaling;
            this.weaponsAllowed = weaponsAllowed;
            this.canDualWield = canDualWield;
            this.isCaster = isCaster;

        }

       

        protected virtual void Assert()
        {
            Debug.Assert(name != null && baseHp > 0 && perLevelHp > 0 && baseStats != null && perLevelStats != null && baseHpPer5 > 0 && fistMinAttackDamage > 0 && fistAttackSpeed > 0);
        }
    }
}
