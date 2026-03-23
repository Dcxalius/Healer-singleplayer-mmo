namespace Project_1.Messaging.Events
{
    internal readonly struct SpellTrainingWindowRequested
    {
        public static readonly SpellTrainingWindowRequested Instance = new SpellTrainingWindowRequested();
    }

    internal readonly struct SpellTrainingPurchaseRequested
    {
        public SpellTrainingPurchaseRequested(string spellKey)
        {
            SpellKey = spellKey ?? string.Empty;
        }

        public string SpellKey { get; }
    }
}
