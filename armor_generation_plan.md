# Armor Generation System Plan

## Purpose

This document outlines a practical plan for adding a system that:

1. generates a 3D armor model from the type of equipment that is created,
2. renders a 64px inventory item sprite from that model, and
3. keeps the model around so it can later generate 2D sprite sheets for units wearing the armor.

The goal is to make armor visuals deterministic, extensible, and compatible with the current architecture of this project.

## Current Project Constraints

The existing codebase is heavily texture-driven.

- Item UI visuals are routed through `ItemData.GfxPath` and `ItemUiSnapshot.GfxPath`.
- Equipment slots in the character window display ordinary item textures.
- Unit world visuals also resolve to texture paths, not live 3D rendered entities.
- `TextureManager` currently scans content folders and loads textures through the MonoGame content pipeline.

This means the new armor system should not start as a live runtime 3D renderer inside the game loop. The cleanest fit is:

- generate armor appearance data and source models outside the frame render path,
- bake textures from those source models,
- feed those baked textures into the existing item and unit rendering systems.

## High-Level Direction

The 3D armor model should be the canonical visual source of truth.

From that source, the system should produce:

- a 64x64 item icon for inventory and equipment UI,
- worn 2D sprite sheets for a compatible unit body,
- optional future outputs such as inspect renders, crafting previews, or marketing renders.

This gives us one visual definition that can support multiple presentation formats.

## Core Principles

### Deterministic Output

The same equipment input should always produce the same appearance unless the generation version changes.

### Constrained Generation

The system should use controlled procedural generation or modular assembly rather than unconstrained text-to-3D generation. This is important for:

- silhouette readability at small sprite sizes,
- rig compatibility,
- stable layering and masking,
- predictable visual quality.

### Cached Products

Rendering icons and worn sprite sheets should be done once and cached. The game should consume cached assets instead of regenerating them in moment-to-moment gameplay.

### Versioned Style

All generated outputs should include a style version so we can refresh visuals later without breaking determinism or save compatibility.

## Proposed System Layers

### 1. ArmorAppearanceSpec

This is the deterministic recipe that describes how a piece of armor should look.

Suggested fields:

- equipment slot
- armor material or gear type
- rarity tier
- silhouette family
- detail family
- trim family
- ornament family
- palette selection
- wear or damage amount
- seed
- generation version

This spec should be stable and serializable.

### 2. ArmorSourceModel

This is the generated or assembled 3D armor source asset. It should exist in a normalized coordinate system and be attached to a known body rig anchor set.

Suggested contents:

- canonical mesh or kit assembly definition
- material assignments
- color regions
- anchor metadata
- slot masking data
- occlusion metadata
- source generation version

### 3. Render Products

These are baked outputs derived from the source model.

At minimum:

- `ItemIcon64`
- `WornSpriteSheet`

Potential later outputs:

- paper doll portrait
- inspect render
- merchant preview render
- loot card render

### 4. Appearance Cache

This layer maps deterministic keys to generated outputs.

The cache should support:

- memory caching for already loaded assets,
- disk caching for generated png and metadata files,
- invalidation when generator version changes.

## Suggested Deterministic Keys

Base appearance key:

- `armor:<slot>:<material>:<seed>:<version>`

Item icon key:

- `icon:<appearance-key>:64`

Worn sprite sheet key:

- `outfit:<unit-rig>:<anim-set>:<appearance-keys>:<version>`

The current `Equipment.Hash` is a good starting point for the appearance seed because it already exists per equipment instance and is carried into UI snapshots.

## Data Ownership

### Gameplay layer

The gameplay item remains authoritative for stats, rarity, slot, and identity.

### Appearance layer

The appearance system becomes authoritative for how the item looks.

### Render layer

The render layer only consumes baked results and cache keys.

This separation keeps visual generation from leaking into combat, inventory logic, or simulation state.

## Integration With Current Codebase

### Item icons

Today, item visuals flow through `GfxPath` in item data and UI snapshots. Generated icons should eventually resolve through the same mechanism so inventory and equipment UI do not need a custom rendering path.

Practical options:

1. build-time or startup bake into content folders,
2. runtime texture registry added beside `TextureManager`,
3. hybrid approach where the generator writes to disk and a runtime registry loads from generated outputs.

Recommended direction:

- start with a runtime-generated texture registry for generated armor outputs,
- keep the current content pipeline for hand-authored textures,
- avoid forcing the MonoGame pipeline to rebuild every time an item is generated.

### Unit worn visuals

Units currently resolve to a texture path for their world render. Worn armor sprite sheets should eventually produce textures that can plug into the same world render flow.

The likely long-term route is:

- derive a unit appearance key from body archetype plus equipped armor appearance keys,
- bake a composed sprite sheet for that outfit,
- expose a normal texture path or generated texture handle to the world rendering system.

## Appearance Generation Strategy

The generator should be slot-aware and rig-aware.

Examples:

- helmets attach to head anchors,
- shoulders attach to shoulder anchors,
- chest pieces can influence torso and upper-arm coverage,
- boots can override foot and lower-leg visual regions,
- cloaks need back-layer behavior.

For each slot, define:

- allowed silhouette families,
- allowed geometry modules,
- optional ornament modules,
- palette rules,
- coverage masks,
- rig anchor points.

This makes the system easier to expand piece by piece.

## Rendering Rules

### Item icon renderer

The item icon renderer should:

- render to 64x64,
- use a fixed camera and lighting rig,
- preserve readability at small sizes,
- apply a pixel cleanup pass if needed,
- ensure silhouettes remain clear even for busy rare or epic items.

### Worn sprite sheet renderer

The worn sprite sheet renderer should:

