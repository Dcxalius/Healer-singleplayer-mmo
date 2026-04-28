using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project_1.WorldGeneration
{
    internal enum ContinentSide
    {
        North,
        East,
        South,
        West
    }

    internal enum RegionBiome
    {
        Plains,
        Forest,
        Cliffs,
        Beach
    }

    internal enum FactionKind
    {
        CityState,
        Enemy
    }

    internal enum RegionSiteKind
    {
        None,
        City,
        CapitalCity,
        EnemyFactionStronghold
    }

    internal readonly struct LevelBand
    {
        public LevelBand(int minLevel, int maxLevel)
        {
            if (maxLevel < minLevel) throw new ArgumentOutOfRangeException(nameof(maxLevel));
            MinLevel = minLevel;
            MaxLevel = maxLevel;
        }

        public int MinLevel { get; }
        public int MaxLevel { get; }
        public int Span => MaxLevel - MinLevel;
    }

    internal sealed class RegionSpawnerCollection
    {
        public int? OwningFactionId { get; internal set; }
        public List<int> SpawnZoneIds { get; } = new List<int>();
    }

    internal sealed class RegionFactionData
    {
        public RegionFactionData(int factionId, FactionKind kind, string name, FactionCardDefinition card = null, IEnumerable<string> mobClassNames = null)
        {
            FactionId = factionId;
            Kind = kind;
            Name = name ?? string.Empty;
            Card = card;
            MobClassNames = mobClassNames?.ToArray() ?? Array.Empty<string>();
        }

        public int FactionId { get; }
        public FactionKind Kind { get; }
        public string Name { get; }
        public FactionCardDefinition Card { get; }
        public IReadOnlyList<string> MobClassNames { get; }
    }

    internal sealed class RegionData
    {
        public RegionData(Point coordinate, int continentId)
        {
            Coordinate = coordinate;
            ContinentId = continentId;
            Spawners = new RegionSpawnerCollection();
        }

        public Point Coordinate { get; }
        public int ContinentId { get; }
        public bool IsOceanRegion { get; internal set; }
        public int? OceanId { get; internal set; }
        public ContinentSide? CoastSide { get; internal set; }
        public ContinentSide? ShallowsTowards { get; internal set; }
        public RegionBiome Biome { get; internal set; }
        public LevelBand RepresentativeLevelBand { get; internal set; }
        public float RepresentativeHeight01 { get; internal set; }
        public RegionSiteKind SiteKind { get; internal set; }
        public bool IsCapital { get; internal set; }
        public RegionFactionData Faction { get; internal set; }
        public RegionSpawnerCollection Spawners { get; }
    }

    internal sealed class ContinentGenerationProfile
    {
        public int SeedOffset { get; init; }
        public string Tag { get; init; } = string.Empty;
        public float MoistureBias { get; init; }
        public float HeatBias { get; init; }
        public float RuggednessBias { get; init; }
        public float DuneStrength { get; init; }
        public float CliffStrength { get; init; }
        public float BeachWidthBias { get; init; }
        public float RegionMoistureScale { get; init; }
        public float RegionHeatScale { get; init; }
        public float RegionRuggednessScale { get; init; }
        public float LevelNoiseScaleMin { get; init; }
        public float LevelNoiseScaleMax { get; init; }
        public int RegionNoiseOctaves { get; init; }
        public int LevelNoiseOctaves { get; init; }
    }

    internal sealed class OceanEdge
    {
        public int Id { get; init; }
        public int ContinentAId { get; init; }
        public int ContinentBId { get; init; }
        public int LengthInRegions { get; init; }
        public ContinentSide SideA { get; init; }
        public ContinentSide SideB { get; init; }
    }

    internal sealed class ContinentNode
    {
        readonly Dictionary<ContinentSide, int> oceansBySide = new Dictionary<ContinentSide, int>();

        public int Id { get; init; }
        public bool IsStarter { get; init; }
        public Point GraphPosition { get; init; }
        public int MinLevel { get; internal set; }
        public int MaxLevel { get; internal set; }
        public int RegionWidth { get; internal set; }
        public int RegionHeight { get; internal set; }
        public ContinentGenerationProfile Profile { get; internal set; }
        public RegionData[,] Regions { get; internal set; }
        public List<int> OceanIds { get; } = new List<int>();
        public IReadOnlyDictionary<ContinentSide, int> OceansBySide => oceansBySide;
        public int LevelSpan => MaxLevel - MinLevel;

        internal void AddOcean(ContinentSide side, int oceanId)
        {
            oceansBySide[side] = oceanId;
            if (!OceanIds.Contains(oceanId))
            {
                OceanIds.Add(oceanId);
            }
        }

        public bool TryGetOceanId(ContinentSide side, out int oceanId)
        {
            return oceansBySide.TryGetValue(side, out oceanId);
        }

        public Point GetTotalBlockDimensions(WorldGenerationSettings settings)
        {
            return new Point(RegionWidth * settings.RegionBlockWidth, RegionHeight * settings.RegionBlockHeight);
        }
    }

    internal sealed class WorldGraph
    {
        readonly Dictionary<int, ContinentNode> continentLookup = new Dictionary<int, ContinentNode>();
        readonly Dictionary<int, OceanEdge> oceanLookup = new Dictionary<int, OceanEdge>();

        public WorldGraph(WorldGenerationSettings settings, WorldGenerationRecipe recipe)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            Recipe = recipe;
        }

        public WorldGenerationSettings Settings { get; }
        public WorldGenerationRecipe Recipe { get; }
        public int StartingContinentId { get; internal set; }
        public List<ContinentNode> Continents { get; } = new List<ContinentNode>();
        public List<OceanEdge> Oceans { get; } = new List<OceanEdge>();

        internal void AddContinent(ContinentNode continent)
        {
            Continents.Add(continent);
            continentLookup[continent.Id] = continent;
        }

        internal void AddOcean(OceanEdge ocean)
        {
            Oceans.Add(ocean);
            oceanLookup[ocean.Id] = ocean;
        }

        public ContinentNode GetContinent(int id)
        {
            return continentLookup.TryGetValue(id, out ContinentNode continent) ? continent : null;
        }

        public OceanEdge GetOcean(int id)
        {
            return oceanLookup.TryGetValue(id, out OceanEdge ocean) ? ocean : null;
        }

        public IEnumerable<ContinentNode> GetNeighbours(ContinentNode continent)
        {
            if (continent == null) yield break;

            for (int i = 0; i < continent.OceanIds.Count; i++)
            {
                OceanEdge ocean = GetOcean(continent.OceanIds[i]);
                if (ocean == null) continue;
                int otherId = ocean.ContinentAId == continent.Id ? ocean.ContinentBId : ocean.ContinentAId;
                ContinentNode neighbour = GetContinent(otherId);
                if (neighbour != null) yield return neighbour;
            }
        }
    }
}
