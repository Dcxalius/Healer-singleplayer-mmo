using Newtonsoft.Json;
using Project_1.Items;
using Project_1.Managers;

namespace Project_1.World.Items.Enchantments
{
    internal class TemporaryEnchantment : Enchantment
    {
        [JsonProperty]
        double timeApplied;
        [JsonProperty]
        double duration;

        public double TimeApplied => timeApplied;
        public double Duration => duration;

        public TemporaryEnchantment(EnchantmentData data, double duration) : base(data)
        {
            this.duration = duration;
            timeApplied = TimeManager.TotalFrameTime;
        }

        [JsonConstructor]
        public TemporaryEnchantment(int dataId, double duration, double timeApplied) : base(dataId)
        {
            this.duration = duration;
            this.timeApplied = timeApplied;
        }

        public void OnApplication(Item item)
        {
            timeApplied = TimeManager.TotalFrameTime;
        }

        public void Update()
        {
            if (TimeManager.TotalFrameTime - timeApplied > duration)
            {
                // Remove the enchantment
            }
        }
    }
}
