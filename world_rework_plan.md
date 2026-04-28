# Node World To Block World Plan

## Goals

- Replace the current flat infinite tile field with a hierarchical world model:
  - `World`
  - `Continent`
  - `Region`
  - `Chunk`
  - `Block`
- Keep chunk storage untyped.
- Store biome, faction, city, and ocean identity at the region level.
- Let continents act as graph nodes connected by explicit ocean edges.
- Move gameplay progression from ad-hoc chunk noise into continent and region driven level bands.
- Introduce a world-card recipe layer so future world creation can be driven by deck draws instead of hardcoded presets.

## Current Bridge Strategy

- The live runtime is still 2D and chunk-based.
- The new node-world system is being introduced first as a generation layer.
- Until full continent placement and block storage land, the current runtime uses the starter continent as the active world bridge.
- Existing `Chunk` and `Tile` runtime classes remain in place during the transition.
- Card selection currently lives as generation-time backend data only. UI and persistence are still future work.

## World Card System

### Intent

- World creation should eventually be driven by card draws.
- The first world should auto-resolve its draws from the deck.
- After the level 10 unlock quest, new worlds should let the player choose from drawn cards instead of only auto-applying them.
- Factions are not part of the main manual draw.
- Faction cards are resolved from a separate deck after the player has locked in the final class-driven draw.

### Current Backend Shape

- A `WorldGenerationRecipe` now sits beside `WorldGenerationSettings`.
- The generator creates a recipe automatically when one is not supplied.
- A recipe contains:
  - world ordinal
  - slot configuration
  - drawn and selected main-deck cards
  - selected player class cards
  - selected NPC class cards by role bucket
  - auto-drawn faction cards
  - aggregate terrain modifiers for world generation

### Slot Progression

- Main world-card draw slots currently start at `2`.
- Faction auto-draw slots currently start at `2`.
- On the fifth world and onward, faction auto-draw slots increase to `3`.
- Player-class slots currently target `5`.
- NPC class slots currently target:
  - `3` tank slots
  - `6` DPS slots
  - `1` flex slot
- Slot counts are progression data and should remain easy to expand later.

### Card Families

- `Terrain`
- `Race`
- `Class`
- `Wildcard`
- `Faction`

### Version 0.1 Card Inventory Target

- At least `5` terrain cards
- At least `5` race cards
- At least `5` class cards
- At least `5` faction cards
- At least `10` wildcard cards

### Current Seeded Card Inventory

- Terrain cards: `5`
- Race cards: `5`
- Wildcard cards: `10`
- Faction cards: `5`
- Class cards: `5`
  - player pool cards are created from current player class content
  - NPC pool cards are created from current player and ally class content

### Class Cards

- Class cards are data-backed from the current class files in `Content/Data/Class`.
- Current generated class-card roster:
  - player `Druid`
  - player `Priest`
  - NPC `Druid`
  - NPC `Priest`
  - NPC `Rogue`
- The same class can exist in multiple card pools without being the same card entry.

### Faction Cards

- Faction cards are built after class-card pools are known.
- A faction card requires a number of classes equal to one fifth of the existing NPC class-card pool, rounded up.
- With the current roster, that means each faction card requires `1` NPC class card.
- Enemy factions in the node-world generator now consume these faction cards and carry their derived mob-class names in region faction metadata.

### Current Generator Effects

- Terrain and wildcard cards currently affect continent profile generation by modifying:
  - moisture bias
  - heat bias
  - ruggedness bias
  - dune strength
  - cliff strength
  - beach width
  - height noise scale
- Race cards currently influence city/capital faction naming.
- Faction cards currently influence enemy faction naming and future mob-class ownership.

## Target Hierarchy

### World

- Holds the global seed and generation settings.
- Owns the continent node graph.
- Owns all ocean edge connections.
- Owns worldgen constants like:
  - chunk footprint: `32 x wh x 32`
  - region size in chunks: `32 x 32`
  - target continent count

### Continents

- Each continent is a node in the world graph.
- Each continent has:
  - a base level range
  - generation profile values used as noise inputs
  - a region grid
  - a list of neighbouring oceans
- The starter continent is always the single `1-10` continent.

### Oceans

- Each node connection is an `Ocean`.
- An ocean stores:
  - continent A
  - continent B
  - length
  - connected coast side on both continents
- Ocean edges are used both for topology and for level-range blending near coastlines.

### Regions

- Regions are the biome and macro gameplay unit.
- Regions contain:
  - biome data
  - ocean/coast metadata
  - faction ownership
  - city/capital/enemy site metadata
  - region-owned spawner collections
- Edge regions of continents are ocean regions.
- Ocean regions are non-traversable at the region level for normal land units, even if some tiles remain visually shallow/walkable near shore.

