using Newtonsoft.Json;
using Project_1.GameObjects.Unit;
using Project_1.GameObjects.Unit.Stats;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
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


        [JsonProperty]
        public int Hash => hash;
        int hash;

        [JsonIgnore]
        public override string Name
        {
            get
            {
                if (ItemQuality != Quality.Uncommon) return EquipmentData.Name;
                return $"{EquipmentData.Name} {EquipmentData.Suffix(Hash).name}";
            }
        }

        [JsonIgnore]
        public (string name, (PrimaryStats.PrimaryStat stat, int value)[] stats) Suffix => suffix;
        (string name, (PrimaryStats.PrimaryStat stat, int value)[] stats) suffix;
            
        

        [JsonIgnore]
        public EquipmentStats Stats => EquipmentData.BaseStats;
        [JsonIgnore]
        public Type type { get => EquipmentData.Slot; }
        [JsonIgnore]
        public GearType Material => EquipmentData.Material;
        [JsonIgnore]
        public PairReport StatReport
        {
            get
            {
                var report = EquipmentData.StatReport;
                if (ItemQuality == Item.Quality.Uncommon)
                {
                    var suffix = Suffix;
                    foreach (var stat in suffix.stats)
                    {
                        report.AddLine(stat.stat.ToString(), stat.value);
                    }
                }
                return report;
            }
        }

        [JsonIgnore]
        public SecondayStatBonus<int>[] SecondaryStatsInt => EquipmentData.SecondayStatsInt;

        [JsonIgnore]
        public SecondayStatBonus<float>[] SecondaryStatsFloat => EquipmentData.SecondayStatsFloat;

        [JsonConstructor]
        Equipment(int id, int hash) : this(ItemFactory.GetItemData<EquipmentData>(id))
        {
            this.hash = hash;
            suffix = EquipmentData.Suffix(hash);
        }


        public Equipment(LootData aLoot) : base(aLoot)
        {
            hash = RandomManager.RollInt();
            suffix = EquipmentData.Suffix(hash);
        }

        public Equipment(EquipmentData aData) : base(aData, 1)
        {
            hash = RandomManager.RollInt();
            suffix = EquipmentData.Suffix(hash);
        }
    }
}
