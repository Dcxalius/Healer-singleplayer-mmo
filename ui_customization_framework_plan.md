# UI Customization Framework Plan

## Goal

Build a reusable UI customization framework that lets players:

- scale the whole UI
- scale UI categories independently
- move and resize HUD elements
- tune icon and plate/bar sizing
- customize chat and spellbar presentation
- eventually rearrange internal layouts inside windows such as `CharacterWindow`

This plan is intentionally phased. The current codebase already supports some coarse HUD move/resize behavior, but deeper customization requires a more structured persistence and layout model.

## Current State Summary

The project already has a few useful building blocks:

- `HUDManager.Save()` persists HUD data into `Hud.set`
- `Hud.def` / `Hud.set` currently store tuples of `(typeName, relativePos, relativeSize)`
- `MoveHUD` mode already exposes move and simple size editing through `SizeChanger`
- HUD elements and plate boxes can already be rescaled and redrawn on window changes
- nameplates and plate boxes are handled separately from the main HUD list

Important limitations in the current design:

- persistence is keyed by type name rather than stable element ids
- multiple instances of the same type rely on ordering, which is fragile
- only top-level position and size are persisted
- there is no concept of global scale, category scale, element presets, or visibility profiles
- window internals are hardcoded
  - example: `CharacterEquipmentPanel` computes slot positions from static layout constants
  - example: `Window.Init(...)` assigns a single global booklet size and placement pattern
- spellbar layout is effectively width-driven, not defined by a reusable layout schema
- chat, plate bars, icon sizes, and content spacing do not have first-class configuration objects

## Design Principles

1. Prefer stable ids over type names.
2. Separate player-facing settings from runtime widget implementation.
3. Treat top-level placement, scaling, and internal layout as different layers.
4. Keep the first phase low-risk by reusing existing HUD move/resize behavior where possible.
5. Preserve safe defaults and provide reset paths at global, category, and element level.
6. Avoid storing layout state implicitly in constructor math.
7. Support migration from existing `Hud.def` / `Hud.set` data instead of breaking saved UI layouts.

## Target Capability Model

The finished system should support four levels of customization.

### 1. Global

- master UI scale
- UI safe-area padding
- optional font scale multiplier
- reset all UI

### 2. Category

- windows scale
- chat scale
- spellbar scale
- plate box scale
- nameplate scale
- icon scale
- buff/debuff scale
- minimap scale

### 3. Element

- visibility
- anchor / position
- size
- scale override
- opacity if desired later
- element-specific properties
  - spellbar button count
  - spellbar direction and row count
  - chat width, height, visible rows
  - plate width, height, buff box spacing

### 4. Internal Layout

- per-window section arrangement
- element grouping inside windows
- optional presets
- long-term drag-and-drop arrangements for things like:
  - character equipment slots
  - stat report position
  - experience bar placement
  - icon groups and panel ordering

## Proposed Architecture

### A. Introduce a `UiProfile` save model

Replace the current tuple list with a structured profile object.

Suggested shape:

```csharp
internal sealed class UiProfile
{
    public int Version { get; set; }
    public UiGlobalSettings Global { get; set; }
    public Dictionary<string, UiCategorySettings> Categories { get; set; }
    public Dictionary<string, UiElementSettings> Elements { get; set; }
    public Dictionary<string, UiWindowLayoutSettings> WindowLayouts { get; set; }
}
```

Suggested submodels:

- `UiGlobalSettings`
  - `MasterScale`
  - `SafeAreaPadding`
  - `FontScale`
- `UiCategorySettings`
  - `Scale`
  - `Visible`
- `UiElementSettings`
  - `Anchor`
  - `RelativePosition`
  - `RelativeSize`
  - `ScaleOverride`
  - `Visible`
  - `Properties`
- `UiWindowLayoutSettings`
  - window-specific section arrangement and child layout descriptors

### B. Assign stable element ids

Every top-level customizable UI component should have a stable key, for example:

- `hud.chat.primary`
- `hud.spellbar.primary`
- `hud.castbar.player`
- `hud.minimap.primary`
- `hud.plate.player`
- `hud.plate.target`
- `hud.party.plate.0`
- `hud.party.buff.0`
- `window.character.self`
- `window.guild`

This avoids relying on:

- class name matching
- array ordering
- repeated instances of the same type

### C. Add a customization registry

Create a small registry that defines:

- which UI elements are customizable
- their stable ids
- category membership
- default settings
- supported properties

Suggested concept:

```csharp
internal sealed class UiCustomizationDescriptor
{
    public string Id { get; init; }
    public string Category { get; init; }
    public RelativeScreenPosition DefaultPos { get; init; }
    public RelativeScreenPosition DefaultSize { get; init; }
    public bool SupportsMove { get; init; }
    public bool SupportsResize { get; init; }
    public bool SupportsInternalLayout { get; init; }
}
```

This registry becomes the migration source for old `Hud.def` data and the canonical source for new defaults.

### D. Split framework responsibilities

Recommended responsibilities:

- `UiProfileManager`
  - load, save, migrate, reset
- `UiCustomizationRegistry`
  - defaults and supported features
- `UiSettingsResolver`
  - merge defaults + profile overrides into runtime values
- `UiCustomizationController`
  - apply changes during gameplay / option mode
- `UiLayoutSerializer`
  - serialize window layout descriptors

## Phased Rollout

## Phase 1: Stable Persistence And Scaling Foundation

### Scope

- add `UiProfile`
- migrate from tuple-based `Hud.set`
- add stable ids for existing top-level HUD elements
- add master scale plus category scales
- keep the current move HUD workflow
- keep top-level window arrangement unchanged

### Deliverables

- new profile format stored alongside or replacing `Hud.set`
- migration from old tuple list
- category scale support for:
  - windows
  - chat
  - spellbar
  - plate boxes
  - nameplates
  - buff boxes
  - icons
- update HUD initialization to use resolved settings instead of hardcoded loaded tuples

### Why this first

This unlocks immediate value with relatively low risk and creates the persistence model the later phases depend on.

## Phase 2: First-Class Element Customization

### Scope

- convert top-level HUD elements to stable customizable entries
- expose visibility, position, size, and scale override
- improve `MoveHUD` mode to edit selected element metadata instead of only raw size
- add reset per element and per category

### Example targets

- chat panel
- primary spellbar
- cast bar
- minimap
- save indicator
- player / target / party plates
- buff boxes

### Deliverables

- element selection panel in HUD edit mode
- clearer save/apply/reset behavior
- better metadata display than the current generic `SizeChanger`

## Phase 3: Rich Subsystem Controls

### Scope

Add subsystem-specific options that are not just size and position.

### Chat

- width / height
- row count
- font scale
- input box scale
- optional padding controls

### Spellbar

- button count
- row count
- orientation
- button size
- icon spacing
- optional keybind label scale

### Plates / Nameplates

- player / target / party width and height
- health/resource bar thickness
- level circle size
- buff icon size and spacing
- nameplate font scale
- collision padding for nameplates

### Windows

- default open size per window type
- open position policy
- optional independent scale per window class

## Phase 4: Internal Window Layout Framework

### Scope

This is the step that enables “arrangement of Character Window” and similar deep customization.

### Key change

Window internals must stop being defined purely by constructor math and become data-driven section layouts.

### Proposed model

Represent a complex window as layout sections, for example:

- header
- equipment panel
- stats panel
- tabs
- exp bar
- footer

Each section gets:

- stable section id
- relative bounds
- visibility
- optional child layout settings

Suggested example:

```csharp
internal sealed class UiWindowSectionLayout
{
    public string SectionId { get; set; }
    public RelativeScreenPosition Position { get; set; }
    public RelativeScreenPosition Size { get; set; }
    public bool Visible { get; set; }
    public Dictionary<string, string> Properties { get; set; }
}
```

### Character window implications

This is a bigger refactor because:

- `CharacterEquipmentPanel` currently computes slot placement from static constants
- `CharacterWindow` assumes fixed report and exp bar placement
- `Window` uses one shared size model for all booklets

### Recommendation

Do not start with freeform drag-and-drop of every child widget.

Start with:

- section presets
- movable panels
- resizable panels
- later expose per-slot or per-grid customization only if still needed

## Phase 5: Presets, Profiles, And Polish

### Scope

- import/export UI profiles
- named presets
- role-based presets
  - healer
  - solo
  - party
- restore defaults for selected categories
- optional profile switching during gameplay

## Migration Plan

### Existing source

Current HUD persistence:

- `Hud.def`
- `Hud.set`
- tuple list of `(typeName, position, size)`

### Migration strategy