- render armor on a standard unit rig and pose set,
- use fixed animation viewpoints and framing,
- output sprite sheets aligned to existing world rendering expectations,
- support masking and hiding body regions covered by armor,
- support layer ordering for capes, shoulders, shields, and oversized pieces.

## Recommended Composition Model

Whole-outfit baking is recommended over stacking many independent 2D overlay sprites at runtime.

Reasons:

- fewer clipping artifacts,
- better cohesion between body and armor,
- easier lighting consistency,
- lower complexity during gameplay rendering.

A limited layered 2D fallback may still be useful during development, but the main path should be baked outfit sheets.

## Storage Plan

Suggested generated asset categories:

- source model metadata
- baked icon png
- baked worn sprite sheet png
- manifest file mapping appearance keys to outputs

Suggested generated cache structure:

- `/generated/armor/source/`
- `/generated/armor/icons/`
- `/generated/armor/outfits/`
- `/generated/armor/manifests/`

The exact path can change, but the system should keep generated outputs clearly separated from hand-authored content.

## Pipeline Phases

### Phase 1: Define appearance spec

Deliverables:

- `ArmorAppearanceSpec` structure
- deterministic key generation
- mapping from equipment inputs to appearance spec
- versioning strategy

Notes:

- use existing equipment slot, material, quality, and hash as input,
- keep the first pass intentionally narrow.

### Phase 2: Generate 64px item icons only

Deliverables:

- source armor generation for a few visible slots
- icon renderer
- disk and memory cache
- UI integration path for generated item textures

Suggested initial slots:

- head
- chest
- legs
- feet

This phase gives immediate visible value while keeping complexity manageable.

### Phase 3: Support source model persistence

Deliverables:

- saved source model metadata
- stable appearance manifests
- reload path from cache without full regeneration

At the end of this phase, the system should be able to reuse source models for future render products.

### Phase 4: Add worn sprite sheet baking for one unit rig

Deliverables:

- one supported body archetype
- one animation or direction set
- baked composed outfit output
- integration path for unit visuals

This should prove that the same armor source model can drive both inventory icons and worn visuals.

### Phase 5: Expand slot coverage and archetypes

Deliverables:

- shoulders
- hands
- belt
- cloak or back
- additional unit rigs
- additional animation sets

This is where the system becomes broadly useful across the game.

### Phase 6: Background generation and polish

Deliverables:

- asynchronous generation queue
- missing-asset placeholder behavior
- regeneration controls
- debugging tools
- visual tuning for readability and cohesion

## Minimum Viable Version

The smallest useful version of this system is:

- deterministic appearance spec,
- one slot family,
- one simple armor generator,
- 64x64 icon renderer,
- generated icon cache,
- item UI integration.

This is enough to validate the system before tackling worn sprite sheets.

## Open Technical Decisions

### Generated textures in runtime vs content pipeline

We need to decide whether generated textures should:

- be written into a runtime cache and loaded directly at runtime, or
- be baked into the normal content pipeline ahead of play.

Current recommendation:

- runtime loading for generated outputs,
- content pipeline reserved for authored assets.

### Source model representation

We need to decide whether source models are:

- parametric definitions only,
- mesh plus metadata,
- modular part references plus transforms.

Current recommendation:

- modular part references plus baked metadata for the first version,
- leave full custom mesh generation for later if needed.

### Outfit output granularity

We need to decide whether outfit outputs are:

- per armor piece overlays,
- per complete equipped loadout,
- or hybrid.

Current recommendation:

- complete equipped loadout baking for shipping quality,
- optional overlay mode for iteration and debugging.

## Risks

### Art inconsistency risk

If the generator has too much freedom, the results may not match the game's visual language.

Mitigation:

- constrain the grammar,
- define palette families,
- define silhouette libraries per slot and material.

### Cache explosion risk

Outfit combinations can grow very quickly as more slots and rigs are supported.

Mitigation:

- cache only on demand,
- use deterministic keys,
- add cache cleanup rules,
- support partial regeneration.

### Readability risk

A good 3D model can still produce a bad 64x64 icon.

Mitigation:

- design slot-specific icon cameras,
- enforce silhouette checks,
- keep strong contrast between main forms and trim details.

### Integration complexity risk

Trying to replace the current rendering architecture all at once would be expensive and fragile.

Mitigation:

- treat this as an asset generation system first,
- integrate through existing texture paths,
- expand incrementally.

## Proposed First Implementation Slice

The best first slice for this repository is:

1. introduce `ArmorAppearanceSpec`,
2. derive it from `EquipmentData` plus `Equipment.Hash`,
3. generate and cache a 64x64 item icon,
4. expose that icon through the existing item snapshot and item UI path,
5. defer worn sprite sheet generation until icons are stable.

This gives immediate player-facing payoff while staying aligned with the codebase as it exists today.

## Suggested Future Code Areas

These are likely future homes for the system, subject to refactoring:

- `Project 1/System/Appearance/`
- `Project 1/System/Appearance/Generation/`
- `Project 1/System/Appearance/Caching/`
- `Project 1/System/Appearance/Rendering/`

Potential types:

- `ArmorAppearanceSpec`
- `ArmorAppearanceKey`
- `ArmorSourceModel`
- `ArmorModelGenerator`
- `ArmorIconRenderer`
- `ArmorOutfitRenderer`
- `ArmorAppearanceCache`
- `GeneratedTextureRegistry`

## Recommended Next Step

Implement the planning and scaffolding for Phase 1 and Phase 2 only.

Specifically:

- define deterministic appearance data,
- define cache keys,
- add a generated texture loading path,
- render generated 64x64 icons for a limited set of armor slots.

That gives us a manageable path to prove the system before we commit to worn unit sprite sheet generation.
