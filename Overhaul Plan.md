# Unified Overhaul Plan — Threaded Simulation + Threaded UI + Main-Thread Rendering (Project 1)

## 1. Goals (what “done” looks like)
1. Move all world simulation off the MonoGame main thread (fixed-step tick).
2. Move all UI state + hit-testing off the MonoGame main thread (event-driven).
3. Keep all GPU work on the MonoGame main thread (GraphicsDevice/SpriteBatch/RenderTarget2D/ContentManager.Load/effect parameter writes).
4. Eliminate direct game→HUD calls and replace them with explicit events (sim → UI) and commands (UI/main → sim).
5. Eliminate hidden thread-affinity hazards (notably side-effectful static constructors and render/GPU work in gameplay classes).
6. Add diagnostics so threading regressions are visible (queue depth, tick time, dropped messages) and add a kill switch to fall back to single-thread update.

## 2. Non-negotiable thread-affinity rules
1. Only the main thread may call:
   1. GraphicsDevice APIs.
   2. SpriteBatch.Begin/Draw/End.
   3. RenderTarget2D creation/binding and Texture2D.SetData.
   4. ContentManager.Load and any Texture2D.FromFile.
2. The simulation thread may not call:
   1. HUDManager (or any UI namespace types).
   2. GraphicsManager / EffectManager / TextureManager.
3. The UI thread may not call:
   1. GraphicsManager / EffectManager / TextureManager.
   2. SpriteBatch APIs.
4. Cross-thread communication is message passing only (queues/mailboxes). No shared mutable objects across threads.

## 3. Target runtime architecture (threads and what each owns)
1. Main thread (MonoGame Update/Draw):
   1. Poll raw input devices (Keyboard/Mouse).
   2. Enqueue input messages to the UI mailbox (and only optionally to sim, via UI-generated commands).
   3. Drain:
      1. Sim → Main render deltas / snapshots.
      2. UI → Main draw lists + invalidation flags.
   4. Execute all Draw() calls using render caches + UI draw lists.
2. Simulation thread (authoritative world, fixed-step):
   1. Owns gameplay state and update chain currently called from Managers/States/Game.Update():
      1. ObjectManager.Update()
      2. TileManager.Update() (after splitting render/GPU parts out)
      3. CorpseManager.Update()
      4. DoodadManager.Update()
      5. SpawnerManager.Update()
      6. ProjectileManager.Update()
      7. (Visual-only managers move out; see “Render-owned systems” below.)
   2. Consumes gameplay commands (from UI/main) and emits events + render deltas.
3. UI thread (state + hit-testing, no GPU):
   1. Owns HUDManager state (windowHandler/plateBoxHandler/namePlateHandler and all UI widgets).
   2. Consumes:
      1. Input events (from main).
      2. Simulation events (from sim).
   3. Produces:
      1. UI draw lists (pure data, no GPU calls).
      2. Dirty flags/invalidation markers for UI render targets.
4. Optional worker jobs (thread pool):
   1. Pathfinding jobs (TileManager.GetPath → PathFinder.GeneratePath).
   2. Save/load IO and JSON parsing (apply results back on sim/UI thread).

## 4. “Full scan” findings that must be addressed before threading
1. Side-effectful static constructors (thread hazard: first-touch thread wins).
   1. GPU/content work in static constructors (must be main-thread explicit init):
      1. Managers/EffectManager.cs (ContentManager.Load + SpriteBatch + RenderTargets).
      2. Textures/TextureManager.cs (ContentManager.Load for textures/fonts/effects).
      3. Managers/GraphicsManager.cs (GraphicsDeviceManager-owned factories; ensure first-touch is main).
      4. Managers/States/StateManager.cs (SpriteBatch creation).
   2. IO/static init that should not run “accidentally” on the wrong thread:
      1. UI/HUD/Managers/HUDManager.cs (loads HUD settings).
      2. Camera/Camera.cs (loads camera settings).
      3. Input/KeyBindManager.cs (loads keybind settings).
      4. Tiles/TileManager.cs (allocations, collision manager).
      5. Several factories: GameObjects/ObjectFactory.cs, Items/ItemFactory.cs, Items/LootFactory.cs, GameObjects/Spells/SpellFactory.cs, etc.
   3. Required change:
      1. Convert each side-effectful static constructor into an explicit Init() method.
      2. Call Init() from a known place on the correct thread:
         1. Main: GraphicsManager.Init, EffectManager.Init, TextureManager.Init, StateManager.Init render objects.
         2. UI: HUDManager.Init state + settings IO.
         3. Sim: ObjectFactory/ItemFactory/SpellFactory init if purely data; if they need textures, split data vs render binding.
