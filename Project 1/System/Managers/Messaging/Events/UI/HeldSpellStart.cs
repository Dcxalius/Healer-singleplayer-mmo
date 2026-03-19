using Project_1.Camera;

namespace Project_1.Messaging.Events
{
    internal readonly struct HeldSpellStart
    {
        public HeldSpellStart(string spellName, AbsoluteScreenPosition grabOffset)
        {
            SpellName = spellName;
            GrabOffset = grabOffset;
        }

        public string SpellName { get; }
        public AbsoluteScreenPosition GrabOffset { get; }
    }
}
