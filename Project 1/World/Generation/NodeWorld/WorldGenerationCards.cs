using System;
using System.Collections.Generic;
using System.Linq;

namespace Project_1.WorldGeneration
{
    internal enum WorldGenerationCardType
    {
        Terrain,
        Race,
        Class,
        Wildcard,
        Faction
    }

    internal enum WorldGenerationClassRole
    {
        None,
        Tank,
        Dps,
        Flex
    }

    internal enum WorldGenerationClassPool
    {
        Player,
        Npc
    }

    internal sealed class WorldGenerationCardModifiers
    {
        public float MoistureBiasBonus { get; init; }
        public float HeatBiasBonus { get; init; }
        public float RuggednessBiasBonus { get; init; }
        public float DuneStrengthBonus { get; init; }
        public float CliffStrengthBonus { get; init; }
        public float BeachWidthBonus { get; init; }
        public float HeightScaleMultiplier { get; init; } = 1f;

        public static WorldGenerationCardModifiers operator +(WorldGenerationCardModifiers left, WorldGenerationCardModifiers right)
        {
            if (left == null) return right ?? new WorldGenerationCardModifiers();
            if (right == null) return left;

            return new WorldGenerationCardModifiers
            {
                MoistureBiasBonus = left.MoistureBiasBonus + right.MoistureBiasBonus,
                HeatBiasBonus = left.HeatBiasBonus + right.HeatBiasBonus,
                RuggednessBiasBonus = left.RuggednessBiasBonus + right.RuggednessBiasBonus,
                DuneStrengthBonus = left.DuneStrengthBonus + right.DuneStrengthBonus,
                CliffStrengthBonus = left.CliffStrengthBonus + right.CliffStrengthBonus,
                BeachWidthBonus = left.BeachWidthBonus + right.BeachWidthBonus,
                HeightScaleMultiplier = left.HeightScaleMultiplier * right.HeightScaleMultiplier
            };
        }
    }

    internal class WorldGenerationCardDefinition
    {
        public WorldGenerationCardDefinition(string id, string name, WorldGenerationCardType type, string description, WorldGenerationCardModifiers modifiers = null)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type;
            Description = description ?? string.Empty;
            Modifiers = modifiers ?? new WorldGenerationCardModifiers();
        }

