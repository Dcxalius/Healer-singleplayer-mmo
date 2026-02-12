using Newtonsoft.Json;
using Project_1.GameObjects.Unit;
using System;
using System.Diagnostics;

namespace Project_1.Items.SubTypes
{
    internal class WeaponData : EquipmentData
    {
        const double OneHandGreenDpsSlope = 0.6;
        const double OneHandGreenDpsOffset = 26.6;
        const int OneHandGreenDpsBaseIlvl = 45;

        const double TwoHandTypeMultiplier = 1.305;
        const double RangedTypeMultiplier = 1.275;
        const double WandTypeMultiplier = 1.522;

        const double RareQualityMultiplier = 1.105;
        const double EpicQualityMultiplier = 1.215;

        const double GreenShieldArmorSlope = 85.0 / 3.0;
        const double GreenShieldArmorOffset = 133.0;
        const double RareShieldArmorMultiplier = 1.122;
        const double EpicShieldArmorMultiplier = 1.436;

        public Attack Attack => attack;
        Attack attack;

        public Weapon.WeaponType WeaponType => weaponType;
        Weapon.WeaponType weaponType;

        public float AttackSpeed => attackSpeed;
        float attackSpeed;

        public double DamageRangeFactor => damageRangeFactor;
        double damageRangeFactor;

        public float MinAttackDamage => minAttackDamage;
        float minAttackDamage;

        public float MaxAttackDamage => maxAttackDamage;
        float maxAttackDamage;

        [JsonConstructor]
        public WeaponData(
            int id,
            string gfxName,
            string name,
            string description,
            Equipment.Type slot,
            float attackSpeed = 2.0f,
            double damageRangeFactor = 0.8,
            Item.Quality quality = Item.Quality.Uncommon,
            Weapon.WeaponType weaponType = Weapon.WeaponType.None,
            int cost = 0,
            int itemLevel = 1)
            : base(id, gfxName, name, description, slot, ItemType.Weapon, quality, cost, Equipment.GearType.None, Array.Empty<SecondayStatBonus<int>>(), Array.Empty<SecondayStatBonus<float>>(), itemLevel)
        {
            this.weaponType = weaponType;
            this.attackSpeed = attackSpeed;
            this.damageRangeFactor = damageRangeFactor;

            SetBaseStats(Array.Empty<int>(), CalculateShieldArmor(itemLevel, quality, weaponType));
            attack = BuildAttack();
            Debug.Assert(weaponType != Weapon.WeaponType.None);
        }

        Attack BuildAttack()
        {
            if (!HasWeaponDamage(weaponType))
            {
                minAttackDamage = 0;
                maxAttackDamage = 0;
                return new Attack(0, 0, 1, weaponType);
            }

            if (attackSpeed <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(attackSpeed), "Attack speed must be greater than 0 for damaging weapon types.");
            }

            if (damageRangeFactor <= 0 || damageRangeFactor >= 2)
            {
                throw new ArgumentOutOfRangeException(nameof(damageRangeFactor), "DamageRangeFactor must be greater than 0 and less than 2.");
            }

            double expectedDps = ComputeExpectedDps(ItemLevel, Quality, weaponType);
            double averageHit = expectedDps * attackSpeed;

            minAttackDamage = (float)(averageHit * damageRangeFactor);
            maxAttackDamage = (float)(averageHit * (2 - damageRangeFactor));

            return new Attack(minAttackDamage, maxAttackDamage, attackSpeed, weaponType);
        }

        static bool HasWeaponDamage(Weapon.WeaponType aWeaponType)
        {
            return aWeaponType switch
            {
                Weapon.WeaponType.Shield or Weapon.WeaponType.Holdable or Weapon.WeaponType.None => false,
                _ => true
            };
        }

        static double ComputeExpectedDps(int aItemLevel, Item.Quality aQuality, Weapon.WeaponType aWeaponType)
        {
            double greenOneHandBaselineDps = (aItemLevel - OneHandGreenDpsBaseIlvl) * OneHandGreenDpsSlope + OneHandGreenDpsOffset;
            return greenOneHandBaselineDps * GetTypeMultiplier(aWeaponType) * GetQualityMultiplier(aQuality);
        }

        static double GetTypeMultiplier(Weapon.WeaponType aWeaponType)
        {
            if (IsTwoHandMelee(aWeaponType))
            {
                return TwoHandTypeMultiplier;
            }

            if (IsRanged(aWeaponType))
            {
                return RangedTypeMultiplier;
            }

            if (aWeaponType == Weapon.WeaponType.Wand)
            {
                return WandTypeMultiplier;
            }

            return 1.0;
        }

        static bool IsTwoHandMelee(Weapon.WeaponType aWeaponType)
        {
            return aWeaponType == Weapon.WeaponType.TwoHandedSword
                || aWeaponType == Weapon.WeaponType.TwoHandedAxe
                || aWeaponType == Weapon.WeaponType.TwoHandedMace
                || aWeaponType == Weapon.WeaponType.Staff;
        }

        static bool IsRanged(Weapon.WeaponType aWeaponType)
        {
            return aWeaponType == Weapon.WeaponType.Bow
                || aWeaponType == Weapon.WeaponType.Gun
                || aWeaponType == Weapon.WeaponType.Thrown;
        }

        static double GetQualityMultiplier(Item.Quality aQuality)
        {
            return aQuality switch
            {
                Item.Quality.Rare => RareQualityMultiplier,
                Item.Quality.Epic => EpicQualityMultiplier,
                _ => 1.0
            };
        }

        static int CalculateShieldArmor(int aItemLevel, Item.Quality aQuality, Weapon.WeaponType aWeaponType)
        {
            if (aWeaponType != Weapon.WeaponType.Shield)
            {
                return 0;
            }

            double greenShieldArmor = (GreenShieldArmorSlope * aItemLevel) + GreenShieldArmorOffset;
            double qualityMultiplier = aQuality switch
            {
                Item.Quality.Rare => RareShieldArmorMultiplier,
                Item.Quality.Epic => EpicShieldArmorMultiplier,
                Item.Quality.Legendary => EpicShieldArmorMultiplier,
                _ => 1.0
            };

            return Math.Max(0, (int)Math.Round(greenShieldArmor * qualityMultiplier, MidpointRounding.AwayFromZero));
        }
    }
}
