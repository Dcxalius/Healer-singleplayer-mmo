using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Textures;

namespace Project_1.Messaging.Events
{
    internal readonly struct EntityUiSnapshot
    {
        public EntityUiSnapshot(
            int renderId,
            string name,
            string className,
            RelationToPlayerKind relationToPlayer,
            Color relationColor,
            int level,
            double currentHealth,
            double maxHealth,
            float currentResource,
            float maxResource,
            Color resourceColor,
            WorldSpace feetPosition,
            int worldHeight)
        {
            RenderId = renderId;
            Name = name;
            ClassName = className;
            RelationToPlayer = relationToPlayer;
            RelationColor = relationColor;
            Level = level;
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
            CurrentResource = currentResource;
            MaxResource = maxResource;
            ResourceColor = resourceColor;
            FeetPosition = feetPosition;
            WorldHeight = worldHeight;
        }

        public int RenderId { get; }
        public string Name { get; }
        public string ClassName { get; }
        public RelationToPlayerKind RelationToPlayer { get; }
        public Color RelationColor { get; }
        public int Level { get; }
        public double CurrentHealth { get; }
        public double MaxHealth { get; }
        public float CurrentResource { get; }
        public float MaxResource { get; }
        public Color ResourceColor { get; }
        public WorldSpace FeetPosition { get; }
        public int WorldHeight { get; }
    }

    internal readonly struct SpellUiSnapshot
    {
        public SpellUiSnapshot(string name, GfxPath gfxPath, bool offCooldown, double cooldownRatio01)
        {
            Name = name ?? string.Empty;
            GfxPath = gfxPath;
            OffCooldown = offCooldown;
            CooldownRatio01 = cooldownRatio01;
        }

        public string Name { get; }
        public GfxPath GfxPath { get; }
        public bool OffCooldown { get; }
        public double CooldownRatio01 { get; }
    }

    internal readonly struct BuffUiSnapshot
    {
        public BuffUiSnapshot(int effectId, GfxPath gfxPath, double durationRemainingMs)
        {
            EffectId = effectId;
            GfxPath = gfxPath;
            DurationRemainingMs = durationRemainingMs;
        }

        public int EffectId { get; }
        public GfxPath GfxPath { get; }
        public double DurationRemainingMs { get; }
    }
}
