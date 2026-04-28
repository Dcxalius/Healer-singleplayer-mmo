using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Textures;
using Project_1.UI.CharacterCreator;
using System.Threading;

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

        public EntityUiSnapshot WithLevel(int level)
        {
            return new EntityUiSnapshot(
                RenderId,
                Name,
                ClassName,
                RelationToPlayer,
                RelationColor,
                level,
                CurrentHealth,
                MaxHealth,
                CurrentResource,
                MaxResource,
                ResourceColor,
                FeetPosition,
                WorldHeight);
        }
    }

    internal readonly struct SpellUiSnapshot
    {
        public SpellUiSnapshot(string name, GfxPath gfxPath, bool offCooldown, double cooldownRatio01, SpellDescriptorSnapshot descriptor)
        {
            Name = name ?? string.Empty;
            GfxPath = gfxPath;
            OffCooldown = offCooldown;
            CooldownRatio01 = cooldownRatio01;
            Descriptor = descriptor;
        }

        public string Name { get; }
        public GfxPath GfxPath { get; }
        public bool OffCooldown { get; }
        public double CooldownRatio01 { get; }
        public SpellDescriptorSnapshot Descriptor { get; }
    }

    internal readonly struct BuffUiSnapshot
    {
        public BuffUiSnapshot(int buffId, GfxPath gfxPath, double finalTime, int count, int maxCount)
        {
            BuffId = buffId;
            Count = count;
            MaxCount = maxCount;
            GfxPath = gfxPath;
            FinalTime = finalTime;
        }

        public int MaxCount { get; }
        public int Count { get; }
        public int BuffId { get; }
        public GfxPath GfxPath { get; }
        public double FinalTime { get; }
    }
}