        public string Id { get; }
        public string Name { get; }
        public WorldGenerationCardType Type { get; }
        public string Description { get; }
        public WorldGenerationCardModifiers Modifiers { get; }
    }

    internal sealed class RaceCardDefinition : WorldGenerationCardDefinition
    {
        public RaceCardDefinition(string id, string name, string cultureAdjective, string description, WorldGenerationCardModifiers modifiers = null)
            : base(id, name, WorldGenerationCardType.Race, description, modifiers)
        {
            CultureAdjective = cultureAdjective ?? name;
        }

        public string CultureAdjective { get; }
    }

    internal sealed class ClassCardDefinition : WorldGenerationCardDefinition
    {
        public ClassCardDefinition(
            string id,
            string name,
            WorldGenerationClassPool pool,
            string sourceClassName,
            IEnumerable<WorldGenerationClassRole> supportedRoles,
            string description,
            string mobClassName = null)
            : base(id, name, WorldGenerationCardType.Class, description)
        {
            Pool = pool;
            SourceClassName = sourceClassName ?? throw new ArgumentNullException(nameof(sourceClassName));
            MobClassName = string.IsNullOrWhiteSpace(mobClassName) ? sourceClassName : mobClassName;
            SupportedRoles = supportedRoles?.Distinct().ToArray() ?? Array.Empty<WorldGenerationClassRole>();
        }

        public WorldGenerationClassPool Pool { get; }
        public string SourceClassName { get; }
        public string MobClassName { get; }
        public IReadOnlyList<WorldGenerationClassRole> SupportedRoles { get; }

        public bool SupportsRole(WorldGenerationClassRole role)
        {
            return SupportedRoles.Contains(role);
        }
    }

    internal sealed class FactionCardDefinition : WorldGenerationCardDefinition
    {
        public FactionCardDefinition(
            string id,
            string name,
            string description,
            IEnumerable<string> requiredClassCardIds,
            string mobFactionName)
            : base(id, name, WorldGenerationCardType.Faction, description)
        {
            RequiredClassCardIds = requiredClassCardIds?.Distinct(StringComparer.OrdinalIgnoreCase).ToArray() ?? Array.Empty<string>();
            MobFactionName = mobFactionName ?? name;
        }

        public IReadOnlyList<string> RequiredClassCardIds { get; }
        public string MobFactionName { get; }
    }

    internal sealed class WorldGenerationSlotConfiguration
    {
        public int MainDrawSlots { get; init; } = 2;
        public int FactionDrawSlots { get; init; } = 2;
        public int PlayerClassSlots { get; init; } = 5;
        public int NpcTankClassSlots { get; init; } = 3;
        public int NpcDpsClassSlots { get; init; } = 6;
        public int NpcFlexClassSlots { get; init; } = 1;
    }

    internal sealed class WorldGenerationCatalog
    {
        readonly Dictionary<string, WorldGenerationCardDefinition> cardsById = new Dictionary<string, WorldGenerationCardDefinition>(StringComparer.OrdinalIgnoreCase);

        public List<WorldGenerationCardDefinition> TerrainCards { get; } = new List<WorldGenerationCardDefinition>();
        public List<RaceCardDefinition> RaceCards { get; } = new List<RaceCardDefinition>();
        public List<WorldGenerationCardDefinition> WildcardCards { get; } = new List<WorldGenerationCardDefinition>();
        public List<ClassCardDefinition> ClassCards { get; } = new List<ClassCardDefinition>();
        public List<FactionCardDefinition> FactionCards { get; } = new List<FactionCardDefinition>();

        public IEnumerable<WorldGenerationCardDefinition> MainDeck
        {
            get
            {
                foreach (WorldGenerationCardDefinition card in TerrainCards) yield return card;
                foreach (RaceCardDefinition card in RaceCards) yield return card;
                foreach (WorldGenerationCardDefinition card in WildcardCards) yield return card;
            }
        }

        public void Add(WorldGenerationCardDefinition card)
        {
            if (card == null) throw new ArgumentNullException(nameof(card));

            cardsById[card.Id] = card;
            switch (card.Type)
            {
                case WorldGenerationCardType.Terrain:
                    TerrainCards.Add(card);
                    break;
                case WorldGenerationCardType.Race:
                    RaceCards.Add((RaceCardDefinition)card);
                    break;
                case WorldGenerationCardType.Class:
                    ClassCards.Add((ClassCardDefinition)card);
                    break;
                case WorldGenerationCardType.Wildcard:
                    WildcardCards.Add(card);
                    break;
                case WorldGenerationCardType.Faction:
                    FactionCards.Add((FactionCardDefinition)card);
                    break;
            }
        }

        public bool TryGetCard(string id, out WorldGenerationCardDefinition card)
        {
            return cardsById.TryGetValue(id, out card);
        }
    }

    internal sealed class WorldGenerationRecipe
    {
        public int WorldOrdinal { get; init; } = 1;
        public WorldGenerationSlotConfiguration Slots { get; init; } = new WorldGenerationSlotConfiguration();
        public WorldGenerationCatalog Catalog { get; init; }
        public List<WorldGenerationCardDefinition> DrawnMainCards { get; } = new List<WorldGenerationCardDefinition>();
        public List<WorldGenerationCardDefinition> SelectedMainCards { get; } = new List<WorldGenerationCardDefinition>();
        public List<ClassCardDefinition> SelectedPlayerClassCards { get; } = new List<ClassCardDefinition>();
        public List<ClassCardDefinition> SelectedNpcTankClassCards { get; } = new List<ClassCardDefinition>();
        public List<ClassCardDefinition> SelectedNpcDpsClassCards { get; } = new List<ClassCardDefinition>();
        public List<ClassCardDefinition> SelectedNpcFlexClassCards { get; } = new List<ClassCardDefinition>();
        public List<FactionCardDefinition> DrawnFactionCards { get; } = new List<FactionCardDefinition>();
        public WorldGenerationCardModifiers AggregateModifiers { get; internal set; } = new WorldGenerationCardModifiers();

        public IEnumerable<ClassCardDefinition> SelectedClassCards
        {
            get
            {
                foreach (ClassCardDefinition card in SelectedPlayerClassCards) yield return card;
                foreach (ClassCardDefinition card in SelectedNpcTankClassCards) yield return card;
                foreach (ClassCardDefinition card in SelectedNpcDpsClassCards) yield return card;
                foreach (ClassCardDefinition card in SelectedNpcFlexClassCards) yield return card;
            }
        }

        public IEnumerable<RaceCardDefinition> SelectedRaceCards => SelectedMainCards.OfType<RaceCardDefinition>();

        public string GetCulturePrefix(int index)
        {
            RaceCardDefinition[] races = SelectedRaceCards.ToArray();
            if (races.Length == 0) return string.Empty;
            return races[Math.Abs(index) % races.Length].CultureAdjective;
        }
    }

    internal static class WorldGenerationProgression
    {
        public static WorldGenerationSlotConfiguration CreateSlotConfiguration(int worldOrdinal)
        {
            int normalizedWorldOrdinal = Math.Max(1, worldOrdinal);
            return new WorldGenerationSlotConfiguration
            {
                MainDrawSlots = 2,
                FactionDrawSlots = normalizedWorldOrdinal >= 5 ? 3 : 2,
                PlayerClassSlots = 5,
                NpcTankClassSlots = 3,
                NpcDpsClassSlots = 6,
                NpcFlexClassSlots = 1
            };
        }
    }
}
