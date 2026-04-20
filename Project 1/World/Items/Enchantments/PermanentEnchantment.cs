using Newtonsoft.Json;

namespace Project_1.World.Items.Enchantments
{
    internal class PermanentEnchantment : Enchantment
    {
        [JsonConstructor]
        public PermanentEnchantment(int dataId) : base(dataId)
        {
        }

        public PermanentEnchantment(EnchantmentData data) : base(data)
        {
        }
    }
}