2. Gameplay code directly mutates UI (must become events):
   1. GameObjects/Entities/EntityStats.RefreshPlates → calls HUDManager.plateBoxHandler.RefreshPlates(this).
   2. Items/Inventory swaps and slot updates → calls HUDManager.RefreshInventorySlot(...).
   3. GameObjects/Doodads/Chest → calls HUDManager.Loot(lootDrop).
   4. GameObjects/Entities/SpellCast → calls HUDManager.ChannelSpell/UpdateChannelSpell/CancelChannel/FinishChannel.
   5. Player constructor/load path → calls HUDManager.SetInventory, windowHandler.RefreshSpellBook, SetCharacterWindow, plateBoxHandler.SetPlayerPlateBox, RefreshGold, LoadSpellBar, etc.
   6. Party/Guild logic → calls HUDManager.plateBoxHandler.* and HUDManager.windowHandler.*.
3. GPU work embedded in gameplay classes (must become render-owned):
   1. GameObjects/FloatingTexts/FloatingText:
      1. Creates RenderTarget2D and SpriteBatch and draws into the target in the constructor.
      2. This must never happen on the sim thread.
   2. Tiles/TileManager.GetTransparent:
      1. Creates a Texture2D via GraphicsManager.CreateNewTexture and calls Texture2D.SetData.
      2. This must be render-owned (main thread) and moved out of sim logic.
   3. Tiles/Chunk.MinimapDraw:
      1. Creates a RenderTarget2D and calls SetData.
      2. Must be render-owned and removed from sim-owned chunk objects (or moved into a render cache keyed by chunk id).
4. Input read inside gameplay:
   1. Player.KeyboardWalk reads KeyBindManager.GetHold each frame.
   2. CameraMover reads InputManager mouse state.
   3. Required change:
      1. Only main thread polls raw input.
      2. UI thread interprets input → emits gameplay commands.
      3. Sim thread never reads InputManager/KeyBindManager directly.

## 5. Messaging layer (bus + mailboxes) — required shape
1. Use three mailboxes (thread-safe queues):
   1. MainMailbox: receives render deltas and UI draw lists.
   2. UiMailbox: receives input events and sim events.
   3. SimMailbox: receives gameplay commands.
2. Inside a thread, use typed channels:
   1. Subscribe<T>(Action<T> handler)
   2. Publish<T>(in T message) (no boxing; no per-publish allocation)
3. Publishing rules:
   1. Any thread may enqueue to another thread’s mailbox.
   2. Only the owner thread drains and dispatches.
4. Allocation rules:
   1. Event/command payloads are readonly structs where possible.
   2. Avoid object-based buses (Publish(object)).
   3. Avoid per-frame array creation (use fixed buffers, pooled arrays, or reuse lists).

## 6. Render caches and draw lists (how drawing stops touching live sim objects)
1. World render cache (sim → main):
   1. EntityRenderSnapshot (id, position, facing, animation state, sprite/texture id, tint, z-sorting info).
   2. Spawn/despawn events to maintain a stable render-side dictionary.
   3. Optional interpolation data (previous/current transforms) if needed later.
2. UI draw list (UI → main):
   1. A list of “UI primitives” (text, sprite, nine-slice, bar fill, clipping rect changes).
   2. Includes stable asset keys (texture ids / font ids) rather than Texture2D references.
   3. Produced per UI surface (HUD overlay, windows, nameplates, minimap, etc.) so render targets can be redrawn selectively.
