# Rendering System Review

## Overview
- **Intent:** this rework is laying groundwork for a more general rendering stack built around reusable cameras, model primitives, and shader-driven model rendering instead of only sprite and tile draw code.
- **Current reality:** the live game still renders through the existing snapshot-based 2D pipeline, with shadows composited by the current shadow renderer. The new stack is mostly exposed through a debug-only preview path.
- **Why adjacent files changed:** block world data, chunk generation, resolution handling, and option UI changes are supporting pieces for that preview path and future migration, even when they are not replacing the old renderer yet.

## New Rendering Foundations
The new `Project 1/System/Rendering` camera layer is trying to create a reusable rendering-side abstraction instead of relying only on the older gameplay camera types. `Camera.cs` provides viewport-sized view/projection handling plus clip-space projection and unprojection helpers. `Camera2D.cs` adds orthographic world-to-screen and screen-to-world behavior, while `Camera3D.cs` adds look-at, perspective projection, and ray construction for 3D scenes.

`Project 1/World/Camera/WorldSpace3D.cs` introduces a matching 3D coordinate type so the renderer and model code can talk in full 3D positions without overloading the existing 2D `WorldSpace`.

The new `Project 1/System/Models/*` files are the start of a model layer. They define common model data such as meshes, vertices, joints, materials, and separate `Model2D` and `Model3D` concepts. That reads like an attempt to give rendering its own data model instead of treating every visual as a textured sprite.

`Project 1/Content/Effects/ModelRender.fx` is the first dedicated shader in this stack. It takes `World`, `View`, and `Projection` matrices plus a diffuse texture and tint, then renders textured geometry. `Project 1/Content/Content.mgcb` now compiles that effect through MGCB, which makes the model path part of the game content pipeline rather than a standalone experiment.

## Actual Runtime Integration
The main runtime hook is in `Project 1/System/Managers/States/Game.cs`. Both `CleanGameDraw()` and `Draw()` call `BlockModelPreviewRenderer.UpdatePreviewTarget()` during draw preparation, so the preview data is refreshed as part of the normal frame flow.

The actual branch point is `DrawWorld()`. If `DebugManager.Mode(DebugMode.ModelPreview)` is enabled, the game draws the model preview path. Otherwise it falls back to `DrawList()`, which still uses `RenderSnapshotManager.DrawGameSnapshots()` plus particles and floating text. That means the rework is scaffolded into the game loop, but world and entity rendering have not been migrated over.

`Project 1/System/Managers/Debug/DebugManager.cs` is the gatekeeper for this path. `DebugMode.ModelPreview` is a dedicated flag, and in `DEBUG` builds it is currently turned on by default inside `DebugManager.Init()`. In practice, that makes the new renderer behave more like an active debug visualization mode than a second production renderer.

## Bridge Between New 3D Data And Existing 2D World
`Project 1/World/Tiles/Core/Block.cs` is one of the clearest signs of the new direction. Chunks now carry `Block[,,]` data with material, collision, vision, elevation, and depth concepts, which is much closer to a block-volume world than the older flat tile model.

`Project 1/World/Tiles/Core/BlockTileBridge.cs` is the compatibility layer. It can turn old tile IDs into blocks, but more importantly it can resolve the top visible block in a column and convert that back into the `TileData` needed by the current runtime. In other words, the new world representation is being shaped underneath a still-2D game.

`Project 1/World/Tiles/Core/Chunk.cs` ties that together. The chunk stores block volume data, lazily builds surface `Tile[,]` data from it, and then still builds `ChunkRenderSnapshot` objects from those surface tiles for the existing renderer. The temporary surface bridge comment in `Chunk.Tile()` is a good summary of the intent: support a future 3D or block-aware world model without breaking the current 2D systems yet.

## Preview And Tooling Support
`Project 1/System/Models/BaseModels/BlockModel.cs` is the first concrete 3D renderable in this stack. It builds cube faces, feeds `ModelRender` with world/view/projection data, and draws textured geometry directly through the graphics device. It looks more like a renderer prototype than a complete scene system, but it proves the path works.

`Project 1/System/Models/BaseModels/BlockModelPreviewRenderer.cs` is the part that makes the prototype visible in-game. It renders a small rotating block model into a render target using `Camera3D`, then uses a `Camera2D` view to stamp that preview texture across chunk and block positions in a top-down debug view. The important nuance is that this is not yet rendering actual world meshes. It is visualizing block-space layout with a reused preview model.

The graphics and UI changes support that workflow. `Project 1/System/Managers/Graphics/GraphicsManager.cs` now exposes display mode information and supported fullscreen sizes. `GraphicsManager.Windowing.cs` normalizes requested window or fullscreen resolutions and applies safer fallback behavior. `Project 1/UI/OptionMenu/ScreenSizeSelect.cs` was rewritten to use numeric inputs for windowed mode, a live fullscreen resolution list, and a borderless information state. Together those changes make the new camera and preview behavior less fragile across window modes and monitor sizes.

## Current Status
- The existing snapshot and shadow pipeline is still the real production renderer. `RenderSnapshotManager` still builds and draws world snapshots, and `SuperSoftShadowRenderer` still composites the live shadow pass.
- The new rendering stack currently reads as **foundation + debug visualization path**, not a full replacement renderer.
- The likely direction is a gradual migration away from purely tile and sprite-oriented rendering toward a block and model-aware pipeline, with compatibility layers kept in place until more runtime systems move over.
