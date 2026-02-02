# Overhaul Progress

Last updated: 2026-02-02

## In Progress
- Thread-affinity assert sweep (remaining managers/utilities).

## Finished — waiting on other implementation
- Render snapshots for entities/world objects/spawners/projectiles/corpses/doodads + VisualEffect snapshots.
- UI/input escape routing via UI-first event, with sim state change on Escape.
- UI click/scroll/release routing via UiInputBridge (waiting on full UI draw-list pipeline).

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