3. UI render targets + invalidation:
   1. Each surface has:
      1. RenderTarget2D (main thread only).
      2. Dirty flag set by UI thread.
      3. Optional heartbeat redraw every 1s as a safety net.

## 7. Concrete migration phases (recommended order for this codebase)
### Phase 0 — Guardrails and deterministic init (no behavior changes)
1. Add a ThreadAffinity utility:
   1. Capture main thread id at startup.
   2. (Later) capture sim/ui thread ids.
   3. Provide AssertMainThread/AssertSimThread/AssertUiThread methods.
2. Replace side-effectful static constructors with Init() + explicit call order:
   1. Main-thread init: GraphicsManager.Init, TextureManager.Init, EffectManager.Init, StateManager.Init.
   2. UI-thread init: HUDManager.Init (settings IO).
   3. Sim-thread init: factories/data init (split data vs render binding).
3. Remove GPU work from gameplay constructors:
   1. FloatingText becomes a pure data request; render thread builds the render target/text.
   2. Tile transparency map generation moves to a render-owned service.
   3. Chunk minimap render targets move to a render-owned cache keyed by chunk id.

### Phase 1 — Event/command interfaces (still single-threaded)
1. Introduce the message types (structs) and the bus API.
2. Replace direct HUDManager calls from gameplay with publishes:
   1. Inventory changes → Inventory.* events.
   2. Spell casting → Cast.* events.
   3. Party/Guild → Party.* and Guild.* events.
   4. Loot interaction → Loot.* events.
3. Keep everything running on the main thread while verifying behavior.

### Phase 2 — Render-owned VFX systems
1. Convert visual-only systems to render-owned (main thread):
   1. ParticleManager becomes render-owned (spawn via events).
   2. FloatingTextManager becomes render-owned (spawn via events).
2. Sim emits:
   1. Vfx.SpawnRequest
   2. FloatingText.SpawnRequest (or Combat.DamageNumberRequested)
3. Main thread owns lifecycle and drawing.

### Phase 3 — UI thread (state + hit-testing)
1. Refactor InputManager:
   1. Main thread: poll Keyboard/Mouse and publish Input.* events.
   2. UI thread: input router performs hit-tests and updates UI state.
2. UI thread emits gameplay commands (not raw clicks):
   1. Cmd.MoveOrder / Cmd.SetTarget / Cmd.CastSpell / Cmd.LootTake / Cmd.InventorySwap, etc.
3. Replace GameState.UIDraw():
   1. Remove direct HUDManager.Draw calls from main-thread update logic.
   2. Main thread renders UI draw lists into UI surfaces when dirty.

### Phase 4 — Simulation thread (authoritative world update)
1. Move the update chain from Managers/States/Game.Update() onto the sim thread:
   1. ObjectManager.Update
   2. TileManager.Update (after splitting render parts)
   3. CorpseManager.Update
   4. DoodadManager.Update
   5. SpawnerManager.Update
   6. ProjectileManager.Update
2. Convert all gameplay entry points to commands handled on sim:
   1. Click handling currently in Game.Click/ObjectManager.Click/SpawnerManager.Click/CorpseManager.Click/DoodadManager.Click/ClickGround becomes UI-derived commands.
   2. Player.KeyboardWalk becomes UI commands (movement intent), not sim reading input.
3. Sim publishes:
   1. State events for UI (health, mana, exp, inventory, loot, etc.).
   2. Render deltas/snapshots for the main thread.

### Phase 5 — Worker jobs (pathfinding + IO)
1. Convert TileManager.GetPath/PathFinder.GeneratePath into a job:
   1. Sim submits path request.
   2. Worker returns path result.
   3. Sim applies result when received.
2. Move Save/Load parsing to jobs; only apply results on sim/UI thread.

