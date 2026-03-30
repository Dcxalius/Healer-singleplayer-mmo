# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run

```bash
dotnet build 'Project 1.sln' -v q                     # Full solution (runs content pipeline)
dotnet build 'Project 1/Project 1.csproj' -v q        # Game project only
dotnet run --project 'Project 1/Project 1.csproj'     # Launch game
```

On Linux-based agents, `mgcb` (content pipeline) may fail if Wine is unavailable. Treat this as an environment issue unless content files changed.

There is no automated test project. For code changes: build the solution, exercise the feature in-game, and note unverified paths in the change summary.

## Code Style

- 4-space indentation, ASCII unless the file already uses Unicode
- `PascalCase` for types, methods, public members; `camelCase` for locals and private fields (existing `aParameter` style is acceptable)
- Prefer small domain-focused classes over large manager files
- State orchestration belongs in routers/managers; domain behavior stays near the owning system

## Architecture Overview

### Entry Point & Threading

`Game1.cs` initializes all systems and coordinates three optional threads:
- **Main thread** — MonoGame render loop
- **Sim thread** (`SimThread`) — world/game simulation
- **UI thread** (`UiThread`) — UI updates

Threading mode is configurable via `ThreadingSettings`. All code must be safe for single-threaded mode as well.

### Message Bus (`System/Managers/Messaging/`)

All cross-system communication goes through `MailboxManager`, which owns three mailboxes:
- **Main mailbox** — main thread events
- **UI mailbox** — UI thread events
- **Sim mailbox** — simulation commands (registered with `RegisterSimCommandType<T>()`)

Domain-specific **routers** handle command dispatch (`InventoryCommandRouter`, `SpellCastRouter`, `ChatCommandRouter`, `ShopCommandRouter`, `SessionFlowRouter`, `WorldInteractionRouter`, etc.). Input is captured as typed snapshots (`KeyboardSnapshot`, `MouseSnapshot`, `KeyBindSnapshot`) and coalesced to reduce message churn.

### State Machine (`System/Managers/States/`)

Seven states managed by `StateManager`: `StartScreen → NewGame → Game ↔ [OptionMenu, PauseMenu, LoadingMenu, MoveHUD]`. Each state owns its Update, UiUpdate, and Draw logic.

**`StateManager.cs` is the primary refactoring target** (≈1,900 lines). It currently mixes state transitions, sim subscriptions, UI routing, chat command parsing, ground-target spell preview/rendering, and logic-tree snapshot building. See `SpringClean.md` for the full decomposition plan. When editing `StateManager`, prefer moving logic out rather than adding more to it.

### World (`World/`)

- **`Entity`** (`World/GameObjects/Entities/`) — base for all living creatures; handles movement, combat, resources, stats, aggro/threat. Over 1,000 lines; most gameplay logic flows through here.
- **`ObjectManager`** — world object lifecycle, party management, and render snapshot publication. Partially decomposed into `ObjectManager.Lifecycle.cs`, `ObjectManager.Lookup.cs`, etc.
- **`TileManager`** — chunk generation, spawning, and visibility culling. Partially decomposed into `TileManager.Generation.cs`, `TileManager.Spawning.cs`, `TileManager.Visibility.cs`.
- **Pathfinding** — A* in `World/Tiles/Navigation/PathFinder.cs`
- **Collision** — rectangle/circle/ellipse hitboxes in `World/Tiles/Collisions/`

### Spells & Buffs (`World/GameObjects/Spells/`)

Spells support instant casts, channeled casts, and area effects. Buffs carry a `BuffId` to allow duplicate effects and build their own snapshots. `RemoveBuff` is an event; buffs do not self-remove via `BuffBox`.

### Items & Inventory (`World/Items/`)

`Inventory.cs` is split into `Inventory.Bags.cs`, `Inventory.Equipment.cs`, `Inventory.Query.cs`, `Inventory.Stacking.cs`. Items are loaded from JSON in `Content/Data/`.

### UI (`UI/`)

`HUDManager` owns all in-game UI. `UIElement.cs` is the base control class (also a known refactoring target). Reusable components live in `UI/UIElements/`. Menus (`StartMenu`, `PauseMenu`, `OptionMenu`, `CharacterCreator`, `LoadingMenu`) each own their own state-specific logic.

### Graphics Pipeline

- `GraphicsManager` — device management, window scaling, scissor stack
- `EffectManager` — shader/effect management
- `TileRenderCache` — cached tile rendering
- `RenderSnapshotManager` — deferred render snapshots collected during update, flushed during draw
- `SuperSoftShadowRenderer` — shadow pass

### Data-Driven Content

All game data is JSON in `Content/Data/` (items, spells, mobs, NPCs, loot tables, classes, effects, projectiles). Loaded via `Newtonsoft.Json`. Factory classes (`ItemFactory`, `ObjectFactory`, `TileFactory`, `SpellFactory`) construct runtime objects from this data.

## Active Refactoring Plan

`SpringClean.md` documents the decomposition backlog. Priority order:
1. `StateManager` — split UI dispatch, ground-targeting, chat commands, logic tree, and domain request handlers into partial-class files
2. `DebugManager` — split into logging, draw, flags, commands
3. `ObjectManager` — split by spawn/despawn, lookup, update loop, render snapshot
4. `Inventory` / `EquipmentData`
5. `UIElement`
6. `TileManager` (continue current direction)
7. Messaging event files (group by domain)

When doing any refactoring, verify file ownership before moving code across systems.