1. Load old tuple data if present.
2. Map entries into stable ids using known descriptors.
3. Preserve current position and size when possible.
4. Fill missing properties from defaults.
5. Save immediately into the new profile format.

### Special cases

- repeated types such as `PartyPlateBox` and `BuffBox` must map by known order during migration
- if tuple counts do not match expectations, fall back to defaults for that subgroup and log a debug warning

## UI/UX Proposal

## Player-Facing Entry Points

### Video / Interface options

For normal settings screens:

- master UI scale slider
- category sliders
- reset category
- open HUD editor

### HUD Editor Mode

Reuse `MoveHUD` as the main advanced customization mode.

Suggested upgrades:

- selectable element outline
- right-side inspector panel
- current element id / name
- move, size, visibility toggles
- scale slider
- reset selected
- duplicate profile / preset actions later

### Internal Window Layout Editor

Do not ship this in the first pass.

Ship only after section-based layouts exist.

## Recommended Data Ownership

### Keep top-level ownership in HUD systems

- `HUDManager` remains responsible for element creation and draw list assembly
- `PlateBoxHandler` remains responsible for plate entities
- `WindowHandler` remains responsible for opening and snapshot binding

### Move settings ownership out of constructors

Constructors should stop owning player settings logic directly.

Instead:

- create with defaults
- apply resolved customization after creation
- reapply when settings change

## Proposed Initial Subsystem Breakdown

### HUD core

- add `UiProfile`
- add registry + resolver
- migrate `HUDManager.ImportSettings()` and `HUDManager.Save()`

### Plates

- add stable ids to player/target/party plate boxes and buff boxes
- add category + per-element scale support

### Chat

- expose chat as first-class customizable element rather than a saved tuple only

### Spellbar

- separate “bar size” from “button layout”
- add spellbar config object

### Windows

- add stable top-level window ids
- keep internal layouts fixed until section system exists

## Risks And Tradeoffs

### 1. Static layout math

Several UI classes compute sizes from static values or global window constants. That makes fine-grained customization difficult until layout becomes data-driven.

### 2. Save format churn

Moving from tuples to a richer profile is worth it, but migration needs to be deliberate to avoid surprising users.

### 3. Order-based repeated elements

Party plates and buff boxes currently depend on array order and creation order. Stable ids need to replace that.

### 4. Threading and redraw behavior

UI runs through a mix of main thread and UI thread helpers. Live customization should centralize invalidation so settings changes remain safe and predictable.

### 5. Scope creep

“Allow the player to modulate everything” can expand indefinitely. The phased model protects delivery by separating:

- global/category scaling
- top-level element editing
- subsystem-specific tuning
- deep internal arrangement

## Recommended First Implementation Slice

If we start after this planning pass, the best first coding slice is:

1. Introduce `UiProfile` and stable ids.
2. Migrate current HUD tuple saves into the new format.
3. Add master scale and category scales.
4. Apply scaling to:
   - chat
   - spellbar
   - cast bar
   - minimap
   - player/target/party plates
5. Extend `MoveHUD` to show selected element metadata and save by stable id.

This gets real player value quickly without needing the large internal-window refactor yet.

## Acceptance Criteria By Milestone

### Milestone A

- old `Hud.set` data migrates successfully
- HUD positions and sizes still load correctly
- global scale changes visible HUD sizes consistently

### Milestone B

- players can move and resize top-level HUD elements
- players can save, reset selected, and reset all
- spellbar, chat, and plates have category scaling

### Milestone C

- subsystem-specific options exist for chat, spellbar, and plates
- no reliance on type-name tuple ordering for persistence

### Milestone D

- at least one complex window uses section-based layout data
- character window sections can be rearranged or resized safely

## Open Questions

- Should global scale affect fonts uniformly, or should text scale be separate from panel/icon scale?
- Should nameplates and plate boxes share one category or be split?
- Should windows remember open/closed state in the same profile, or remain gameplay-driven?
- Do we want one spellbar config system designed for multiple bars now, even if only one bar is currently active?
- For character window arrangement, do we want:
  - preset layouts first
  - drag-and-drop section editing first
  - full child-widget editing later

## Recommendation

Proceed with the framework in two broad tracks:

- Track 1: scalable, stable, saveable top-level UI customization
- Track 2: data-driven internal window layouts

Track 1 should ship first. Track 2 should begin only after the new profile and stable-id foundation are in place.
