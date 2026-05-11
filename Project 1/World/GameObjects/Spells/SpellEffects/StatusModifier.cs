using Newtonsoft.Json;

namespace Project_1.World.GameObjects.Spells.SpellEffects
{
    internal readonly struct StatusModifier
    {
        public string Stat => stat;
        readonly string stat;

        public double Amount => amount;
        readonly double amount;

        public bool Flat => flat;
        readonly bool flat;

        public string Category => category;
        readonly string category;

        [JsonConstructor]
        public StatusModifier(string stat, double amount, bool flat = true, string category = null)
        {
            this.stat = stat;
            this.amount = amount;
            this.flat = flat;
            this.category = category;
        }
    }
}