## 8. Event and command catalog (verified from code + expanded)
### 8.1 Input events (main → UI)
1. Input.MouseClick {screenPos, button, modifiers, timestamp}
2. Input.MouseRelease {screenPos, button, modifiers, timestamp}
3. Input.MouseScroll {screenPos, steps, up, modifiers, timestamp}
4. Input.KeyPressed {key, modifiers, timestamp}
5. Input.KeyReleased {key, modifiers, timestamp}
6. Input.TextInput {character, timestamp}

### 8.2 UI/system events (UI ↔ Main)
1. UI.WindowToggle {windowId, open}
2. UI.DialogOpened {dialogId, payload}
3. UI.DialogClosed {dialogId}
4. UI.HUDMoveModeChanged {enabled}
5. UI.RedrawRequested {surfaceId}
6. HUD.RescaleRequested {screenW, screenH}
7. HUD.SaveRequested {layoutData}
8. UI.HeartbeatRedraw {timestamp}

### 8.3 Commands (UI/Main → Sim) — replace direct click/gameplay calls
1. Cmd.MoveOrder {unitId, worldPos, queueMode}
2. Cmd.SetTarget {attackerId, targetId?}
3. Cmd.CastSpell {casterId, spellId, targetId?, worldPos?}
4. Cmd.CancelCast {casterId}
5. Cmd.InventorySwap {ownerId, bagA, slotA, bagB, slotB}
6. Cmd.InventorySwapBetweenOwners {ownerAId, bagA, slotA, ownerBId, bagB, slotB}
7. Cmd.LootOpen {playerId, lootDropId}
8. Cmd.LootTake {playerId, lootDropId, slot, amount}
9. Cmd.LootClose {playerId, lootDropId}
10. Cmd.OpenGossip {playerId, npcId}
11. Cmd.SelectGossipOption {playerId, npcId, optionId}
12. Cmd.OpenShop {playerId, npcId}
13. Cmd.BuyFromShop {playerId, npcId, itemId, amount}
14. Cmd.ToggleCharacterWindow {open}
15. Cmd.ToggleInspectWindow {targetGuildMemberId?, open}

### 8.4 Simulation → UI events (replace HUDManager.* calls found in the scan)
1. Player.InventoryAssigned {ownerId, inventoryMeta}
2. Player.InventorySlotChanged {ownerId, bag, slot, itemSnapshot?}
3. Player.EquipmentSlotChanged {ownerId, slot, itemSnapshot?}
4. Player.GoldChanged {ownerId, gold}
5. Player.SpellbarChanged {ownerId, slots[]}
6. Player.SpellbookChanged {ownerId, spells[]}
7. Player.SpellbookSpellAdded {ownerId, spellId}
8. Player.HealthChanged {entityId, current, max}
9. Player.ResourceChanged {entityId, resourceType, current, max}
10. Player.ExperienceChanged {entityId, current, needed, level}
11. Player.LevelChanged {entityId, newLevel}
12. Player.StatsChanged {entityId, statSnapshot}
13. Target.Changed {ownerId, newTargetId?} (replaces plateBoxHandler.SetNewTarget usage)
14. Nameplate.Added {entityId, nameplateData}
15. Nameplate.Removed {entityId}
16. Party.MemberAdded {memberData}
17. Party.MemberRemoved {memberId}
18. Party.MemberListUpdated {members[]}
19. Party.ControlChanged {memberId, controlState}
20. Guild.MemberListUpdated {members[]}
21. Guild.InviteStatusUpdated {memberIds[], statuses[]}
22. Loot.Opened {playerId, lootDropId, items[]}
23. Loot.SlotChanged {lootDropId, slot, itemSnapshot?}
24. Loot.SlotRemoved {lootDropId, slot}
25. Loot.Closed {lootDropId}
26. Dialogue.GossipOpened {npcId, text, options[]}
27. Shop.Opened {npcId, itemsForSale[]}
28. Shop.Closed {npcId}

