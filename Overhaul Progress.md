# Overhaul Progress

Last updated: 2026-02-12

## In Progress
- None.

## Finished — waiting on other implementation
- None.

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
- UI-thread rescale routing: main thread now publishes `HudRescaleRequested` on window size changes, UI thread handles HUD/menu rescale, and GPU-only resize stays on main thread (state rescale split accordingly).
- Texture metadata catalog: non-main thread texture size/avg color lookups now use `TextureCatalog`, and `TextureManager` accessors assert main-thread usage.
- GPU-affinity asserts: `Texture`/`UITexture`/`Text` draw paths and debug shape draws now assert main-thread usage.
- GPU-affinity asserts (cont.): added main-thread guards to particle, hitbox, and visual-effect draw paths.
- GPU-affinity sweep: added main-thread guards to misc draw entry points (game draw list, option menu draw, entity/spawner minimap draws, tile/chunk draws, cooldown texture, assignable image, ground effects).
- GPU-affinity sweep (cont.): added main-thread guards to held item/spell drawing and world/ground-effect draw entry points.
- GPU-affinity sweep (cont.): added main-thread guards across UI widget draw overrides, node viewer/minimap draw, and corpse draw.
- UI invalidation: mouse snapshot updates now invalidate the active UI surface on position/scroll change (HUD for game/move-hud, state UI otherwise).
- UI snapshot hardening: removed plate-box refresh/set APIs that accepted live `Entity`/`GuildMember` objects; character/inspect windows now consume snapshots only.
- UI snapshot hardening: descriptor tooltip events now carry `ItemDescriptorSnapshot` (no live `Item` references), and descriptor box renders from immutable snapshot text payloads.
- Payload hardening sweep (non-`GfxPath`): converted mailbox item/stat payloads to immutable snapshots (`ItemUiSnapshot`, `StatReportSnapshot`), replaced invite `IList` payloads with arrays, and made `SaveLoadPayload` immutable (constructor-only token arrays).
- Mailbox diagnostics (phase start): added mailbox counters for total published/dequeued, dispatch misses, no-subscriber deliveries, handler invocations/failures; debug overlay now shows queue peak plus publish/failure totals per mailbox.
- Mailbox diagnostics (phase follow-up): added opt-in typed mailbox coalescing (registered for keyboard/keybind/mouse snapshots and movement intent), plus `TotalCoalesced`/`TotalDropped` counters and overlay/threshold warning output for coalesced/dropped spikes.
- Thread-loop diagnostics: added `SimThreadStats`/`UiThreadStats` (last/avg/max frame ms, overrun count at 16ms budget, pulse wait timeouts), and surfaced both in debug overlay.
- Diagnostics threshold alerts: added non-spam warning logs (cooldown-based) for mailbox backlog, mailbox handler-failure deltas, and sim/UI thread overrun/timeout timing issues.
- Command-routing migration (phase 1 of mailbox ownership cleanup): gameplay command subscriptions in `StateManager` now subscribe on `Mailboxes.Sim`, with a temporary `Main->Sim` forwarding shim for existing publishers; `SimThread` now drains both `Main` and `Sim` each pulse.
- Command-routing migration (phase 2): gameplay/state command publishers were switched from `Mailboxes.Main.Publish(...)` to `Mailboxes.Sim.Publish(...)` across UI/input/save/state call sites; single-thread fallback now drains `Mailboxes.Sim` in `Game1.Update`.
- Command-routing migration (phase 3 cleanup): removed temporary `Main->Sim` forwarding shim in `StateManager`; gameplay command handlers are now subscribed only on `Mailboxes.Sim` (with `Main` reserved for worker completion traffic).
- Mailbox ownership cleanup (worker completion): `WorkerCompletionReady` now publishes/subscribes on `Mailboxes.Sim`, and `SimThread` no longer drains `Mailboxes.Main` each pulse.
- Payload hardening sweep (non-`GfxPath`, continuation): event payload enums are now messaging-local (`StateKind`, `ClickKind`, `RelationToPlayerKind`, `EquipmentSlotKind`, dialogue enums) with explicit boundary conversions, reducing direct cross-layer type coupling in mailbox contracts.
- Snapshot draw cutover: `Game.Draw` world pass now renders through `RenderSnapshotManager.DrawGameSnapshots(...)`, consolidating tile/entity/world-object/spawner/projectile draw onto the snapshot pipeline boundary.
- Escape routing finalized: Escape is now explicitly split into UI event (`EscapePressed`) and sim command (`EscapeRequested`) so UI-first consumption is enforced before any sim state transition.
- Render boundary hardening: added explicit `DrawSnapshots(...)` world-manager entrypoints and routed `RenderSnapshotManager` through them; legacy manager `Draw(...)` methods now exist only as `[Obsolete]` wrappers.
- Camera snapshot hardening: non-sim camera reads now use immutable `CameraRenderSnapshot` data, while sim-owner updates publish snapshots each frame (and on resize/load/zoom), reducing live cross-thread camera-state races.
- Hot-allocation sweep (plan §11): removed per-frame light-position array allocations in `Game.Draw`, replaced party position snapshotting with fixed-count copy/snapshot fields, removed per-tick `Chunk[,]` allocation in `TileManager.Update`, and replaced `PartyControlCleared` array payload with fixed render-id fields.
- Hot-allocation sweep (plan §11, cont.): removed repeated `ObjectManager` aggregate-list allocations by reusing a scratch list for update/server-tick/plate refresh, and removed per-snapshot `Chunk[]` allocation in `TileManager.BuildRenderSnapshot` via a reusable sorted chunk scratch list.
- Hot-allocation sweep (plan §11, cont.): minimap snapshot publishing now uses double-buffered reusable storage (`MinimapSnapshotManager`) with a count-based read API (`TryGetSnapshot`), removing per-frame `List`+`ToArray` churn in minimap dots.
- Hot-allocation sweep (plan §11, cont.): `HUDManager` UI/hud-move draw-list rebuilds now use double-buffered reusable `UiDrawList`/`HudMoveDrawList` snapshots (copy+grow buffers) instead of `hudElements.ToArray()` / `dialogueBoxes.ToArray()` on invalidation.
- Hot-allocation sweep (plan §11, cont.): plate draw-list rebuild path is now non-alloc at rebuild time; `HUDManager` uses double-buffered reusable `PlateDrawList` snapshots plus scratch capture via new `NamePlateHandler.CopyDrawList(...)` / `PlateBoxHandler.CopyDrawList(...)` APIs.
- Hot-allocation sweep (plan §11, cont.): `NamePlateHandler.Update()` now reuses scratch collections for collision resolution and iterates unique pairs directly (`j = i + 1`), removing per-update `ToList`/temporary list allocations and reverse-pair containment checks.
- Hot-allocation sweep (plan §11, cont.): keybind snapshot path no longer allocates per frame; `InputManager.PublishKeyboardSnapshots()` now publishes bitmask-based `KeyBindSnapshot`, and `UiKeyBindStateCache` / `KeyBindStateCache` consume masks directly (no bool-array snapshots/copies).
- Hot-allocation sweep (plan §11, cont.): removed high-frequency modifier-array churn in key evaluation by switching `KeySet` modifier checks to `InputManager.IsHoldModifierHeld(...)` (no `CheckHoldModifiers()` allocations in keybind polling loops).
- Hot-allocation sweep (plan §11, cont.): tile transparency snapshot publishing now reuses double buffers in `TileRenderCache` and swaps under lock, removing per-update `Color[]` allocations for transparency maps.
- Hot-allocation sweep (plan §11, cont.): common visual-effect snapshot builds are now non-alloc for 0/1/2 active effects via `VisualEffectSnapshotBatch` inline storage (`GameObject.BuildEffectSnapshotBatch()`), with array fallback only when an object has more than two simultaneous effects.
- Hot-allocation sweep (plan §11, cont.): click/release/scroll modifier payloads are now mask-based (`Modifiable` + event constructors + `InputManager` event creation), removing per-event `bool[]` allocations on pointer input.
- Hot-allocation sweep (plan §11, cont.): removed unused legacy array-return draw-list APIs (`NamePlateHandler.GetDrawList()`, `PlateBoxHandler.GetDrawList()`) after the non-alloc copy/counted draw-list path became the only call site.
- Render-boundary hardening (follow-up): minimap rendering now targets explicit snapshot entrypoints (`DrawMinimapSnapshots`) and legacy draw/minimap wrappers are guarded (`[Obsolete]` + debug asserts) to fail fast if live-object paths are accidentally reintroduced.
- Render-boundary hardening (closeout): removed the remaining legacy draw/minimap wrapper methods entirely (`ObjectManager`/`TileManager`/`SpawnerManager` draw wrappers; legacy manager draw wrappers; legacy live-object minimap methods on `Entity`/`Chunk`/`Spawner`), leaving snapshot entrypoints as the only API.
- UI invalidation refinement: moved away from raw mouse-delta invalidation by tracking `UIElement` interaction-version changes (hover/press/release/move/resize/visibility), then invalidating HUD/state surfaces only when interaction state actually changes; click/release/scroll now invalidate only when UI/HUD consumes input.
- UI invalidation refinement (tuning): reduced interaction-version false positives by guarding no-op visibility toggles, ignoring HUD-move state flips for non-moveable widgets, and only bumping interaction version when clamped moves actually change final position.
- State UI draw-list allocation sweep: converted non-game states (`StartScreen`/`PauseMenu`/`LoadingMenu`/`NewGame`/`MoveHUD`/`OptionMenu`) to reusable double-buffered `UiElementDrawList` snapshots, and added count-based draw-list copy APIs in `OptionManager` to avoid per-update list/array recreation.
