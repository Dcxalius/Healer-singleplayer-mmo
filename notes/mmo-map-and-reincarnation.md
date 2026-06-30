# MMO Notes: Reincarnation, World Time, and Map System

## Branch Purpose

This branch is for design notes only. It captures early rules for character retirement/reincarnation, world movement time tracking, and a map-generation system that is separate from the minimap.

## Character Reset, Retirement, and Reincarnation

Resetting a character should not delete the character outright. Instead, it should retire the current character and allow the player to reincarnate into a child character.

### Core Rules

1. **Reset means retirement**
   - The previous character becomes retired.
   - Retired characters should remain part of account/world history.
   - Retirement should preserve useful legacy data for future systems.

2. **Reincarnation creates a child**
   - The new playable character starts as a child.
   - This implies a new life stage, not just a stat reset.
   - The reincarnated child may inherit selected legacy values from the retired character.

3. **Retirement should support future history systems**
   - Former characters could appear in family trees, records, towns, graves, legends, mentorship systems, or NPC references.
   - The exact inheritance rules should be defined later.

### Open Design Questions

1. What does the child inherit from the retired character?
2. Does the retired character remain physically present in the world?
3. Can retired characters become NPCs?
4. Can a player have multiple retired ancestors?
5. Is reincarnation instant, or does it require an in-world event?

## World Movement and Time Tracking

Movement in the world should be time tracked. The current conversion rule is:

```text
1 real-time hour of tracked world movement = 1 in-world year
```

This means travel has meaningful aging consequences. The player is not only moving through space, but also through the character lifecycle.

### Implications

1. **Long travel ages the character**
   - Distance and route choice become strategically important.
   - Dangerous shortcuts may become valuable because they save lifespan.

2. **Reincarnation connects naturally to travel**
   - A character who spends too much time traveling may eventually age out of their current life.
   - Retirement and reincarnation can become part of the normal long-term gameplay loop.

3. **Movement systems need time output**
   - Any world movement calculation should produce elapsed movement time.
   - The time system should convert elapsed movement time into age progression.

### Suggested Data

```text
CharacterAgeYears
CharacterLifeStage
MovementTimeTrackedSeconds
WorldYearsPerRealHour = 1
TotalWorldYearsPassed
```

## Separate Map System

The map system should be completely separate from the minimap.

The minimap is for local, immediate navigation. The map system described here is for world structure, terrain identity, chunk data, and generated map visuals.

## Chunk-Based Map Data

Each world chunk should provide map information. At minimum, every chunk should define its terrain type.

### Chunk Map Fields

```text
ChunkId
ChunkPosition
TerrainType
TerrainVariant
ElevationBand
HasCliff
CliffDirections
VisualEdgeData
DiscoveredState
RegionId
BiomeId
```

### Terrain Type

Each chunk should expose a terrain type used by the map renderer.

Examples:

1. Grassland
2. Forest
3. Mountain
4. Swamp
5. Desert
6. Snow
7. Coast
8. Ocean
9. River
10. Road
11. Settlement
12. Ruins

Terrain type should be the base visual identity of the chunk.

## Wonky Terrain and Directional Visual Data

Some terrain cannot be represented only by a terrain type. Cliffs, ledges, sharp elevation changes, coast edges, ravines, and similar features need directional graphics information.

### Cliff Data

A chunk with a cliff should indicate which directions contain cliff edges.

Example using cardinal and diagonal directions:

```text
CliffNorth
CliffNorthEast
CliffEast
CliffSouthEast
CliffSouth
CliffSouthWest
CliffWest
CliffNorthWest
```

Alternative compact form:

```text
CliffDirections = North | East | SouthEast
```

### Purpose

Directional cliff data allows the map renderer to place cliff graphics visually and correctly.

The chunk does not need to store the final image. It stores enough positional and directional information for the map renderer to generate the visual edge.

## Graphics Position Info

Chunks should be able to provide graphics position info for terrain features that need special placement.

This should support generated visuals such as:

1. Cliff edges
2. Coastline edges
3. River bends
4. Roads entering or exiting chunk sides
5. Mountain ridges
6. Forest density patches
7. Settlement borders
8. Region borders

### Suggested Structure

```text
MapVisualFeature
- FeatureType
- LocalPosition
- Direction
- Variant
- Priority
- BlendMode
```

Example:

```text
FeatureType: CliffEdge
LocalPosition: EastSide
Direction: East
Variant: RockyCliff01
Priority: 80
BlendMode: Overlay
```

## Map Renderer Responsibility

The map renderer should consume chunk map data and generate the final map appearance.

The chunk owns data. The renderer owns visuals.

### Renderer Responsibilities

1. Read chunk terrain type.
2. Select base map tile or texture.
3. Read directional terrain features.
4. Generate cliff/coast/river/road edges.
5. Blend neighboring chunks where needed.
6. Respect discovery state.
7. Output a map image or map layer.

## Separation from Minimap

The separate map system should not depend on the minimap.

### World Map

1. Uses chunk-level terrain data.
2. Can show large-scale terrain, regions, travel routes, cliffs, borders, and discovered areas.
3. May be generated offline, cached, or updated when chunks change.

### Minimap

1. Uses immediate local player surroundings.
2. Should focus on nearby navigation.
3. Can use live entity data, nearby interactables, and local geometry.
4. Should not be the source of truth for world map terrain.

## Initial Implementation Direction

1. Define a `MapChunkInfo` data model.
2. Add terrain type to every generated chunk.
3. Add directional cliff data to chunks with sharp elevation changes.
4. Add a generic `MapVisualFeature` list for special generated graphics.
5. Build a map-renderer prototype that renders base terrain first, then overlays directional features.
6. Keep minimap logic separate from this system.

## Later Questions

1. Should map data be stored in save files, generated from world seed, or both?
2. Should cliff visuals be derived from elevation differences automatically?
3. Should players discover exact terrain type only after visiting a chunk?
4. Can maps become inaccurate if world data changes?
5. Can players annotate maps separately from terrain data?