### Chunks

- Chunks are storage, streaming, and render units.
- Chunks are not biome typed.
- Chunk contents are generated from the parent region and continent profile.

### Blocks

- Blocks become the true world cell.
- Blocks should eventually carry:
  - material
  - collision/solid state
  - transparency/vision state
  - elevation/depth
  - optional decoration/surface data

## Level Range Rules

- Starter continent is fixed to `1-10`.
- Other continents have base level spans constrained to:
  - minimum span: `5`
  - maximum span: `15`
- Nearby continents influence each other near connected coasts through ocean overlay blending.
- Continent size scales with base level span.

## Block Resolution Level Sampling

- Each continent produces two block-resolution 2D Perlin fields:
  - range-span field
  - local-max field
- The effective continent min/max at a point first includes nearby ocean-neighbour overlays.
- Then the local fields shape the final block-level band:
  - at `rangeNoise = 0`, local span is `5`
  - at `rangeNoise = 1`, local min equals local max
  - at `maxNoise = 0`, local max is `continentMin + 5`
  - at `maxNoise = 1`, local max is `continentMax`

## Perlin Noise By Layer

### World / Continent Layer

- Low-frequency noise sampled in node space.
- Drives:
  - moisture bias
  - heat bias
  - ruggedness bias
  - dune strength
  - cliff strength
  - beach width bias

### Region Layer

- Medium-frequency noise sampled in region space.
- Drives:
  - biome distribution
  - coast feel
  - forest vs plains balance
  - cliffs vs beaches

### Block Layer

- High-resolution noise sampled in block space.
- Drives:
  - local level range
  - local maximum level
  - terrain details like dunes, cliffs, shallows, and elevation

### Height Field

- Each world has one world-level height Perlin field with options set at creation time:
  - seed offset
  - scale
  - octaves
  - persistence
  - lacunarity
  - ocean falloff power
- Height is sampled at block resolution.
- Land regions currently use the sampled height to bias cliffs and rocky tiles.
- Ocean regions multiply the sampled height by a coast-to-edge falloff:
  - near the coast-facing side of the ocean region, multiplier stays close to `1`
  - at the outer ocean-facing edge, multiplier is exactly `0`
- This lets ocean regions keep shallow coastal approaches while still collapsing toward sea level at the outer boundary.

## Biomes

Initial biome set:

- Plains
- Forest
- Cliffs
- Beach

Longer term:

- Dunes can stay as a continent profile style even if the region biome is still `Plains`.
- Additional biome families can be layered in later without changing chunk typing.

## Factions, Cities, And Region Spawners

- Cities are created when a region resolves to the lowest local min and max values.
- The smallest such city in a continent is the capital.
- Every city has its own faction.
- Enemy factions are created where the local minimum is near the continent maximum and the local maximum is still higher.
- Enemy factions should increasingly be sourced from resolved faction cards instead of only anonymous generated labels.
- Spawner ownership belongs to the region, not the global world field.
- Existing global `SpawnerManager` will eventually become a runtime coordinator over region-owned spawn collections.

## Ocean Region Rules

- Every edge region of a continent is an ocean region.
- Ocean regions store which side is coast-facing and which direction the shallows face.
- Ocean regions can contain some accessible shallow tiles on the continent-facing side.
- Ocean regions remain blocked to players and normal land units at the region traversal level.

## Runtime Migration Phases

### Phase 1: Node World Foundation

- Add world graph, continents, oceans, regions, profiles, and level sampling.
- Generate node world on new-game start.
- Keep existing chunk runtime active.

### Phase 2: Starter Continent Bridge

- Map current chunk positions into the starter continent.
- Generate current chunk tiles from region biome and ocean metadata.
- Generate current chunk average levels from continent/block sampling.
- Use starter capital/city data for initial spawn placement.

### Phase 3: Full Continent Placement

- Replace the temporary starter-only bridge with full graph-to-world placement.
- Support multiple active continents in runtime coordinates.
- Make oceans and coastlines spatially consistent across connected nodes.

### Phase 4: Block Storage

- Replace `Tile` as the gameplay cell with block storage.
- Keep a surface extraction layer for top-down rendering.
- Move collision, LOS, and pathing to block state queries.

### Phase 5: Region Gameplay Ownership

- Move spawner ownership into regions/factions.
- Add city, capital, faction, and enemy stronghold gameplay hooks.
- Use region metadata for NPC, mob, and settlement placement.

## Immediate Next Tasks

- Add recipe persistence so world saves remember which cards were drawn and resolved.
- Build the first world-creation UI around the new recipe instead of auto-resolving everything silently.
- Add debug and inspection helpers so we can see continent, region, biome, faction, and card data during runtime.
- Thread region-owned spawner managers through the new faction card class ownership.
