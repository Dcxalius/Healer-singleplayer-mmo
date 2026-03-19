namespace Project_1.Messaging.Events
{
    internal readonly struct SpellCastRequested
    {
        public SpellCastRequested(string spellName)
        {
            SpellName = spellName;
        }

        public string SpellName { get; }
    }
}