### 8.5 Simulation → Main render events (to stop drawing from live sim objects)
1. Render.EntitySpawned {entityId, renderInitData}
2. Render.EntityDespawned {entityId}
3. Render.TransformUpdated {entityId, pos, facing, z}
4. Render.AnimationChanged {entityId, animState}
5. Render.TintChanged {entityId, tint}
6. Render.TileChunkUpdated {chunkId, tileIdsOrFlags} (if tiles can change; otherwise snapshot on load)
7. Render.MinimapChunkDataUpdated {chunkId, cpuColorMap} (main builds Texture/RenderTarget)
8. Render.TransparencyMapRequest {originWorldPos} (main computes & caches, replacing TileManager.GetTransparent)

### 8.6 Simulation → Main VFX requests (replace GPU work in gameplay)
1. Vfx.SpawnRequest {vfxId, worldPos, params}
2. FloatingText.SpawnRequest {text, color, worldPos, velocity?, durationMs}

## 9. File-by-file “touch list” (what changes where)
1. Input/InputManager.cs:
   1. Split into:
      1. Main-thread poller (Keyboard.GetState/Mouse.GetState).
      2. Publisher of Input.* messages.
   2. Remove direct calls to StateManager/HUDManager.
2. GameObjects/Entities/Players/Player.cs:
   1. Remove KeyboardWalk reading KeyBindManager from sim.
   2. Handle movement via commands (e.g., Cmd.MoveIntent / Cmd.MoveOrder).
   3. Replace HUDManager calls in ctor/load with Player.* events.
3. UI/HUD/Managers/HUDManager.cs:
   1. Replace static constructor with Init().
   2. All public “game-facing” methods become event handlers on the UI thread.
4. Items/Inventory.cs and GameObjects/Unit/Equipment.cs:
   1. Replace HUDManager.RefreshInventorySlot / RefreshCharacterWindow* with Inventory/Equipment events.
5. GameObjects/Entities/EntityStats.cs and EntityAttack.cs:
   1. Replace plate/target updates with Target.Changed + Stats/Health events.
6. GameObjects/Doodads/Chest.cs and loot flow:
   1. Replace HUDManager.Loot and HUDManager.GetLootItem/ReduceLootItem with Loot.* events + Cmd.LootTake.
7. GameObjects/Entities/SpellCast.cs:
   1. Replace HUDManager.ChannelSpell/UpdateChannelSpell/CancelChannel/FinishChannel with Cast.* events.
8. Tiles/TileManager.cs and Tiles/Chunk.cs:
   1. Split sim responsibilities (collision/pathing/chunk streaming data) from render responsibilities (minimap render targets, transparency textures).
   2. Move GetTransparent and Chunk.MinimapDraw GPU work into render-owned caches.
9. GameObjects/FloatingTexts/FloatingText.cs:
   1. Remove RenderTarget2D/SpriteBatch creation from constructor.
   2. Replace with FloatingText.SpawnRequest and render-owned text rendering (or a pooled atlas approach later).
10. Managers/EffectManager.cs and Textures/TextureManager.cs:
   1. Replace static constructors with Init() called on main thread.
   2. Ensure they are never first-touched by sim/UI thread.

## 10. Performance and correctness checkpoints (must be added)
1. Add counters/telemetry:
   1. Sim tick duration (ms), fixed-step drift.
   2. UI dispatch time and draw-list build time.
   3. Queue depths for each mailbox.
   4. Dropped/coalesced messages counts (if you add coalescing).
2. Add assertions:
   1. GPU calls only on main thread.
   2. HUD/UI calls only on UI thread.
   3. Sim state mutation only on sim thread.
3. Add a kill switch:
   1. Run sim on main thread (single-thread mode) for debugging and bisecting regressions.

## 11. Known current hot allocations (fix alongside threading)
1. Managers/States/Game.Draw currently allocates Vector2[] every frame for light positions.
2. Party.GetPositions allocates a WorldSpace[] every access.
3. TileManager.Update allocates a new Chunk[,] each frame.
4. Party command operations call commands.ToArray() in UI-facing calls.
5. Required change:
   1. Replace per-frame arrays with cached buffers or pooled arrays.
   2. Prefer “write into caller-provided buffer” APIs for frequent access paths.

