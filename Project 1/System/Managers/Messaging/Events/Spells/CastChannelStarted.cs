using Project_1.Textures;

namespace Project_1.Messaging.Events
{
    internal readonly struct CastChannelStarted
    {
        public CastChannelStarted(int casterRenderId, string spellName, GfxPath spellGfxPath, double durationMs)
        {
            CasterRenderId = casterRenderId;
            SpellName = spellName;
            SpellGfxPath = spellGfxPath;
            DurationMs = durationMs;
        }

        public int CasterRenderId { get; }
        public string SpellName { get; }
        public GfxPath SpellGfxPath { get; }
        public double DurationMs { get; }
    }
}
