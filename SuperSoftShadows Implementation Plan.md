# SuperSoftShadows Implementation Plan

Date: 2026-03-06

## Locked Decisions
1. Light sources: party lights plus additional nearby emitters.
2. Combine strategy: per-light combine pass (not single aggregated draw).
3. Radius source: interface-driven in tiles.
4. Default radius: 3 tiles.
5. Player radius: 6 tiles.
6. Guild member radius: 5 tiles.
7. `LightPenetration`: start at half a tile (`Tile.Size.X * 0.5f` world units).

## Performance Guardrails
1. Core cost is `activeLights * segmentsPerLight`.
2. Keep active lights capped near player:
3. `MaxActiveLights = 16` (5 party + up to 11 nearby emitters).
4. Culling radius for non-party emitters: start at 40 tiles from player.
5. Segment cap per light after cull/merge: `MaxSegmentsPerLight = 512` hard cap.
6. Merge collinear tile edges before mesh build (major win).
7. If over budget, drop lowest-priority non-party emitters first.

## Step Plan
1. Add `ILightEmitter` interface with `float LightRadiusTiles { get; }`.
2. Implement `ILightEmitter` on player and guild member classes.
3. Add a light collection snapshot for render (`position + radiusTiles + priority/type`).
4. Build tile occluder boundary segments from solid-vs-transparent tile edges.
5. Merge contiguous collinear edges into longer segments.
6. Add shadow geometry snapshot payload for segments.
7. Build `ShadowVertex` + index buffers from each segment list.
8. Add per-light shadow mask render pass using `SuperSoftShadows` (`u_matrix`, `u_light`, `LightPenetration`).
9. Add per-light combine into final shadow mask RT.
10. Composite mask over world render before UI/plates.
11. Add profiling counters: active lights, segments/light, total shadow draw calls, shadow pass ms.
12. Add fallbacks: hard caps + graceful degradation when caps exceeded.

## Validation Checklist
1. Segment orientation consistent (no inverted forward shadows).
2. Matrix/space consistency (`segments`, `u_light`, `u_matrix` all in same space).
3. Radius units validated (tiles -> world units conversion).
4. 5-party-light scene stable.
5. 5-party + nearby emitters scene stable within cap.
6. No per-frame allocations in hot path after warmup.
7. Visual blend acceptable at corners and thin walls.

## Implementation Progress
- [x] Step 1: Add `ILightEmitter` interface.
- [x] Step 2: Implement emitter radii on player and guild member.
- [x] Step 3: Build render light snapshot (`position + radiusTiles`) with caps and range culling.
- [x] Step 4: Build tile occluder boundary segments from solid-vs-open edges.
- [x] Step 5: Merge contiguous collinear edges.
- [x] Step 6: Build shadow geometry snapshot payload per light.
- [x] Step 7: Build `ShadowVertex` + indexed segment quads.
- [x] Step 8: Render per-light shadow mask pass with `SuperSoftShadows`.
- [x] Step 9: Per-light combine into final shadow mask RT.
- [x] Step 10: Composite mask over world render before UI/plates.
- [x] Step 11: Add profiling counters for shadow pass.
- [x] Step 12: Add fallback degradation order under heavy load.
