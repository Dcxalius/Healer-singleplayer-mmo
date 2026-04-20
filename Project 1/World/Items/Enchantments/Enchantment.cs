using Newtonsoft.Json;
using Project_1.GameObjects.Unit.Stats;
using Project_1.Items.SubTypes;
using System;

namespace Project_1.World.Items.Enchantments
{
    internal class Enchantment
    {
        protected EnchantmentData data;

        public int DataId => data.Id;
        public string Name => data.Name;
        [JsonIgnore]
        public PrimaryStats PrimaryStats => data.PrimaryStats;
        [JsonIgnore]
        public SecondayStatBonus<int>[] SecondaryStatsInt => data.SecondaryStatsInt;
        [JsonIgnore]
        public SecondayStatBonus<float>[] SecondaryStatsFloat => data.SecondaryStatsFloat;

        protected Enchantment(int dataId) : this(EnchantmentFactory.GetData(dataId))
        {
        }

        protected Enchantment(EnchantmentData data)
        {
            this.data = data ?? throw new ArgumentNullException(nameof(data));
        }
    }
}
