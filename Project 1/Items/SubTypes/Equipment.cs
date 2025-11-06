using Newtonsoft.Json;
using Project_1.GameObjects.Unit;
using Project_1.GameObjects.Unit.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Items.SubTypes
{
    internal class Equipment : Item
    {
        public enum Type //TODO: Find better name
        {
            Head,
            Neck,
            Shoulders,
            Back,
            Chest,
            Wrist,
            Hands,
            Belt,
            Legs,
            Feet,
            Finger,
            Trinket,
            OneHander,
            MainHander,
            OffHander,
            TwoHander,
            Ranged,
            Count
        }

        public enum SecondaryStatsOnItems
        {
            BonusDamage,
            Armor,
            Hit,
            Crit,
            Vampirism,
            Dodge,
            Parry,
            Block,

            BaseSpellDamage,
            SpellCrit,
            ArcaneSpellDamage,
            FireSpellDamage,
            FrostSpellDamage,
            HolySpellDamage,
            NatureSpellDamage,
            ShadowSpellDamage,
            ArcaneCrit,
            FireCrit,
            FrostCrit,
            HolyCrit,
            NatureCrit,
            ShadowCrit,
            SpellVamp,
        }

        public enum GearType
        {
            Cloth,
            Leather,
            Mail,
            Plate,
            Count,
            None
        }
        [JsonIgnore]
        public EquipmentData EquipmentData => itemData as EquipmentData;

        [JsonIgnore]
        public EquipmentStats Stats => EquipmentData.BaseStats;
        [JsonIgnore]
        public Type type { get => EquipmentData.Slot; }
        [JsonIgnore]
        public GearType Material => EquipmentData.Material;
        [JsonIgnore]
        public PairReport StatReport => EquipmentData.StatReport;

        [JsonConstructor]
        Equipment(int id) : this(ItemFactory.GetItemData<EquipmentData>(id)) { }


        public Equipment(LootData aLoot) : base(aLoot)
        {
        }

        public Equipment(EquipmentData aData) : base(aData, 1)
        {
        }
    }
}
