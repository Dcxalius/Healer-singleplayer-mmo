using Microsoft.Xna.Framework;
using Project_1.GameObjects.Spells;
using Project_1.Items;

namespace Project_1.Messaging.Events
{
    internal readonly struct HeldItemStart
    {
        public HeldItemStart(object source, AbsoluteScreenPosition grabOffset)
        {
            Source = source;
            GrabOffset = grabOffset;
        }
        public object Source { get; }
        public AbsoluteScreenPosition GrabOffset { get; }
    }

    internal readonly struct HeldItemEnd
    {
    }

    internal readonly struct HeldSpellStart
    {
        public HeldSpellStart(Spell spell, AbsoluteScreenPosition grabOffset)
        {
            Spell = spell;
            GrabOffset = grabOffset;
        }
        public Spell Spell { get; }
        public AbsoluteScreenPosition GrabOffset { get; }
    }

    internal readonly struct HeldSpellEnd
    {
    }
}
