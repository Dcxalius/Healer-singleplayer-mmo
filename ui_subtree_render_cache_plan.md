# UI Subtree Render Cache Plan

## Goal

Move UI rendering from the current immediate recursive child draw model to a parent-owned subtree cache model where:

- each parent keeps the most recent rendered image for each child subtree
- children only regenerate their image when they are marked dirty
- parents composite cached child images instead of forcing every child to redraw every frame
- if a parent throws during redraw, it keeps its previous image and retries on the next draw instead of poisoning the whole tree
- each element can generate a fresh image of itself and hand that image to its parent on request

## Current State

### What exists today

- `State` and `GameState` already cache full-screen render targets and only redraw those targets when dirty:
  - `Project 1/System/Managers/States/State.cs`
  - `Project 1/System/Managers/States/GameState.cs`
- `HUDManager` already caches draw lists and invalidates them through coarse `InvalidateUi` / `InvalidatePlates` signals:
  - `Project 1/UI/HUD/Managers/HUDManager.cs`
- `UIElement.Draw` still recursively draws its own children directly every time the parent draws:
  - `Project 1/UI/UIElements/UIElement.Rendering.cs`
- `UIElement` tracks broad interaction changes through `InteractionVersion`, but it does not track subtree render dirtiness at the element level:
  - `Project 1/UI/UIElements/UIElement.cs`
  - `Project 1/UI/UIElements/UIElement.Layout.cs`
  - `Project 1/UI/UIElements/UIElement.Tree.cs`

### Why this does not satisfy the target behavior

- child output is not retained as an image owned by the parent
- redraw cost is still recursive even when only one small child changed
- failures during parent redraw do not preserve a prior good subtree image
- there is no explicit "render me to an image and return it" contract between child and parent
- invalidation is mostly state-wide or HUD-wide instead of subtree-local

### Existing precedent worth reusing

- `TileRenderCache` already follows a "prepare data elsewhere, create/update GPU resource on the main thread, reuse until invalidated" pattern:
  - `Project 1/World/Tiles/Rendering/TileRenderCache.cs`
- `GraphicsManager.CreateRenderTarget` already gives us the primitive we need for subtree targets:
  - `Project 1/System/Managers/Graphics/GraphicsManager.Resources.cs`

## Target Model

### Core idea

Every `UIElement` becomes a cacheable render node.

Each node owns:

- a render target containing its latest composited image
- a dirty flag for its own local visuals
- a dirty flag indicating one or more child images changed
- a generation counter or version for change tracking
- a last-known-good render target that remains valid if a redraw attempt fails

Parents stop recursively drawing live children into the frame. Instead they:

1. ask each child for its current image
2. trigger child redraw only if the child is dirty
3. composite the returned child images into the parent render target
4. hand the parent render target upward as that parent’s current image

### Rendering contract

Add a render contract on `UIElement` conceptually shaped like this:

- `MarkRenderDirty()`
- `MarkSubtreeDirty()`
- `bool NeedsRedraw`
- `Texture2D GetRenderedImage()`
- `bool TryRefreshRenderedImage()`
- `void DrawSelf(SpriteBatch batch)` or equivalent local-only draw step
- `void CompositeChildren(SpriteBatch batch)` driven by cached child images rather than direct child draw calls

The important split is:

- `DrawSelf` renders only the element’s own visuals
- `GetRenderedImage` returns the cached subtree image
- parent composition uses child images, not recursive child draw calls

## Dirtying Rules

### Local dirtiness

Mark an element locally dirty when its own appearance changes, for example:

- `Visible`
- `Color`
- `gfx`
- text value
- size
- position within parent
- scroll state
- hover/pressed state if it affects visuals

### Child dirtiness

When a child becomes dirty or produces a newer image:

- the child marks itself dirty
- the parent marks `childImageDirty`
- that propagation bubbles upward until the root cached surface knows some subtree below it changed

### Event-driven invalidation

Prefer explicit dirty calls over broad global interaction checks.

Use this progression:

1. keep the existing `InteractionVersion` and coarse HUD/state invalidation while introducing subtree caches
2. add targeted `MarkRenderDirty` calls to the most common UI mutation points
3. reduce dependence on whole-state redraw invalidation once subtree dirtiness is trustworthy

## Failure Behavior

### Parent redraw failure

If a parent fails while generating a new image:

- keep the previous cached image alive
- do not clear the last good render target
- leave the parent marked dirty
- retry regeneration on the next request for that image
- log the failure with the parent type and exception

This directly matches the desired behavior: "if a parent throws an exception it should just try to redraw again."

### Child redraw failure

If a child fails while refreshing:

- parent should keep using the child’s last good image if one exists
- child stays dirty
- parent still completes composition using the last known child image when possible
- if no valid child image exists yet, parent can either skip the child for that frame or draw a fallback placeholder

### Main rule

Rendering failures should degrade image freshness, not collapse the frame.

## Implementation Shape

### Phase 1: Introduce subtree cache fields on `UIElement`

Touch:

- `Project 1/UI/UIElements/UIElement.cs`
- `Project 1/UI/UIElements/UIElement.Rendering.cs`
- likely one or more new `UIElement.*.cs` partials for render-cache responsibilities

Add fields roughly like:

- `RenderTarget2D cachedRenderTarget`
- `RenderTarget2D lastGoodRenderTarget`
- `bool localRenderDirty`
- `bool childRenderDirty`
- `bool subtreeRenderDirty`
- `bool renderInProgress`
- `int renderVersion`
- `int lastPresentedVersion`

