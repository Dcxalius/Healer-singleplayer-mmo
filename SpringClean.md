# Spring Clean Plan

## Start Here
Start with `Managers/States`, specifically [StateManager.cs](/mnt/c/users/cassandra/source/repos/project%201/Project%201/Managers/States/StateManager.cs).

Why this is the best first cleanup target:
- It is the largest obvious god-file at about 1,900 lines.
- It mixes state transitions, sim subscriptions, UI routing, chat command parsing, ground-target spell preview/rendering, logic-tree snapshot building, and gameplay request handlers.
- `Tiles` is busy, but it is already partially decomposed into smaller files. `StateManager` is not.

Concrete split plan for [StateManager.cs](/mnt/c/users/cassandra/source/repos/project%201/Project%201/Managers/States/StateManager.cs):
- Keep `StateManager.cs` for only:
  - state registry
  - state switching
  - draw/update entry points
  - high-level init
- Move UI dispatch into `StateManager.Ui.cs`:
  - `UiClick`
  - `UiRelease`
  - `UiScroll`
  - `UiEscapePressed`
  - `UiUpdate`
  - `UiOnLeave`
  - `UiOnEnter`
  - `UiRescale`
- Move ground-target logic into `StateManager.GroundTargeting.cs`:
  - `HandleSpellCastRequested`
  - `BeginGroundTargeting`
  - `CancelGroundTargeting`
  - `UpdateGroundTargetPreview`
  - `UpdateGroundSpellVisuals`
  - `ResolveGroundTargetPlacement`
  - `DrawGroundSpellEffects`
  - `DrawGroundTargetPreview`
- Move chat into `StateManager.ChatCommands.cs`:
  - command specs
  - tokenizer
  - `/help`, `/where`, `/chunklevels`, `/tp`, `/createitem`
- Move logic window snapshot code into `StateManager.LogicTree.cs`
- Move gameplay request handlers into domain files:
  - `StateManager.InventoryRequests.cs`
  - `StateManager.PartyRequests.cs`
  - `StateManager.WorldInputRequests.cs`

