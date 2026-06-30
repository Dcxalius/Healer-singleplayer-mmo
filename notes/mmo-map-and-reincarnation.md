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

## Parentage, Factions, and Birth Rules

Every reincarnated child must have two parents.

Both parents are NPCs, and each parent belongs to a faction. Parentage therefore ties the reincarnated character directly into the world simulation, faction politics, social identity, and local history.

### Parent Rules

1. **Every child has two parents**
   - A reincarnated child always has exactly two parents by default.
   - Both parents are NPCs.
   - Both parents belong to factions.
   - The parents may belong to the same faction or different factions.

2. **Parents can be of either gender**
   - Parent slots are not restricted to one fixed gender pairing.
   - Any gender combination can be valid, depending on race, culture, magic, and world rules.

3. **Faction ownership matters**
   - Each parent has a faction identity.
   - The child's starting reputation, political risk, inherited alliances, or social conflicts can later be derived from the parents' factions.
   - Mixed-faction parentage can create interesting gameplay hooks.

### Humankind Race Carrying Rule

For humankind races, one parent carries the growing child, and only one parent can carry it under normal rules.

```text
HumankindChild
- ParentA: NPC + Faction
- ParentB: NPC + Faction
- CarryingParent: ParentA or ParentB
```

Only one parent can be the carrying parent unless magic gets involved.

Magic can override or modify the normal carrying rule. This allows later systems for magical surrogacy, shared carrying, artificial wombs, spiritual reincarnation, faction rituals, divine intervention, or race-specific exceptions.

### Race-Specific Child Movement Rules

Different races can move or protect a growing child in different ways.

Humankind-style pregnancy should be only one model, not the universal model.

Possible race models:

1. **Carried internally**
   - One parent carries the growing child.
   - This is the default humankind model.

2. **Egg laying**
   - One or both parents create or fertilize an egg.
   - The child develops outside the body.
   - The egg may need guarding, warmth, terrain, magic, or faction structures.

3. **Seeds**
   - Plantlike races may create seeds.
   - The growing child may need soil, water, light, a grove, or a faction-controlled nursery.

4. **External vessel**
   - Some races may grow children in pods, crystals, pools, machines, cocoons, or magical containers.

5. **Split-stage growth**
   - A child may begin carried by one parent, then move to an egg, seed, nest, ritual site, or other growth state.

6. **Magical transfer**
   - Magic may move the growing child between parents, vessels, locations, or planes.
   - This should be treated as an exception layer over the normal race rules.

### Suggested Data

```text
ChildId
ParentAId
ParentAFactionId
ParentBId
ParentBFactionId
RaceId
BirthRuleType
CarryingParentId
GrowthContainerId
GrowthLocationId
MagicInvolved
InheritanceSourceCharacterId
```

### Birth Rule Types

```text
InternalCarry
Egg
Seed
ExternalVessel
SplitStage
MagicalTransfer
OtherRaceSpecific
```

### Design Intent

The child system should connect reincarnation to the living world.

The player is not simply choosing a new body from a menu. The new character is born into social context, faction context, racial biology, and possibly magical exception rules.

This makes resets part of the game's world simulation instead of a purely mechanical prestige system.

### Open Parentage Questions

1. Are parents selected automatically, manually, or through faction/world simulation?
2. Can the retired character influence parent selection?
3. Can one or both parents be from hostile factions?
4. Does the child inherit faction standing from both parents?
5. Does the child inherit race from one parent, both parents, or separate race rules?
6. Can magic create parentage that breaks the normal two-parent rule?
7. Can non-humankind races have more than two biological contributors while still assigning two legal/social parents?

### Open Design Questions

1. What does the child inherit from the retired character?
2. Does the retired character remain physically present in the world?
3. Can retired characters become NPCs?
4. Can a player have multiple retired ancestors?
5. Is reincarnation instant, or does it require an in-world event?
6. How much control does the player have over parent faction, race, and location?

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
7. Define parent NPC references and faction references for reincarnated children.
8. Define race-specific `BirthRuleType` values before implementing child creation.

## Later Questions

1. Should map data be stored in save files, generated from world seed, or both?
2. Should cliff visuals be derived from elevation differences automatically?
3. Should players discover exact terrain type only after visiting a chunk?
4. Can maps become inaccurate if world data changes?
5. Can players annotate maps separately from terrain data?
6. Should faction parentage affect starting location, safety, reputation, or inheritance?
