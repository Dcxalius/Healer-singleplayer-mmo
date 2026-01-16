# Overhaul Checklist — Progress & Remaining Steps

## Completed
- Added thread-affinity guardrails (`Managers/ThreadAffinity`) and asserted main-thread initialization for GPU/static managers (GraphicsManager, EffectManager, TextureManager, StateManager, HUDManager). `Program`/`Game1` capture main thread and init systems early.
- Introduced mailboxes/message bus (`Messaging/Mailbox`, `Messaging/Mailboxes`) and bridged input to events (`InputManager` publishes to UI mailbox, `InputEventBridge` routes to current handlers).
- Refactored rendering helpers:
  - FloatingText: removed per-text render targets/SpriteBatch; main-thread-only draw using `Text`.
  - Tile transparency/minimap: added `TileRenderCache` for transparency maps and chunk minimap targets; draw now uses cache.
- Began event-driven UI hooks (HUDManager subscribes to UI mailbox events):
  - Inventory slots: `InventorySlotChanged` events; HUDManager updates HUD slots.
  - Loot open: `LootOpened` events; loot UI builds from `LootState` snapshots.
  - Casting: channel start/progress/cancel/finish events.
  - Equipment/stats/exp/nameplates/plates/targets: events replace HUDManager calls; bridge routes to HUD handlers.
  - Inventory assignment, spellbook refresh, spellbar load, character window/player plate set, gold change, guild invite status: now event-driven.
  - Party/guild control: add/remove walkers, add/remove members, guild roster, party cleared events routed to HUD.
  - Buff added events → plate boxes.
  - Gossip/Shop: events replace direct window calls.
  - Descriptor box events replace direct HUD descriptor calls on hover.
- Gameplay classes updated to publish events instead of HUDManager (Player, SpellBook, Equipment, BaseStats, EntityStats, EntityAttack, BuffList, Party, Guild, Npc, Corpse, Chest, SpellCast, ObjectManager).
- Removed mailbox request/response hacks for loot/inspect/shop state; inventory UI now queries `LootState` and HUD-managed state directly, and duplicate `ShopOpened` definitions were collapsed.
- Loot events are now consumed directly by `HUDManager` (no UIEventBridge hop); `LootBox` builds from `LootState` snapshots and refreshes on slot-change/remove events.
- Loot open now emits snapshots immediately (`LootState.Open` used at publish sites), eliminating re-clone work inside HUD consumption.
- Loot drops cache dropper position/height instead of holding live world-object references, reducing cross-thread coupling risk; `LootOpened` also carries a UI-safe `LootContext` for range/despawn checks and `LootClosed` fires on corpse despawn.
- UI invalidation hook added: loot events/closures mark the UI dirty; `GameState` redraws UI targets only on invalidation or a 1s heartbeat.
- Split UI render targets (plates vs windows) with separate dirty flags; plate/nameplate events now invalidate only the plate surface.
- UI render targets are recreated on rescale; both UI and plate targets track dirty separately.
- Replaced remaining UI-element HUDManager calls with UI mailbox events (dialogue close, HUD size changer, HUD save).
- Keyboard input now flows main → UI → sim via snapshots; sim uses cached keybind/keyboard state for movement, pause, debug, and camera toggle.
- Mouse position now flows main → UI → sim via snapshots; sim uses cached mouse state in camera/debug logic.
- UI hover/drag now reads mouse snapshots via `UiMouseStateCache` instead of `InputManager`.
- UI keybind/keyboard reads now use `UiKeyboardStateCache`/`UiKeyBindStateCache` instead of polling `InputManager`.
- InputBox text entry now uses `UiTextInputManager` with keyboard snapshots; input capture no longer reads `InputManager`.
- UI draw lists are now snapshot on the UI thread (UI elements + plates) and main thread draws from snapshots using per-surface invalidation flags.
- Removed UIEventBridge; HUDManager now owns UI mailbox subscriptions for UI state updates.
- State changes now route through sim-thread requests instead of direct UI-thread calls.

## In Progress / Partial
- Loot pipeline:
  - `LootState` owns cloned loot arrays and publishes slot-changed/slot-removed updates; UI now uses `LootContext` for range/despawn checks and no longer receives `LootDrop`.
- Render cache: transparency/minimap done; verify no remaining GPU work in gameplay (e.g., other helpers).

## Remaining
- Finish loot eventing:
  - Emit loot snapshot on open; keep UI fully driven by snapshots/events (no HUDManager wiring).
  - Ensure UI invalidation/redraw cadence aligns with event-driven loot updates.
- Sweep remaining HUDManager direct calls (HUD internals) now that draw-list refactor landed.
- UI threading:
  - Add UI thread with event-driven state/hit-testing, invalidation flags per UI surface, draw-list production.
  - Main thread: redraw dirty targets + 1s heartbeat.
- Simulation threading:
  - Move game update chain to sim thread with command handlers; main thread only draws.
  - Publish render/UI events from sim; consume input commands from main/UI threads.
- Cleanup/Telemetry:
  - Add thread assertions in hot paths, queue depth/timing diagnostics, and kill switch for single-thread fallback.
  - Replace temporary bridges with direct event consumption once UI/sim threads are in place.