Bulky functions worth splitting immediately:
- `Init()` in [StateManager.cs#L98](/mnt/c/users/cassandra/source/repos/project%201/Project%201/Managers/States/StateManager.cs#L98)
  - split into `CreateStates()`, `SubscribeStateEvents()`, `SubscribeGameplayEvents()`, `SubscribeInputSnapshots()`
- `ApplyStateChange()` in [StateManager.cs#L210](/mnt/c/users/cassandra/source/repos/project%201/Project%201/Managers/States/StateManager.cs#L210)
  - replace switch-heavy wiring with a state lookup table
- `UiUpdate()` in [StateManager.cs#L416](/mnt/c/users/cassandra/source/repos/project%201/Project%201/Managers/States/StateManager.cs#L416)
  - pull per-state routing out
- `HandleChatCommandRequested()` in [StateManager.cs#L797](/mnt/c/users/cassandra/source/repos/project%201/Project%201/Managers/States/StateManager.cs#L797)
  - replace switch dispatch with a command dictionary

## Folder Pass
### Managers
- Split [DebugManager.cs](/mnt/c/users/cassandra/source/repos/project%201/Project%201/Managers/DebugManager.cs) next. It should likely become:
  - `DebugLogging`
  - `DebugDraw`
  - `DebugFlags`
  - `DebugCommands`
- Keep `WorkerPool`, `RenderSnapshotManager`, and save/shadow managers separate unless they start crossing domains.

#### Managers Deep Dive
- [StateManager.cs](/mnt/c/users/cassandra/source/repos/project%201/Project%201/Managers/States/StateManager.cs) currently owns too many event subscriptions that are not state concerns.
- `StateManager` should keep only:
  - `StateChangeRequested`
  - `ResetToMainMenuRequested`
  - `CreateNewPlayerRequested`
  - `LoadSaveRequested`
  - `ContinueLastSaveRequested`
  - `NewGameRequested`
  - `SaveLoadParsed`
  - `EscapeRequested`
  - UI snapshot/input cache subscriptions that are truly state-scoped
- Move inventory-related request handling out of `StateManager` entirely:
  - `InventorySwapItemsRequested`
  - `InventorySwapEquipmentRequested`
  - `InventoryEquipBagRequested`
  - `InventoryUnequipBagRequested`
  - `InventorySwapBagsRequested`
  - `InventorySwapBagSlotsRequested`
  - `LootItemRequested`
  - `InventoryEquipRequested`
  - `InventoryConsumeRequested`
  - `EquipmentSwapRequested`
  - `EquipmentMoveToInventoryRequested`
- Those should be handled by a small sim-thread command router closer to the domain, for example:
  - `Items/Inventory/InventoryCommandRouter.cs`
  - or `GameObjects/Entities/Friendlies/Players/PlayerInventoryCommands.cs`
- Move party-related commands out of `StateManager`:
  - `PartyMemberInviteRequested`
  - `PartyMemberKickRequested`
  - `MoveOrderRequested`
  - `PartyTargetOrderRequested`
  - `PartyCommandRequested`
- These belong near player/party orchestration, not state orchestration. Good homes would be:
  - `GameObjects/Entities/Friendlies/Players/PartyCommandRouter.cs`
  - or `ObjectManager.PartyCommands.cs`
- Move world interaction routing out of `StateManager`:
  - `WorldClickRequested`
  - `WorldReleaseRequested`
  - `WorldScrollRequested`
  - `InteractRequested`
  - `TargetRequested`
  - `TargetClearedRequested`
- The state layer only needs to decide whether the game world is allowed to consume input. The actual click interpretation should live in a world interaction controller, for example:
  - `Managers/States/GameWorldInputRouter.cs`
  - or `GameObjects/WorldInteractionRouter.cs`
- `SpellCastRequested` and ground-target preview are borderline:
  - state gating for "only in game" can stay near `StateManager`
  - spell execution and targeting rules should move toward combat/spell code
- [DebugManager.cs](/mnt/c/users/cassandra/source/repos/project%201/Project%201/Managers/DebugManager.cs) should be split by function, not just line count:
  - diagnostics file/session handling
  - overlay text/stat rendering
  - debug mode flags
  - transient debug shapes
- [GraphicsManager.cs](/mnt/c/users/cassandra/source/repos/project%201/Project%201/Managers/GraphicsManager.cs) is doing three jobs:
  - graphics device factory
  - window/fullscreen management
  - scissor stack management
- Split that into:
  - `GraphicsDeviceService`
  - `WindowManager`
  - `ScissorStack`
- [SaveManager.cs](/mnt/c/users/cassandra/source/repos/project%201/Project%201/Managers/Saves/SaveManager.cs) is mostly cohesive, but should be split into:
  - save index/discovery
  - async load orchestration
  - payload application
  - settings path helpers
- [RenderSnapshotManager.cs](/mnt/c/users/cassandra/source/repos/project%201/Project%201/Managers/RenderSnapshotManager.cs) is small enough, but it is actually a frame composition coordinator. Rename would help:
  - `WorldSnapshotCoordinator`
  - or `RenderFrameCoordinator`

### Tiles
- Keep the current partial-decomposition direction.
- Split [TileManager.cs](/mnt/c/users/cassandra/source/repos/project%201/Project%201/Tiles/TileManager.cs) further into:
  - `TileManager.Generation.cs`
  - `TileManager.Rendering.cs`
  - `TileManager.Persistence.cs`
  - `TileManager.Doodads.cs`
  - `TileManager.Pathfinding.cs`
- Merge tiny lookup helpers:
  - [TileConversion.cs](/mnt/c/users/cassandra/source/repos/project%201/Project%201/Tiles/TileConversion.cs)
  - [TileQuery.cs](/mnt/c/users/cassandra/source/repos/project%201/Project%201/Tiles/TileQuery.cs)
  into something like `TileLookup.cs`
- [Chunk.cs](/mnt/c/users/cassandra/source/repos/project%201/Project%201/Tiles/Chunk.cs) is approaching split territory:
  - generation
  - persistence
  - rendering snapshot

### UI
- [UIElement.cs](/mnt/c/users/cassandra/source/repos/project%201/Project%201/UI/UIElements/UIElement.cs) is too large for a base control.
- Split it into:
  - layout
  - interaction/input
  - render invalidation
  - tree/parent-child management
- [CharacterWindow.cs](/mnt/c/users/cassandra/source/repos/project%201/Project%201/UI/HUD/Windows/CharacterWindow.cs) should likely split by panel/tab.
- [ChatPanel.cs](/mnt/c/users/cassandra/source/repos/project%201/Project%201/UI/HUD/Chat/ChatPanel.cs) should separate:
  - transcript/history
  - input handling
  - command submission
  - layout/rendering
- `HUD/Managers` looks over-managered. I would merge tiny handler classes if they are mostly pass-through glue.

### GameObjects
- [ObjectManager.cs](/mnt/c/users/cassandra/source/repos/project%201/Project%201/GameObjects/ObjectManager.cs) is the next strong candidate after `StateManager`.
- Split by responsibility:
  - spawn/despawn
  - lookup/query
  - update loop
  - render snapshot publication
  - player/party-specific helpers
- The `Entities` and `Unit` split is mostly reasonable, but there is naming overlap with equipment that should be cleaned up.

### Items
- [Inventory.cs](/mnt/c/users/cassandra/source/repos/project%201/Project%201/Items/Inventory.cs) is too big.
- Split into:
  - stacking/add-remove rules
  - bag slot operations
  - equipment handoff
  - query/snapshot helpers
- [EquipmentData.cs](/mnt/c/users/cassandra/source/repos/project%201/Project%201/Items/SubTypes/EquipmentData.cs) is a major hotspot.
- `Items/SubTypes` is a weak folder name. I would rename toward domain meaning, for example:
  - `Items/Data`
  - `Items/Equipment`
  - `Items/Weapons`

### Messaging
- Event files should be grouped by domain instead of mixed giant buckets.
- Good grouping would be:
  - `Events/Inventory`
  - `Events/Party`
  - `Events/Chat`
  - `Events/State`
  - `Events/Combat`
- [Mailbox.cs](/mnt/c/users/cassandra/source/repos/project%201/Project%201/Messaging/Mailbox.cs) is big enough to split by mailbox type or subscription/publication behavior if it keeps growing.

### Input
- [InputManager.cs](/mnt/c/users/cassandra/source/repos/project%201/Project%201/Input/InputManager.cs) should probably separate:
  - device polling
  - command translation
  - UI forwarding
- Text input now has its own shape; that argues for a small `Input/Text` sub-area rather than more logic in the root input manager.

### Camera, Textures, Particles
- These are not where I would spend cleanup time first.
- They look like normal-sized subsystems relative to the rest of the project.

## Suggested Order
1. `Managers/States`
2. `Managers/DebugManager`
3. `GameObjects/ObjectManager`
4. `Items/Inventory` and `Items/SubTypes/EquipmentData`
5. `UI/UIElements/UIElement`
6. `Tiles`
7. `Messaging`