Also add helpers to dispose/recreate targets on rescale or size change.

### Phase 2: Separate self rendering from child rendering

Refactor `UIElement.Draw` so it is no longer the recursive truth source for normal composition.

Split into:

- frame presentation path: draw the already cached subtree image to the screen
- cache generation path: render self + cached child images into the node’s render target

Recommended direction:

- keep `Draw(SpriteBatch)` as the public entry point for screen presentation
- add internal render-cache methods that generate the image backing that draw

### Phase 3: Add parent/child image handoff

Add a child image request flow:

- parent calls `child.GetRenderedImage()`
- child internally calls `TryRefreshRenderedImage()` if dirty
- child returns its latest valid texture

This satisfies "generate a new image of itself, and hand it off to their parent when asked for."

### Phase 4: Bubble dirty state upward

Update all UI mutation points so they dirty the correct subtree:

- `Visible` setter
- `Move`
- `Resize`
- text setters in label-like controls
- texture swaps
- selection/hover/pressed state transitions
- child add/remove in `UIElement.Tree.cs`

This is likely the single highest-value part of the change after the cache surfaces exist.

### Phase 5: Convert composition to cached-child composition

Update `UIElement.Rendering.cs` so the parent regeneration step:

1. clears the parent render target
2. draws the parent’s own background/local visuals
3. requests each child image
4. draws those child images into the parent target using each child’s bounds

At this point, direct recursive `child.Draw(...)` during cache generation should be removed or reduced to a transitional fallback.

### Phase 6: Root integration

After subtree caching works, top-level systems should present root images rather than re-walking the whole tree:

- `UiElementDrawList`
- `UiDrawList`
- `PlateDrawList`
- `HudMoveDrawList`
- `GameState.UIDraw`
- state-specific menu draw lists such as:
  - `StartScreen`
  - `PauseMenu`
  - `OptionMenu`
  - `LoadingMenu`
  - `NewGame`
  - `MoveHUD`

The root lists can still exist, but each root element in the list should mostly be drawing its cached image.

## Recommended API Additions

### `UIElement`

- `internal void MarkRenderDirty()`
- `internal void MarkChildRenderDirty()`
- `internal void MarkSubtreeDirty()`
- `internal Texture2D GetRenderedImage()`
- `internal bool TryRefreshRenderedImage()`
- `internal virtual void DrawLocal(SpriteBatch batch)`
- `internal virtual void DrawChildrenToCache(SpriteBatch batch)`
- `internal virtual void OnRenderTargetInvalidated()`

### Child management

In `UIElement.Tree.cs`, child mutations should:

- mark the parent subtree dirty
- invalidate cached child layout
- ensure removed children dispose their cached render targets if ownership is local

### Rescale / layout

In `UIElement.Layout.cs`, `Move`, `Resize`, `Rescale`, and visibility changes should mark the node dirty and bubble that dirty state upward.

## Exception Strategy

### `TryRefreshRenderedImage`

The refresh method should use this pattern:

1. if not dirty, return true immediately
2. if render target missing or wrong size, recreate it
3. attempt to redraw into the current target
4. if successful:
   - swap/promote to last good
   - clear dirty flags
   - increment version
5. if an exception occurs:
   - log it
   - preserve last good target
   - keep dirty flags set
   - return false

That gives retry-on-next-request behavior without wiping out good visuals.

## Threading Notes

### Important constraint

GPU resource creation and render target drawing are main-thread-only in this repo.

That means:

- dirty propagation can happen on UI/main ownership paths
- actual render target refresh must stay on the main thread
- no child should attempt to create or mutate GPU resources from the sim thread

The existing split in `GameState`, `HUDManager`, and `GraphicsManager` should remain intact.

## Rollout Strategy

### Step 1

Implement subtree cache plumbing in `UIElement` without changing all controls yet.

### Step 2

Convert a small, low-risk subtree first:

- `SaveStatusIndicator`
- `ChatPanel`
- a simple menu box

These are good first candidates because they are visually self-contained and easy to verify.

### Step 3

Convert broader HUD containers:

- `InventoryBox`
- `LootBox`
- `DescriptorBox`
- dialogue windows

### Step 4

Convert state menu roots and top-level draw list presentation.

### Step 5

Reduce or remove the coarse "redraw the whole UI target because interaction changed" fallback once subtree dirtiness has proven stable.

## Verification Checklist

- changing one small child does not force unrelated siblings to rerender
- parent receives a child image rather than recursively drawing live child state
- resizing recreates subtree targets cleanly
- hidden elements do not regenerate unnecessarily
- exceptions during parent redraw preserve the previous image
- a failed redraw retries on the next request
- state/HUD root render targets still composite correctly on top of the game world
- scissor behavior still matches existing child clipping expectations
- no GPU resources leak when elements are removed or resized

## Risks

- render target memory usage can grow quickly for large UI trees
- scissor and clipping behavior may change subtly when moving from direct child draw to texture composition
- text clarity can degrade if subtree images are cached at the wrong size or rescaled repeatedly
- frequent tiny invalidations may still cause many redraws if dirty bubbling is too broad
- input/hit-testing remains live against element bounds, so cached visuals must stay synchronized with layout changes

## Recommendation

Do this in two layers:

1. introduce per-element cached subtree images and dirty bubbling first
2. only after that, simplify the higher-level state redraw rules

That keeps the architectural change localized and lets the current state-level caching remain a safety net while the subtree system matures.
