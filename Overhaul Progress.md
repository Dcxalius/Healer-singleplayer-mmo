# Overhaul Progress

Last updated: 2026-02-03

## In Progress
- None.

## Finished — waiting on other implementation
- Render snapshots for entities/world objects/spawners/projectiles/corpses/doodads + VisualEffect snapshots.
- UI/input escape routing via UI-first event, with sim state change on Escape.

## Finished
- Texture render snapshots (sim builds, main draws).
- ThreadAffinity utility + main/sim/ui registration + broad asserts in UI draw paths and input caches.
- ThreadingSettings kill switch (P1_SINGLE_THREAD + per-thread disables).
- Mailboxes message bus (Main/UI/Sim) with typed Publish/Subscribe.
- UI thread loop (UiThread) + UI input snapshots/caches.
- Sim thread loop (SimThread) running StateManager.Update on pulse.
- RenderSnapshotManager builds world snapshots (entities/doodads/corpses/projectiles/spawners/tiles).
- Tile transparency map snapshots via TileRenderCache (sim builds, main applies).
- WorkerPool jobs for pathfinding + chunk generation; save/load parsing off-thread.
- Gameplay→HUD direct calls migrated to mailbox events (inventory/party/guild/plates/loot/channeling/etc.).
- Tile render snapshots (ChunkRenderSnapshot; main thread draws snapshots, sim builds/caches).
- Particle + FloatingText render-owned (sim spawn queues, main update/draw; cleared on main menu).
- Render cache with spawn/despawn (stable ids) for entities/world objects/spawners/tiles.
- Main/UI->sim mailbox payload migration (phase 1): target/party/interact + inventory/equipment commands now pass render IDs/value data instead of live object refs.
- Main/UI->sim mailbox payload migration (phase 1b): save-load requests now pass save name (not `Save` object), and spell-cast requests now pass spell name (resolved on sim via `SpellBook`).
- Main/UI->sim mailbox payload migration (phase 1c): world click/release/scroll commands now pass value payloads (relative positions/buttons/modifier masks) instead of mutable input event objects.
- Sim->UI payload migration (phase 2a): cast-channel events now pass value payloads (`GfxPath`, duration, progress) instead of live `Entity`/`Spell` references.
- Sim->UI payload migration (phase 2b): target/plate/nameplate events now pass `EntityUiSnapshot`/render IDs instead of live `Entity` references.
- Sim->UI payload migration (phase 2c): guild/party/inspect/buffs now use snapshots/render IDs; party command/control events use render ID payloads (no live `GuildMember` refs).
- Sim->UI payload migration (phase 2d): character/equipment/stats/exp/spellbook/spellbar/player-plate/gold events now use render IDs + value snapshots (`CharacterWindowSnapshot`, item snapshots, spell-name snapshots) instead of live `Friendly`/`Player`/`Spell` refs.
- Sim->UI payload migration (phase 2e): `InventoryAssigned` now passes an `InventoryUiSnapshot` (bag/item snapshots) instead of a live `Inventory` reference.
- Sim->UI payload migration (phase 2f): `InventorySlotChanged` now carries `InventoryUiSnapshot` as well; inventory UI refresh no longer consumes live `Inventory` instances.
- Cross-thread payload hardening: `GossipOpened` now carries `GossipUiSnapshot`; worker completion event now uses completion IDs (`WorkerCompletionReady`) instead of `Action` payloads; `SaveLoadPayload` no longer carries a live `Save` reference (uses save name lookup on apply).
- UI-event payload hardening: replaced UI object references in hold/dialog/hud-size events with value IDs (`HeldItemStart.SourceUiElementId`, `HeldSpellStart.SpellName`, `HudSizeChangerSet.UiElementId`, `DialogueClosed.DialogueBoxId`) and added `UIElement` ID registry lookup on UI thread.
- Logic window flow hardened: UI click now requests a sim-built logic-tree snapshot (`LogicWindowSnapshotRequested` → `LogicWindowSnapshotSet`), and `NodeViewer` draws from snapshot data (no `ObjectManager` reads on UI/main thread).
- Mailbox hardening pass: subscriber registry is now concurrent + copy-on-write per message type, and dispatch isolates handler exceptions instead of breaking the mailbox pump.
- Mailbox typed-channel pass: replaced object queue with per-type typed channels + a global dispatch-order queue (preserves cross-type publish order while avoiding boxed struct payloads).
- Thread-affinity/assert sweep (continuation): added explicit main-thread asserts for `SimThread`/`UiThread` pulse lifecycle and `WorkerPool` start/stop; constrained worker completion apply to sim thread; tightened `SaveManager.SaveData`/`TimeManager.Init`/`TimeManager.Load` affinities; made `RandomManager` thread-safe for cross-thread callers.
- Thread-affinity/assert sweep closure: added `ThreadAffinity.AssertGameThread` guardrail and applied it to shared utility entry points (`TimeManager.StartPause/StopPause`, `SaveManager.Saves`/`NameAlreadyExists`/`TryGetSaveByName`); also constrained `SaveManager.RequestScreenshot` to sim thread.
- UI draw-list pipeline ownership tightened: when UI thread is running, `GameState` now consumes already-built UI/plate snapshots and no longer rebuilds draw lists on main thread (single-thread fallback still rebuilds locally).
- State-transition lock cleanup: removed redundant `HUDManager.UiLock` usage inside UI state enter/leave paths (`MoveHUD`, `OptionMenu`, `LoadingMenu`, `NewGame`) and narrowed `StateManager.ApplyStateChange` locking to the no-UI-thread fallback path.
- Non-transition lock trim: `GameState` dirty/heartbeat signaling is now lock-free (`Interlocked` + volatile flags), removing extra `HUDManager.UiLock` contention from sim heartbeat and invalidation callbacks.
- Lock-scope reduction pass: fallback/main-thread paths now only take `HUDManager.UiLock` when another worker thread can contend (`SimThread`/`UiThread` running), reducing lock usage in pure single-thread fallback for `Game1` update dispatch and state/menu rescale paths.
