using Microsoft.Xna.Framework;
using Project_1.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project_1.WorldGeneration
{
    internal static class NodeWorldGenerator
    {
        static readonly Point[] cardinalDirections = new Point[]
        {
            new Point(0, -1),
            new Point(1, 0),
            new Point(0, 1),
            new Point(-1, 0)
        };

        public static WorldGraph Generate(WorldGenerationSettings settings)
        {
            settings ??= new WorldGenerationSettings();
            settings.Validate();

            WorldGenerationRecipe recipe = settings.Recipe ?? WorldGenerationDeckBuilder.CreateRandomRecipe(settings);
            WorldGraph world = new WorldGraph(settings, recipe);
            List<Point> graphPositions = GenerateGraphPositions(settings.TargetContinentCount, settings.Seed);
            for (int i = 0; i < graphPositions.Count; i++)
            {
                world.AddContinent(new ContinentNode
                {
                    Id = i,
                    IsStarter = i == 0,
                    GraphPosition = graphPositions[i]
                });
            }

            world.StartingContinentId = 0;

            CreateOceanEdges(world);
            AssignContinentProfiles(world);
            AssignContinentLevelBands(world);
            AssignContinentSizes(world);
            GenerateRegions(world);

            return world;
        }

        internal static LevelBand SampleBlockLevelBand(WorldGraph world, ContinentNode continent, Point localBlock)
        {
            if (world == null) throw new ArgumentNullException(nameof(world));
            if (continent == null) throw new ArgumentNullException(nameof(continent));

            WorldGenerationSettings settings = world.Settings;
            Point totalBlocks = continent.GetTotalBlockDimensions(settings);
            int clampedX = Math.Clamp(localBlock.X, 0, Math.Max(0, totalBlocks.X - 1));
            int clampedY = Math.Clamp(localBlock.Y, 0, Math.Max(0, totalBlocks.Y - 1));

            (float effectiveMin, float effectiveMax) = GetEffectiveContinentBand(world, continent, clampedX, clampedY, totalBlocks);
            int roundedMin = Math.Clamp((int)MathF.Round(effectiveMin), settings.MinWorldLevel, settings.MaxWorldLevel);
            int roundedMax = Math.Clamp((int)MathF.Round(effectiveMax), roundedMin + settings.ContinentMinLevelSpan, settings.MaxWorldLevel);
            if (roundedMax - roundedMin < settings.ContinentMinLevelSpan)
            {
                roundedMax = Math.Min(settings.MaxWorldLevel, roundedMin + settings.ContinentMinLevelSpan);
            }

            ContinentGenerationProfile profile = continent.Profile;
            float rangeNoise = PerlinNoiseGenerator.Fractal01(
                clampedX,
                clampedY,
                seed: profile.SeedOffset + 1103,
                scale: profile.LevelNoiseScaleMin,
                octaves: profile.LevelNoiseOctaves,
                persistence: 0.5f,
                lacunarity: 2f);
            float maxNoise = PerlinNoiseGenerator.Fractal01(
                clampedX,
                clampedY,
                seed: profile.SeedOffset + 2213,
                scale: profile.LevelNoiseScaleMax,
                octaves: profile.LevelNoiseOctaves,
                persistence: 0.5f,
                lacunarity: 2f);

            int localSpan = (int)MathF.Round(MathHelper.Lerp(settings.ContinentMinLevelSpan, 0f, rangeNoise));
            int localMax = (int)MathF.Round(MathHelper.Lerp(roundedMin + settings.ContinentMinLevelSpan, roundedMax, maxNoise));
            int localMin = Math.Max(roundedMin, localMax - localSpan);
            if (localMin > localMax) localMin = localMax;

            return new LevelBand(localMin, localMax);
        }

        internal static float SampleBlockHeight01(WorldGraph world, ContinentNode continent, Point localBlock)
        {
            if (world == null) throw new ArgumentNullException(nameof(world));
            if (continent == null) throw new ArgumentNullException(nameof(continent));

            WorldGenerationSettings settings = world.Settings;
            Point totalBlocks = continent.GetTotalBlockDimensions(settings);
            int clampedX = Math.Clamp(localBlock.X, 0, Math.Max(0, totalBlocks.X - 1));
            int clampedY = Math.Clamp(localBlock.Y, 0, Math.Max(0, totalBlocks.Y - 1));

            float scaleModifier = MathHelper.Lerp(1.2f, 0.75f, continent.Profile?.CliffStrength ?? 0f);
            float recipeHeightScale = world.Recipe?.AggregateModifiers?.HeightScaleMultiplier ?? 1f;
            float baseHeight = PerlinNoiseGenerator.Fractal01(
                clampedX,
                clampedY,
                seed: settings.HeightNoiseSeedOffset + (continent.Profile?.SeedOffset ?? continent.Id),
                scale: settings.HeightNoiseScale * scaleModifier * recipeHeightScale,
                octaves: settings.HeightNoiseOctaves,
                persistence: settings.HeightNoisePersistence,
                lacunarity: settings.HeightNoiseLacunarity);

            float borderMultiplier = GetHeightBorderMultiplier(world, continent, clampedX, clampedY, totalBlocks);
            return MathHelper.Clamp(baseHeight * borderMultiplier, 0f, 1f);
        }

        static List<Point> GenerateGraphPositions(int targetCount, int seed)
        {
            Random rng = new Random(seed);
            List<Point> positions = new List<Point> { Point.Zero };
            HashSet<Point> occupied = new HashSet<Point> { Point.Zero };

            while (positions.Count < targetCount)
            {
                List<Point> candidates = new List<Point>();
                for (int i = 0; i < positions.Count; i++)
                {
                    Point current = positions[i];
                    for (int d = 0; d < cardinalDirections.Length; d++)
                    {
                        Point candidate = current + cardinalDirections[d];
                        if (occupied.Contains(candidate) || candidates.Contains(candidate)) continue;
                        candidates.Add(candidate);
                    }
                }

                if (candidates.Count == 0) break;
                Point chosen = candidates[rng.Next(candidates.Count)];
                positions.Add(chosen);
                occupied.Add(chosen);
            }

            return positions;
        }

        static void CreateOceanEdges(WorldGraph world)
        {
            Random rng = new Random(world.Settings.Seed ^ 0x2A533);
            int oceanId = 0;
            for (int i = 0; i < world.Continents.Count; i++)
            {
                for (int j = i + 1; j < world.Continents.Count; j++)
                {
                    Point delta = world.Continents[j].GraphPosition - world.Continents[i].GraphPosition;
                    if (Math.Abs(delta.X) + Math.Abs(delta.Y) != 1) continue;

                    ContinentSide sideA = GetSideFromDelta(delta);
                    ContinentSide sideB = Opposite(sideA);
                    OceanEdge ocean = new OceanEdge
                    {
                        Id = oceanId++,
                        ContinentAId = world.Continents[i].Id,
                        ContinentBId = world.Continents[j].Id,
                        LengthInRegions = rng.Next(world.Settings.MinOceanLengthInRegions, world.Settings.MaxOceanLengthInRegions + 1),
                        SideA = sideA,
                        SideB = sideB
                    };
                    world.AddOcean(ocean);
                    world.Continents[i].AddOcean(sideA, ocean.Id);
                    world.Continents[j].AddOcean(sideB, ocean.Id);
                }
            }
        }

        static void AssignContinentProfiles(WorldGraph world)
        {
            for (int i = 0; i < world.Continents.Count; i++)
            {
                ContinentNode continent = world.Continents[i];
                int x = continent.GraphPosition.X;
                int y = continent.GraphPosition.Y;

                float moisture = PerlinNoiseGenerator.Fractal01(x, y, world.Settings.Seed + 101, 6f, 3, 0.5f, 2f);
                float heat = PerlinNoiseGenerator.Fractal01(x, y, world.Settings.Seed + 211, 5f, 3, 0.5f, 2f);
                float ruggedness = PerlinNoiseGenerator.Fractal01(x, y, world.Settings.Seed + 307, 4f, 4, 0.55f, 2f);
                WorldGenerationCardModifiers modifiers = world.Recipe?.AggregateModifiers ?? new WorldGenerationCardModifiers();

                moisture = MathHelper.Clamp(moisture + modifiers.MoistureBiasBonus, 0f, 1f);
                heat = MathHelper.Clamp(heat + modifiers.HeatBiasBonus, 0f, 1f);
                ruggedness = MathHelper.Clamp(ruggedness + modifiers.RuggednessBiasBonus, 0f, 1f);

                float duneStrength = MathHelper.Clamp((heat * 0.65f) + ((1f - moisture) * 0.45f) - (ruggedness * 0.2f), 0f, 1f);
                float cliffStrength = MathHelper.Clamp((ruggedness * 0.7f) + ((1f - duneStrength) * 0.2f), 0f, 1f);
                float beachWidthBias = MathHelper.Clamp(((1f - ruggedness) * 0.55f) + (moisture * 0.2f), 0f, 1f);
                duneStrength = MathHelper.Clamp(duneStrength + modifiers.DuneStrengthBonus, 0f, 1f);
                cliffStrength = MathHelper.Clamp(cliffStrength + modifiers.CliffStrengthBonus, 0f, 1f);
                beachWidthBias = MathHelper.Clamp(beachWidthBias + modifiers.BeachWidthBonus, 0f, 1f);

                string tag = "Plains";
                if (duneStrength >= 0.72f) tag = "Dunes";
                else if (cliffStrength >= 0.68f) tag = "Cliffs";
                else if (moisture >= 0.62f) tag = "Forest";

                continent.Profile = new ContinentGenerationProfile
                {
                    SeedOffset = HashCode.Combine(world.Settings.Seed, continent.Id, x, y),
                    Tag = tag,
                    MoistureBias = moisture,
                    HeatBias = heat,
                    RuggednessBias = ruggedness,
                    DuneStrength = duneStrength,
                    CliffStrength = cliffStrength,
                    BeachWidthBias = beachWidthBias,
                    RegionMoistureScale = MathHelper.Lerp(5f, 10f, 1f - moisture),
                    RegionHeatScale = MathHelper.Lerp(5f, 11f, heat),
                    RegionRuggednessScale = MathHelper.Lerp(4f, 9f, ruggedness),
                    LevelNoiseScaleMin = MathHelper.Lerp(120f, 320f, MathHelper.Clamp(1f - duneStrength, 0f, 1f)),
                    LevelNoiseScaleMax = MathHelper.Lerp(140f, 360f, MathHelper.Clamp(1f - cliffStrength, 0f, 1f)),
                    RegionNoiseOctaves = 3,
                    LevelNoiseOctaves = 4
                };
            }
        }

        static void AssignContinentLevelBands(WorldGraph world)
        {
            WorldGenerationSettings settings = world.Settings;
            Dictionary<int, List<int>> adjacency = BuildAdjacency(world);
            Queue<int> queue = new Queue<int>();
            HashSet<int> visited = new HashSet<int>();

            ContinentNode starter = world.GetContinent(world.StartingContinentId);
            starter.MinLevel = 1;
            starter.MaxLevel = 10;
            queue.Enqueue(starter.Id);
            visited.Add(starter.Id);

            Random rng = new Random(settings.Seed ^ 0x71B9);

            while (queue.Count > 0)
            {
                int currentId = queue.Dequeue();
                ContinentNode current = world.GetContinent(currentId);
                List<int> neighbours = adjacency[currentId];
                for (int i = 0; i < neighbours.Count; i++)
                {
                    int neighbourId = neighbours[i];
                    if (visited.Contains(neighbourId)) continue;
                    ContinentNode neighbour = world.GetContinent(neighbourId);
                    LevelBand band = CreateAdjacentBand(settings, current, neighbour, rng);
                    neighbour.MinLevel = band.MinLevel;
                    neighbour.MaxLevel = band.MaxLevel;
                    visited.Add(neighbourId);
                    queue.Enqueue(neighbourId);
                }
            }

            for (int pass = 0; pass < 2; pass++)
            {
                for (int i = 0; i < world.Continents.Count; i++)
                {
                    ContinentNode continent = world.Continents[i];
                    if (continent.IsStarter) continue;

                    List<int> neighbours = adjacency[continent.Id];
                    if (neighbours.Count == 0) continue;

                    float minAccumulator = continent.MinLevel * 0.65f;
                    float maxAccumulator = continent.MaxLevel * 0.65f;
                    float weight = 0.65f;
                    for (int j = 0; j < neighbours.Count; j++)
                    {
                        ContinentNode neighbour = world.GetContinent(neighbours[j]);
                        minAccumulator += neighbour.MinLevel * 0.175f;
                        maxAccumulator += neighbour.MaxLevel * 0.175f;
                        weight += 0.175f;
                    }

                    LevelBand smoothed = NormalizeContinentBand(settings, minAccumulator / weight, maxAccumulator / weight);
                    continent.MinLevel = smoothed.MinLevel;
                    continent.MaxLevel = smoothed.MaxLevel;
                }
            }
        }

        static Dictionary<int, List<int>> BuildAdjacency(WorldGraph world)
        {
            Dictionary<int, List<int>> adjacency = new Dictionary<int, List<int>>();
            for (int i = 0; i < world.Continents.Count; i++)
            {
                adjacency[world.Continents[i].Id] = new List<int>();
            }

            for (int i = 0; i < world.Oceans.Count; i++)
            {
                OceanEdge ocean = world.Oceans[i];
                adjacency[ocean.ContinentAId].Add(ocean.ContinentBId);
                adjacency[ocean.ContinentBId].Add(ocean.ContinentAId);
            }

            return adjacency;
        }

        static LevelBand CreateAdjacentBand(WorldGenerationSettings settings, ContinentNode parent, ContinentNode child, Random rng)
        {
            int span = rng.Next(settings.ContinentMinLevelSpan, settings.ContinentMaxLevelSpan + 1);
            int overlap = rng.Next(2, 6);
            int shift = rng.Next(-1, 3);
            int min = Math.Max(settings.MinWorldLevel, parent.MaxLevel - overlap + shift);

            if (child.Profile != null)
            {
                if (child.Profile.DuneStrength >= 0.7f) min = Math.Max(settings.MinWorldLevel, min - 1);
                if (child.Profile.CliffStrength >= 0.7f) min = Math.Min(settings.MaxWorldLevel - settings.ContinentMinLevelSpan, min + 1);
            }

            int max = min + span;
            return NormalizeContinentBand(settings, min, max);
        }

        static LevelBand NormalizeContinentBand(WorldGenerationSettings settings, float minValue, float maxValue)
        {
            int min = Math.Clamp((int)MathF.Round(minValue), settings.MinWorldLevel, settings.MaxWorldLevel);
            int max = Math.Clamp((int)MathF.Round(maxValue), settings.MinWorldLevel, settings.MaxWorldLevel);
            if (max < min + settings.ContinentMinLevelSpan)
            {
                max = min + settings.ContinentMinLevelSpan;
            }
            if (max - min > settings.ContinentMaxLevelSpan)
            {
                max = min + settings.ContinentMaxLevelSpan;
            }
            if (max > settings.MaxWorldLevel)
            {
                int span = Math.Min(settings.ContinentMaxLevelSpan, max - min);
                max = settings.MaxWorldLevel;
                min = Math.Max(settings.MinWorldLevel, max - Math.Max(settings.ContinentMinLevelSpan, span));
            }

            if (max - min < settings.ContinentMinLevelSpan)
            {
                min = Math.Max(settings.MinWorldLevel, max - settings.ContinentMinLevelSpan);
            }

            return new LevelBand(min, max);
        }

        static void AssignContinentSizes(WorldGraph world)
        {
            for (int i = 0; i < world.Continents.Count; i++)
            {
                ContinentNode continent = world.Continents[i];
                float span01 = MathHelper.Clamp(
                    (continent.LevelSpan - world.Settings.ContinentMinLevelSpan) /
                    (float)(world.Settings.ContinentMaxLevelSpan - world.Settings.ContinentMinLevelSpan),
                    0f,
                    1f);
                float widthNoise = PerlinNoiseGenerator.Fractal01(continent.Id, continent.GraphPosition.X, world.Settings.Seed + 4011, 4f, 2, 0.5f, 2f);
                float heightNoise = PerlinNoiseGenerator.Fractal01(continent.Id, continent.GraphPosition.Y, world.Settings.Seed + 4331, 4f, 2, 0.5f, 2f);

                int interiorWidth = Math.Clamp((int)MathF.Round(MathHelper.Lerp(3f, 7f, span01) + MathHelper.Lerp(-1f, 1f, widthNoise)), 3, 8);
                int interiorHeight = Math.Clamp((int)MathF.Round(MathHelper.Lerp(3f, 7f, span01) + MathHelper.Lerp(-1f, 1f, heightNoise)), 3, 8);

                continent.RegionWidth = interiorWidth + 2;
                continent.RegionHeight = interiorHeight + 2;
            }
        }

        static void GenerateRegions(WorldGraph world)
        {
            int nextFactionId = 1;
            for (int i = 0; i < world.Continents.Count; i++)
            {
                ContinentNode continent = world.Continents[i];
                continent.Regions = new RegionData[continent.RegionWidth, continent.RegionHeight];

                for (int x = 0; x < continent.RegionWidth; x++)
                {
                    for (int y = 0; y < continent.RegionHeight; y++)
                    {
                        RegionData region = new RegionData(new Point(x, y), continent.Id);
                        bool isOcean = x == 0 || y == 0 || x == continent.RegionWidth - 1 || y == continent.RegionHeight - 1;
                        region.IsOceanRegion = isOcean;

                        ContinentSide? coastSide = GetPreferredCoastSide(continent, x, y);
                        if (isOcean && coastSide.HasValue)
                        {
                            region.CoastSide = coastSide;
                            region.ShallowsTowards = Opposite(coastSide.Value);
                            if (continent.TryGetOceanId(coastSide.Value, out int oceanId))
                            {
                                region.OceanId = oceanId;
                            }
                        }

                        region.Biome = DetermineRegionBiome(world, continent, x, y, region.IsOceanRegion);
                        Point sampleBlock = GetRegionCenterBlock(world.Settings, x, y);
                        region.RepresentativeLevelBand = SampleBlockLevelBand(world, continent, sampleBlock);
                        region.RepresentativeHeight01 = SampleBlockHeight01(world, continent, sampleBlock);
                        continent.Regions[x, y] = region;
                    }
                }

                nextFactionId = AssignRegionFactions(world, continent, nextFactionId);
            }
        }

        static int AssignRegionFactions(WorldGraph world, ContinentNode continent, int nextFactionId)
        {
            List<RegionData> cityCandidates = new List<RegionData>();
            List<RegionData> enemyCandidates = new List<RegionData>();

            for (int x = 0; x < continent.RegionWidth; x++)
            {
                for (int y = 0; y < continent.RegionHeight; y++)
                {
                    RegionData region = continent.Regions[x, y];
                    if (region == null || region.IsOceanRegion) continue;

                    if (IsCityCandidate(continent, region.RepresentativeLevelBand))
                    {
                        cityCandidates.Add(region);
                        continue;
                    }

                    if (IsEnemyFactionCandidate(continent, region.RepresentativeLevelBand))
                    {
                        enemyCandidates.Add(region);
                    }
                }
            }

            cityCandidates.Sort((a, b) =>
            {
                int minCompare = a.RepresentativeLevelBand.MinLevel.CompareTo(b.RepresentativeLevelBand.MinLevel);
                if (minCompare != 0) return minCompare;
                return a.RepresentativeLevelBand.MaxLevel.CompareTo(b.RepresentativeLevelBand.MaxLevel);
            });

            for (int i = 0; i < cityCandidates.Count; i++)
            {
                RegionData region = cityCandidates[i];
                bool isCapital = i == 0;
                region.IsCapital = isCapital;
                region.SiteKind = isCapital ? RegionSiteKind.CapitalCity : RegionSiteKind.City;
                string culturePrefix = world.Recipe?.GetCulturePrefix(i) ?? string.Empty;
                string locationName = isCapital
                    ? $"Capital {continent.Id}"
                    : $"City {continent.Id}-{region.Coordinate.X}-{region.Coordinate.Y}";
                string factionName = string.IsNullOrWhiteSpace(culturePrefix)
                    ? locationName
                    : $"{culturePrefix} {locationName}";
                region.Faction = new RegionFactionData(nextFactionId++, FactionKind.CityState, factionName);
                region.Spawners.OwningFactionId = region.Faction.FactionId;
            }

            FactionCardDefinition[] factionCards = world.Recipe?.DrawnFactionCards?.ToArray() ?? Array.Empty<FactionCardDefinition>();
            for (int i = 0; i < enemyCandidates.Count; i++)
            {
                RegionData region = enemyCandidates[i];
                if (region.Faction != null) continue;
                region.SiteKind = RegionSiteKind.EnemyFactionStronghold;
                FactionCardDefinition factionCard = factionCards.Length > 0 ? factionCards[i % factionCards.Length] : null;
                IReadOnlyList<string> mobClassNames = ResolveFactionMobClassNames(world, factionCard);
                string factionName = factionCard?.MobFactionName ?? $"Enemy {continent.Id}-{region.Coordinate.X}-{region.Coordinate.Y}";
                region.Faction = new RegionFactionData(nextFactionId++, FactionKind.Enemy, factionName, factionCard, mobClassNames);
                region.Spawners.OwningFactionId = region.Faction.FactionId;
            }

            return nextFactionId;
        }

        static IReadOnlyList<string> ResolveFactionMobClassNames(WorldGraph world, FactionCardDefinition factionCard)
        {
            if (world?.Recipe == null || factionCard == null || factionCard.RequiredClassCardIds.Count == 0)
            {
                return Array.Empty<string>();
            }

            List<string> mobClassNames = new List<string>();
            foreach (string classCardId in factionCard.RequiredClassCardIds)
            {
                ClassCardDefinition selectedClass = world.Recipe.SelectedClassCards.FirstOrDefault(card => string.Equals(card.Id, classCardId, StringComparison.OrdinalIgnoreCase));
                if (selectedClass != null)
                {
                    mobClassNames.Add(selectedClass.MobClassName);
                }
            }

            return mobClassNames.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        }

        static bool IsCityCandidate(ContinentNode continent, LevelBand band)
        {
            return band.MinLevel <= continent.MinLevel + 1 && band.MaxLevel <= continent.MinLevel + 5;
        }

        static bool IsEnemyFactionCandidate(ContinentNode continent, LevelBand band)
        {
            return band.MinLevel >= continent.MaxLevel - 1 && band.MaxLevel > band.MinLevel;
        }

        static RegionBiome DetermineRegionBiome(WorldGraph world, ContinentNode continent, int regionX, int regionY, bool isOcean)
        {
            ContinentGenerationProfile profile = continent.Profile;
            float moisture = PerlinNoiseGenerator.Fractal01(regionX, regionY, profile.SeedOffset + 501, profile.RegionMoistureScale, profile.RegionNoiseOctaves, 0.5f, 2f);
            float heat = PerlinNoiseGenerator.Fractal01(regionX, regionY, profile.SeedOffset + 607, profile.RegionHeatScale, profile.RegionNoiseOctaves, 0.5f, 2f);
            float ruggedness = PerlinNoiseGenerator.Fractal01(regionX, regionY, profile.SeedOffset + 709, profile.RegionRuggednessScale, profile.RegionNoiseOctaves, 0.5f, 2f);
            float coastalInfluence = ComputeCoastalInfluence(regionX, regionY, continent.RegionWidth, continent.RegionHeight);

            if (isOcean)
            {
                return ruggedness + profile.CliffStrength * 0.45f >= 0.85f ? RegionBiome.Cliffs : RegionBiome.Beach;
            }

            if (coastalInfluence >= 0.7f)
            {
                return ruggedness + profile.CliffStrength * 0.35f >= 0.8f ? RegionBiome.Cliffs : RegionBiome.Beach;
            }

            if (moisture + profile.MoistureBias * 0.3f >= 0.8f && heat <= 0.85f)
            {
                return RegionBiome.Forest;
            }

            if (ruggedness + profile.RuggednessBias * 0.4f >= 0.82f)
            {
                return RegionBiome.Cliffs;
            }

            return RegionBiome.Plains;
        }

        static float ComputeCoastalInfluence(int x, int y, int width, int height)
        {
            int distanceToEdge = Math.Min(Math.Min(x, width - 1 - x), Math.Min(y, height - 1 - y));
            int maxDistance = Math.Max(1, Math.Min(width, height) / 2);
            return MathHelper.Clamp(1f - (distanceToEdge / (float)maxDistance), 0f, 1f);
        }

        static Point GetRegionCenterBlock(WorldGenerationSettings settings, int regionX, int regionY)
        {
            return new Point(
                regionX * settings.RegionBlockWidth + settings.RegionBlockWidth / 2,
                regionY * settings.RegionBlockHeight + settings.RegionBlockHeight / 2);
        }

        static (float MinLevel, float MaxLevel) GetEffectiveContinentBand(WorldGraph world, ContinentNode continent, int blockX, int blockY, Point totalBlocks)
        {
            float minLevel = continent.MinLevel;
            float maxLevel = continent.MaxLevel;
            WorldGenerationSettings settings = world.Settings;

            for (int i = 0; i < continent.OceanIds.Count; i++)
            {
                OceanEdge ocean = world.GetOcean(continent.OceanIds[i]);
                if (ocean == null) continue;

                ContinentSide side = ocean.ContinentAId == continent.Id ? ocean.SideA : ocean.SideB;
                int neighbourId = ocean.ContinentAId == continent.Id ? ocean.ContinentBId : ocean.ContinentAId;
                ContinentNode neighbour = world.GetContinent(neighbourId);
                if (neighbour == null) continue;

                float distance01 = GetDistanceToSide01(blockX, blockY, totalBlocks, side);
                float distanceWeight = MathHelper.Clamp(1f - distance01 / settings.OceanOverlayDistanceFactor, 0f, 1f);
                float oceanLengthScale = MathHelper.Clamp(
                    (ocean.LengthInRegions - settings.MinOceanLengthInRegions) /
                    (float)Math.Max(1, settings.MaxOceanLengthInRegions - settings.MinOceanLengthInRegions),
                    0f,
                    1f);
                float weight = distanceWeight * MathHelper.Lerp(0.6f, 1f, oceanLengthScale) * settings.OceanOverlayStrength;
                if (weight <= 0f) continue;

                minLevel = MathHelper.Lerp(minLevel, neighbour.MinLevel, weight);
                maxLevel = MathHelper.Lerp(maxLevel, neighbour.MaxLevel, weight);
            }

            if (maxLevel < minLevel + settings.ContinentMinLevelSpan)
            {
                maxLevel = minLevel + settings.ContinentMinLevelSpan;
            }

            return (minLevel, Math.Min(settings.MaxWorldLevel, maxLevel));
        }

        static float GetHeightBorderMultiplier(WorldGraph world, ContinentNode continent, int blockX, int blockY, Point totalBlocks)
        {
            if (world == null || continent == null) return 0f;

            WorldGenerationSettings settings = world.Settings;
            Point regionCoord = new Point(
                blockX / settings.RegionBlockWidth,
                blockY / settings.RegionBlockHeight);
            if (regionCoord.X < 0 || regionCoord.Y < 0 || regionCoord.X >= continent.RegionWidth || regionCoord.Y >= continent.RegionHeight)
            {
                return 0f;
            }

            RegionData region = continent.Regions?[regionCoord.X, regionCoord.Y];
            if (region == null) return 0f;
            if (!region.IsOceanRegion) return 1f;
            if (region.ShallowsTowards == null) return 0f;

            int localBlockX = PositiveModulo(blockX, settings.RegionBlockWidth);
            int localBlockY = PositiveModulo(blockY, settings.RegionBlockHeight);
            float localX01 = localBlockX / (float)Math.Max(1, settings.RegionBlockWidth - 1);
            float localY01 = localBlockY / (float)Math.Max(1, settings.RegionBlockHeight - 1);

            float multiplier = region.ShallowsTowards.Value switch
            {
                ContinentSide.North => 1f - localY01,
                ContinentSide.East => localX01,
                ContinentSide.South => localY01,
                ContinentSide.West => 1f - localX01,
                _ => 0f
            };

            multiplier = MathHelper.Clamp(multiplier, 0f, 1f);
            if (settings.OceanHeightFalloffPower != 1f)
            {
                multiplier = MathF.Pow(multiplier, settings.OceanHeightFalloffPower);
            }

            return multiplier;
        }

        static float GetDistanceToSide01(int blockX, int blockY, Point totalBlocks, ContinentSide side)
        {
            float maxX = Math.Max(1, totalBlocks.X - 1);
            float maxY = Math.Max(1, totalBlocks.Y - 1);
            return side switch
            {
                ContinentSide.North => blockY / maxY,
                ContinentSide.East => (maxX - blockX) / maxX,
                ContinentSide.South => (maxY - blockY) / maxY,
                ContinentSide.West => blockX / maxX,
                _ => 1f
            };
        }

        static ContinentSide? GetPreferredCoastSide(ContinentNode continent, int x, int y)
        {
            List<ContinentSide> sides = new List<ContinentSide>(2);
            if (y == 0) sides.Add(ContinentSide.North);
            if (x == continent.RegionWidth - 1) sides.Add(ContinentSide.East);
            if (y == continent.RegionHeight - 1) sides.Add(ContinentSide.South);
            if (x == 0) sides.Add(ContinentSide.West);

            for (int i = 0; i < sides.Count; i++)
            {
                if (continent.TryGetOceanId(sides[i], out _)) return sides[i];
            }

            return sides.Count > 0 ? sides[0] : null;
        }

        static ContinentSide GetSideFromDelta(Point delta)
        {
            if (delta.X > 0) return ContinentSide.East;
            if (delta.X < 0) return ContinentSide.West;
            if (delta.Y > 0) return ContinentSide.South;
            return ContinentSide.North;
        }

        static ContinentSide Opposite(ContinentSide side)
        {
            return side switch
            {
                ContinentSide.North => ContinentSide.South,
                ContinentSide.East => ContinentSide.West,
                ContinentSide.South => ContinentSide.North,
                ContinentSide.West => ContinentSide.East,
                _ => side
            };
        }

        static int PositiveModulo(int value, int modulo)
        {
            int result = value % modulo;
            return result < 0 ? result + modulo : result;
        }
    }
}
