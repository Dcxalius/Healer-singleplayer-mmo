using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Project_1.WorldGeneration
{
    internal static class WorldGenerationDeckBuilder
    {
        public static WorldGenerationCatalog BuildDefaultCatalog()
        {
            WorldGenerationCatalog catalog = new WorldGenerationCatalog();
            AddTerrainCards(catalog);
            AddRaceCards(catalog);
            AddWildcardCards(catalog);
            AddClassCards(catalog);
            AddFactionCards(catalog);
            return catalog;
        }

        public static WorldGenerationRecipe CreateRandomRecipe(WorldGenerationSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            WorldGenerationCatalog catalog = BuildDefaultCatalog();
            Random rng = new Random(unchecked(settings.Seed * 397) ^ 0x51A7);
            WorldGenerationRecipe recipe = new WorldGenerationRecipe
            {
                WorldOrdinal = Math.Max(1, settings.WorldOrdinal),
                Slots = WorldGenerationProgression.CreateSlotConfiguration(settings.WorldOrdinal),
                Catalog = catalog
            };

            recipe.DrawnMainCards.AddRange(DrawUnique(catalog.MainDeck.ToList(), recipe.Slots.MainDrawSlots, rng));
            recipe.SelectedMainCards.AddRange(recipe.DrawnMainCards);

            List<ClassCardDefinition> playerCards = catalog.ClassCards
                .Where(card => card.Pool == WorldGenerationClassPool.Player)
                .ToList();
            recipe.SelectedPlayerClassCards.AddRange(DrawUnique(playerCards, recipe.Slots.PlayerClassSlots, rng));

            List<ClassCardDefinition> npcCards = catalog.ClassCards
                .Where(card => card.Pool == WorldGenerationClassPool.Npc)
                .ToList();
            recipe.SelectedNpcTankClassCards.AddRange(DrawUnique(npcCards.Where(card => card.SupportsRole(WorldGenerationClassRole.Tank)).ToList(), recipe.Slots.NpcTankClassSlots, rng));
            recipe.SelectedNpcDpsClassCards.AddRange(DrawUnique(npcCards.Where(card => card.SupportsRole(WorldGenerationClassRole.Dps)).ToList(), recipe.Slots.NpcDpsClassSlots, rng));
            recipe.SelectedNpcFlexClassCards.AddRange(DrawUnique(npcCards.Where(card => card.SupportsRole(WorldGenerationClassRole.Flex)).ToList(), recipe.Slots.NpcFlexClassSlots, rng));

            HashSet<string> selectedClassIds = recipe.SelectedClassCards
                .Select(card => card.Id)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            List<FactionCardDefinition> eligibleFactionCards = catalog.FactionCards
                .Where(card => card.RequiredClassCardIds.All(selectedClassIds.Contains))
                .ToList();
            recipe.DrawnFactionCards.AddRange(DrawUnique(eligibleFactionCards, recipe.Slots.FactionDrawSlots, rng));

            WorldGenerationCardModifiers aggregate = new WorldGenerationCardModifiers();
            for (int i = 0; i < recipe.SelectedMainCards.Count; i++)
            {
                aggregate += recipe.SelectedMainCards[i].Modifiers;
            }

            recipe.AggregateModifiers = aggregate;
            return recipe;
        }

        static void AddTerrainCards(WorldGenerationCatalog catalog)
        {
            catalog.Add(new WorldGenerationCardDefinition(
                "terrain-rolling-plains",
                "Rolling Plains",
                WorldGenerationCardType.Terrain,
                "Biases continents toward gentler inland biomes.",
                new WorldGenerationCardModifiers
                {
                    MoistureBiasBonus = 0.05f,
                    CliffStrengthBonus = -0.12f,
                    DuneStrengthBonus = -0.08f,
                    HeightScaleMultiplier = 0.95f
                }));
            catalog.Add(new WorldGenerationCardDefinition(
                "terrain-deep-forests",
                "Deep Forests",
                WorldGenerationCardType.Terrain,
                "Increases moisture and woodland density.",
                new WorldGenerationCardModifiers
                {
                    MoistureBiasBonus = 0.18f,
                    HeatBiasBonus = -0.04f
                }));
            catalog.Add(new WorldGenerationCardDefinition(
                "terrain-cliff-marches",
                "Cliff Marches",
                WorldGenerationCardType.Terrain,
                "Raises ruggedness and cliff probability.",
                new WorldGenerationCardModifiers
                {
                    RuggednessBiasBonus = 0.2f,
                    CliffStrengthBonus = 0.22f,
                    HeightScaleMultiplier = 1.12f
                }));
            catalog.Add(new WorldGenerationCardDefinition(
                "terrain-long-beaches",
                "Long Beaches",
                WorldGenerationCardType.Terrain,
                "Widens coasts and softens cliff transitions.",
                new WorldGenerationCardModifiers
                {
                    BeachWidthBonus = 0.28f,
                    CliffStrengthBonus = -0.08f
                }));
            catalog.Add(new WorldGenerationCardDefinition(
                "terrain-dune-sea",
                "Dune Sea",
                WorldGenerationCardType.Terrain,
                "Pushes continents toward hotter, sand-heavy terrain.",
                new WorldGenerationCardModifiers
                {
                    HeatBiasBonus = 0.24f,
                    MoistureBiasBonus = -0.18f,
                    DuneStrengthBonus = 0.35f
                }));
        }

        static void AddRaceCards(WorldGenerationCatalog catalog)
        {
            catalog.Add(new RaceCardDefinition("race-humans", "Humans", "Human", "Encourages trade cities and stable frontier growth."));
            catalog.Add(new RaceCardDefinition("race-elves", "Elves", "Elven", "Biases settlements toward older forests and quieter borders.", new WorldGenerationCardModifiers { MoistureBiasBonus = 0.08f }));
            catalog.Add(new RaceCardDefinition("race-dwarves", "Dwarves", "Dwarven", "Favors sturdier cliffside settlements.", new WorldGenerationCardModifiers { RuggednessBiasBonus = 0.08f, CliffStrengthBonus = 0.05f }));
            catalog.Add(new RaceCardDefinition("race-orcs", "Orcs", "Orcish", "Pushes harsher frontier pressure and drier interiors.", new WorldGenerationCardModifiers { HeatBiasBonus = 0.06f, DuneStrengthBonus = 0.06f }));
            catalog.Add(new RaceCardDefinition("race-beastkin", "Beastkin", "Beastkin", "Leans toward wilder coasts and untamed edges.", new WorldGenerationCardModifiers { BeachWidthBonus = 0.05f, RuggednessBiasBonus = 0.04f }));
        }

        static void AddWildcardCards(WorldGenerationCatalog catalog)
        {
            AddWildcard(catalog, "wildcard-ancient-roads", "Ancient Roads", "Stabilizes connected landmasses.", new WorldGenerationCardModifiers { BeachWidthBonus = 0.04f });
            AddWildcard(catalog, "wildcard-broken-frontier", "Broken Frontier", "Makes borders harsher and rougher.", new WorldGenerationCardModifiers { RuggednessBiasBonus = 0.08f, CliffStrengthBonus = 0.08f });
            AddWildcard(catalog, "wildcard-fertile-rivers", "Fertile Rivers", "Softens terrain and raises inland moisture.", new WorldGenerationCardModifiers { MoistureBiasBonus = 0.1f, CliffStrengthBonus = -0.05f });
            AddWildcard(catalog, "wildcard-high-peaks", "High Peaks", "Creates steeper continents.", new WorldGenerationCardModifiers { CliffStrengthBonus = 0.12f, HeightScaleMultiplier = 1.1f });
            AddWildcard(catalog, "wildcard-overgrown-wilds", "Overgrown Wilds", "Adds extra humidity and greener interiors.", new WorldGenerationCardModifiers { MoistureBiasBonus = 0.08f });
            AddWildcard(catalog, "wildcard-quiet-wilds", "Quiet Wilds", "Reduces harshness and keeps softer transitions.", new WorldGenerationCardModifiers { RuggednessBiasBonus = -0.05f, DuneStrengthBonus = -0.03f });
            AddWildcard(catalog, "wildcard-salt-winds", "Salt Winds", "Broadens coastal influence.", new WorldGenerationCardModifiers { BeachWidthBonus = 0.1f });
            AddWildcard(catalog, "wildcard-stormfront", "Stormfront", "Adds both moisture and harsher coastlines.", new WorldGenerationCardModifiers { MoistureBiasBonus = 0.05f, CliffStrengthBonus = 0.04f });
            AddWildcard(catalog, "wildcard-sunblessed", "Sunblessed", "Warms continents and supports dune formation.", new WorldGenerationCardModifiers { HeatBiasBonus = 0.1f, DuneStrengthBonus = 0.08f });
            AddWildcard(catalog, "wildcard-twin-moons", "Twin Moons", "Introduces slightly stranger terrain contrast.", new WorldGenerationCardModifiers { HeatBiasBonus = 0.03f, MoistureBiasBonus = 0.03f, RuggednessBiasBonus = 0.03f });
        }

        static void AddWildcard(WorldGenerationCatalog catalog, string id, string name, string description, WorldGenerationCardModifiers modifiers)
        {
            catalog.Add(new WorldGenerationCardDefinition(id, name, WorldGenerationCardType.Wildcard, description, modifiers));
        }

        static void AddClassCards(WorldGenerationCatalog catalog)
        {
            string root = Path.Combine(Game1.ContentManager.RootDirectory, "Data", "Class");
            AddClassCardsFromFolder(catalog, Path.Combine(root, "Player"), WorldGenerationClassPool.Player);
            AddClassCardsFromFolder(catalog, Path.Combine(root, "Player"), WorldGenerationClassPool.Npc);
            AddClassCardsFromFolder(catalog, Path.Combine(root, "Ally"), WorldGenerationClassPool.Npc);
        }

        static void AddClassCardsFromFolder(WorldGenerationCatalog catalog, string folder, WorldGenerationClassPool pool)
        {
            if (!Directory.Exists(folder)) return;

            string[] files = Directory.GetFiles(folder, "*.class");
            for (int i = 0; i < files.Length; i++)
            {
                string raw = File.ReadAllText(files[i]);
                JObject json = JObject.Parse(raw);
                string name = json.Value<string>("Name");
                if (string.IsNullOrWhiteSpace(name)) continue;

                string id = $"{pool.ToString().ToLowerInvariant()}-class-{name.ToLowerInvariant()}";
                List<WorldGenerationClassRole> roles = GetRolesForClass(name, pool);
                string description = pool == WorldGenerationClassPool.Player
                    ? $"Unlocks {name} as a player-facing world class card."
                    : $"Adds {name} to the NPC and faction class pool.";

                catalog.Add(new ClassCardDefinition(id, name, pool, name, roles, description));
            }
        }

        static List<WorldGenerationClassRole> GetRolesForClass(string className, WorldGenerationClassPool pool)
        {
            List<WorldGenerationClassRole> roles = new List<WorldGenerationClassRole>();
            switch (className?.Trim().ToUpperInvariant())
            {
                case "DRUID":
                    roles.Add(WorldGenerationClassRole.Tank);
                    roles.Add(WorldGenerationClassRole.Flex);
                    if (pool == WorldGenerationClassPool.Player)
                    {
                        roles.Add(WorldGenerationClassRole.Dps);
                    }
                    break;
                case "PRIEST":
                    roles.Add(WorldGenerationClassRole.Flex);
                    if (pool == WorldGenerationClassPool.Player)
                    {
                        roles.Add(WorldGenerationClassRole.Dps);
                    }
                    break;
                case "ROGUE":
                    roles.Add(WorldGenerationClassRole.Dps);
                    break;
                default:
                    roles.Add(WorldGenerationClassRole.Flex);
                    break;
            }

            return roles;
        }

        static void AddFactionCards(WorldGenerationCatalog catalog)
        {
            ClassCardDefinition[] npcCards = catalog.ClassCards
                .Where(card => card.Pool == WorldGenerationClassPool.Npc)
                .ToArray();
            if (npcCards.Length == 0) return;

            int requiredClassCount = Math.Max(1, (int)Math.Ceiling(npcCards.Length / 5f));
            string[] factionNames =
            {
                "Bone Banner",
                "Cinder Pact",
                "Thorn Covenant",
                "Salt Reavers",
                "Gloam Wardens"
            };

            for (int i = 0; i < factionNames.Length; i++)
            {
                string[] requiredClassIds = npcCards
                    .Skip((i * requiredClassCount) % npcCards.Length)
                    .Take(requiredClassCount)
                    .Select(card => card.Id)
                    .ToArray();
                if (requiredClassIds.Length < requiredClassCount)
                {
                    requiredClassIds = requiredClassIds
                        .Concat(npcCards.Take(requiredClassCount - requiredClassIds.Length).Select(card => card.Id))
                        .ToArray();
                }

                catalog.Add(new FactionCardDefinition(
                    $"faction-{factionNames[i].ToLowerInvariant().Replace(' ', '-')}",
                    factionNames[i],
                    "A hostile faction deck entry that resolves after class selection.",
                    requiredClassIds,
                    factionNames[i]));
            }
        }

        static List<T> DrawUnique<T>(IReadOnlyList<T> source, int count, Random rng)
        {
            if (source == null || source.Count == 0 || count <= 0)
            {
                return new List<T>();
            }

            List<T> bag = source.ToList();
            for (int i = bag.Count - 1; i > 0; i--)
            {
                int swapIndex = rng.Next(i + 1);
                (bag[i], bag[swapIndex]) = (bag[swapIndex], bag[i]);
            }

            return bag.Take(Math.Min(count, bag.Count)).ToList();
        }
    }
}
