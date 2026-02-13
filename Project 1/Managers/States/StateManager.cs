using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Entities.Corspes;
using Project_1.GameObjects.Doodads;
using Project_1.GameObjects.Spawners;
using Project_1.Input;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.Items;
using Project_1.Items.SubTypes;
using Project_1.Particles;
using Project_1.GameObjects.Spells;
using Project_1.UI;
using Project_1.UI.HUD.Managers;
using Project_1.UI.OptionMenu;
using Project_1.UI.PauseMenu;
using Project_1.UI.UIElements.Boxes;
using Project_1.UI.UIElements;
using Project_1.Tiles;
using Project_1.GameObjects.Entities.Friendlies;
using Project_1.GameObjects.Entities.Friendlies.GuildMembers;
using Project_1.GameObjects.Entities.Friendlies.Players;
using Project_1.GameObjects.Entities.Friendlies.Npcs;
using Project_1.Managers.Saves;

namespace Project_1.Managers.States
{

    internal static class StateManager
    {
        public enum States
        {
            StartScreen,
            Game,
            MoveHUD,
            PauseMenu,
            OptionMenu,
            LoadingMenu,
            NewGame
        }

        static State currentState;
        static States currentStateEnum;

        static StartScreen startScreen;
        static Game game;
        static MoveHUD moveHUD;
        static PauseMenu pauseMenu;
        static OptionMenu optionMenu;
        static LoadingMenu loadingMenu;
        static NewGame newGame;

        static SpriteBatch finalBatch;

        public static RenderTarget2D CleanGameTarget => game.CleanGameDraw();
        public static RenderTarget2D FinalGameFrame { get => finalGameFrame; set => finalGameFrame = value; }
        static RenderTarget2D finalGameFrame;
        static bool pendingRedrawGame;
        public static Rectangle RenderTargetPosition { set => renderTargetPosition = value; }
        static Rectangle renderTargetPosition;

        public static States PreviousState => previousState;
        static States previousState;
        static bool stateChangePending;
        static States pendingState;
        static bool initialized;

        public static void Init()
        {
            ThreadAffinity.AssertMainThread();
            if (initialized) return;
            initialized = true;

            finalBatch = GraphicsManager.CreateSpriteBatch();

            startScreen = new StartScreen();
            game = new Game();
            moveHUD = new MoveHUD();
            pauseMenu = new PauseMenu();
            optionMenu = new OptionMenu();
            loadingMenu = new LoadingMenu();
            newGame = new NewGame();

            currentState = startScreen;
            currentStateEnum = States.StartScreen;

            Mailboxes.Ui.Subscribe<StateChanged>(HandleUiStateChanged);
            SubscribeSimCommand<StateChangeRequested>(e => SetState(e.State.ToStateManagerState()));
            SubscribeSimCommand<ResetToMainMenuRequested>(_ => HandleResetToMainMenuRequested());
            SubscribeSimCommand<CreateNewPlayerRequested>(HandleCreateNewPlayerRequested);
            SubscribeSimCommand<SaveDataRequested>(_ => HandleSaveDataRequested());
            SubscribeSimCommand<LoadSaveRequested>(HandleLoadSaveRequested);
            SubscribeSimCommand<ContinueLastSaveRequested>(_ => HandleContinueLastSaveRequested());
            SubscribeSimCommand<NewGameRequested>(_ => HandleNewGameRequested());
            SubscribeSimCommand<ShopPurchaseRequested>(HandleShopPurchaseRequested);
            SubscribeSimCommand<SpellCastRequested>(HandleSpellCastRequested);
            SubscribeSimCommand<TargetRequested>(HandleTargetRequested);
            SubscribeSimCommand<LogicWindowSnapshotRequested>(HandleLogicWindowSnapshotRequested);
            SubscribeSimCommand<SaveLoadParsed>(HandleSaveLoadParsed);
            SubscribeSimCommand<PartyMemberInviteRequested>(HandlePartyMemberInviteRequested);
            SubscribeSimCommand<PartyMemberKickRequested>(HandlePartyMemberKickRequested);
            SubscribeSimCommand<InventorySwapItemsRequested>(HandleInventorySwapItemsRequested);
            SubscribeSimCommand<InventorySwapEquipmentRequested>(HandleInventorySwapEquipmentRequested);
            SubscribeSimCommand<InventoryEquipBagRequested>(HandleInventoryEquipBagRequested);
            SubscribeSimCommand<InventoryUnequipBagRequested>(HandleInventoryUnequipBagRequested);
            SubscribeSimCommand<InventorySwapBagsRequested>(HandleInventorySwapBagsRequested);
            SubscribeSimCommand<InventorySwapBagSlotsRequested>(HandleInventorySwapBagSlotsRequested);
            SubscribeSimCommand<LootItemRequested>(HandleLootItemRequested);
            SubscribeSimCommand<InventoryEquipRequested>(HandleInventoryEquipRequested);
            SubscribeSimCommand<InventoryConsumeRequested>(HandleInventoryConsumeRequested);
            SubscribeSimCommand<EquipmentSwapRequested>(HandleEquipmentSwapRequested);
            SubscribeSimCommand<EquipmentMoveToInventoryRequested>(HandleEquipmentMoveToInventoryRequested);
            SubscribeSimCommand<WorldClickRequested>(HandleWorldClickRequested);
            SubscribeSimCommand<WorldReleaseRequested>(HandleWorldReleaseRequested);
            SubscribeSimCommand<WorldScrollRequested>(HandleWorldScrollRequested);
            SubscribeSimCommand<PlayerMovementRequested>(HandlePlayerMovementRequested);
            SubscribeSimCommand<MoveOrderRequested>(HandleMoveOrderRequested);
            SubscribeSimCommand<PartyTargetOrderRequested>(HandlePartyTargetOrderRequested);
            SubscribeSimCommand<TargetClearedRequested>(_ => HandleTargetClearedRequested());
            SubscribeSimCommand<PartyCommandRequested>(HandlePartyCommandRequested);
            SubscribeSimCommand<InteractRequested>(HandleInteractRequested);
            SubscribeSimMailbox<KeyboardSnapshot>(e => KeyboardStateCache.Update(e));
            SubscribeSimMailbox<KeyBindSnapshot>(e => KeyBindStateCache.Update(e));
            SubscribeSimMailbox<MouseSnapshot>(e => MouseStateCache.Update(e));
            SubscribeSimCommand<EscapeRequested>(_ => HandleEscapePressed());
            Mailboxes.Ui.Subscribe<HudRescaleRequested>(e => UiRescale(e.WindowSize));
        }

        static void SubscribeSimCommand<T>(Action<T> handler)
        {
            Mailboxes.RegisterSimCommandType<T>();
            SubscribeSimMailbox(handler);
        }

        static void SubscribeSimMailbox<T>(Action<T> handler)
        {
            Mailboxes.Sim.Subscribe(handler);
        }

        public static void Update()
        {
            ThreadAffinity.AssertSimThread();
            KeyboardStateCache.BeginFrame();
            KeyBindStateCache.BeginFrame();
            ApplyPendingStateChange();
            currentState.Update();
        }

        public static void PopUp(DialogueBox aDialogueBox) => currentState.PopUp(aDialogueBox);
        public static void RemovePopUp(DialogueBox aDialogueBox) => currentState.RemovePopUp(aDialogueBox);

        public static void SetState(States aState)
        {
            ThreadAffinity.AssertSimThread();
            pendingState = aState;
            stateChangePending = true;
        }

        public static void RequestStateChange(States aState)
        {
            if (!SimThread.IsRunning || ThreadAffinity.IsSimThread)
            {
                SetState(aState);
                return;
            }
            Mailboxes.PublishSimCommand(new StateChangeRequested(aState.ToStateKind()));
        }

        static void ApplyPendingStateChange()
        {
            if (!stateChangePending) return;
            stateChangePending = false;
            ApplyStateChange(pendingState);
        }

        static void ApplyStateChange(States aState)
        {
            States leavingState = currentStateEnum;
            currentState.OnLeave();
            previousState = leavingState;
            switch (aState)
            {
                case States.StartScreen:
                    currentState = startScreen;
                    break;
                case States.Game:
                    currentState = game;
                    break;
                case States.PauseMenu:
                    currentState = pauseMenu;
                    break;
                case States.OptionMenu:
                    currentState = optionMenu;
                    break;
                case States.MoveHUD:
                    currentState = moveHUD;
                    break;
                case States.LoadingMenu:
                    currentState = loadingMenu;
                    break;
                case States.NewGame:
                    currentState = newGame;
                    break;
                default:
                    throw new NotImplementedException();
            }
            currentStateEnum = aState;
            currentState.OnEnter();
            if (UiThread.IsRunning || SimThread.IsRunning)
            {
                Mailboxes.PublishUiEvent(new StateChanged(leavingState.ToStateKind(), aState.ToStateKind()));
                return;
            }

            UiOnLeave(leavingState);
            UiOnEnter(aState);
        }

        public static void RedrawGame()
        {
            if (!ThreadAffinity.IsMainThread)
            {
                pendingRedrawGame = true;
                return;
            }
            finalGameFrame = game.Draw();
        }


        public static bool Release(ReleaseEvent aRelease)
        {
            ThreadAffinity.AssertSimThread();
            return currentState.Release(aRelease);
        }

        public static bool Scroll(ScrollEvent aScroll)
        {
            ThreadAffinity.AssertSimThread();
            if (currentState.Scroll(aScroll)) return true;
            Camera.Camera.Scroll(aScroll);
            return true;
        }

        public static void Rescale()
        {
            ThreadAffinity.AssertMainThread();
            if (currentState == null) return;
            game.Rescale();
            moveHUD.Rescale();
            optionMenu.Rescale();
            pauseMenu.Rescale();
            startScreen.Rescale();
            loadingMenu.Rescale();
            newGame.Rescale();
        }
        public static void Draw()
        {
            ThreadAffinity.AssertMainThread();
            if (pendingRedrawGame && ThreadAffinity.IsMainThread)
            {
                pendingRedrawGame = false;
                finalGameFrame = game.Draw();
            }
            RenderTarget2D target = currentState.Draw();

            finalBatch.Begin();
            finalBatch.Draw(target, renderTargetPosition, Color.White);
            finalBatch.End();
        }
        public static States CurrentState => currentStateEnum;

        static void AssertUiOwnerThread()
        {
            if (UiThread.IsRunning)
            {
                ThreadAffinity.AssertUiThread();
                return;
            }

            ThreadAffinity.AssertMainThread();
        }

        internal static bool UiClick(ClickEvent aClick)
        {
            AssertUiOwnerThread();
            switch (currentStateEnum)
            {
                case States.StartScreen:
                    return startScreen.UiClick(aClick);
                case States.PauseMenu:
                    return pauseMenu.UiClick(aClick);
                case States.OptionMenu:
                    return optionMenu.UiClick(aClick);
                case States.LoadingMenu:
                    return loadingMenu.UiClick(aClick);
                case States.NewGame:
                    return newGame.UiClick(aClick);
                case States.MoveHUD:
                    return moveHUD.UiClick(aClick);
                default:
                    return false;
            }
        }

        internal static bool UiRelease(ReleaseEvent aRelease)
        {
            AssertUiOwnerThread();
            switch (currentStateEnum)
            {
                case States.StartScreen:
                    return startScreen.UiRelease(aRelease);
                case States.PauseMenu:
                    return pauseMenu.UiRelease(aRelease);
                case States.OptionMenu:
                    return optionMenu.UiRelease(aRelease);
                case States.LoadingMenu:
                    return loadingMenu.UiRelease(aRelease);
                case States.NewGame:
                    return newGame.UiRelease(aRelease);
                case States.MoveHUD:
                    return moveHUD.UiRelease(aRelease);
                default:
                    return false;
            }
        }

        internal static bool UiScroll(ScrollEvent aScroll)
        {
            AssertUiOwnerThread();
            switch (currentStateEnum)
            {
                case States.StartScreen:
                    return startScreen.UiScroll(aScroll);
                case States.PauseMenu:
                    return pauseMenu.UiScroll(aScroll);
                case States.OptionMenu:
                    return optionMenu.UiScroll(aScroll);
                case States.LoadingMenu:
                    return loadingMenu.UiScroll(aScroll);
                case States.NewGame:
                    return newGame.UiScroll(aScroll);
                case States.MoveHUD:
                    return moveHUD.UiScroll(aScroll);
                default:
                    return false;
            }
        }

        internal static bool UiEscapePressed()
        {
            AssertUiOwnerThread();
            switch (currentStateEnum)
            {
                case States.StartScreen:
                    return startScreen.UiEscapePressed();
                case States.PauseMenu:
                    return pauseMenu.UiEscapePressed();
                case States.OptionMenu:
                    return optionMenu.UiEscapePressed();
                case States.LoadingMenu:
                    return loadingMenu.UiEscapePressed();
                case States.NewGame:
                    return newGame.UiEscapePressed();
                case States.MoveHUD:
                    return moveHUD.UiEscapePressed();
                default:
                    return false;
            }
        }

        internal static void UiInvalidate()
        {
            AssertUiOwnerThread();
            currentState?.MarkUiDirty();
        }

        internal static void UiUpdate()
        {
            AssertUiOwnerThread();
            long interactionVersionBefore = UIElement.InteractionVersion;
            switch (currentStateEnum)
            {
                case States.StartScreen:
                    startScreen.UiUpdate();
                    break;
                case States.PauseMenu:
                    pauseMenu.UiUpdate();
                    break;
                case States.OptionMenu:
                    optionMenu.UiUpdate();
                    break;
                case States.LoadingMenu:
                    loadingMenu.UiUpdate();
                    break;
                case States.NewGame:
                    newGame.UiUpdate();
                    break;
                case States.MoveHUD:
                    moveHUD.UiUpdate();
                    break;
                default:
                    break;
            }

            if (currentStateEnum != States.Game
                && currentStateEnum != States.MoveHUD
                && UIElement.InteractionVersion != interactionVersionBefore)
            {
                currentState?.MarkUiDirty();
            }
        }

        static void HandleEscapePressed()
        {
            ThreadAffinity.AssertSimThread();
            switch (currentStateEnum)
            {
                case States.Game:
                    RequestStateChange(States.PauseMenu);
                    break;
                case States.PauseMenu:
                    pauseMenu.HandleEscapePressed();
                    break;
                default:
                    break;
            }
        }

        internal static void UiOnLeave(States state)
        {
            AssertUiOwnerThread();
            switch (state)
            {
                case States.Game:
                    game.UiOnLeave();
                    break;
                case States.MoveHUD:
                    moveHUD.UiOnLeave();
                    break;
                case States.OptionMenu:
                    optionMenu.UiOnLeave();
                    break;
                case States.LoadingMenu:
                    loadingMenu.UiOnLeave();
                    break;
                case States.NewGame:
                    newGame.UiOnLeave();
                    break;
                default:
                    break;
            }
        }

        internal static void UiOnEnter(States state)
        {
            AssertUiOwnerThread();
            switch (state)
            {
                case States.MoveHUD:
                    moveHUD.UiOnEnter();
                    break;
                case States.OptionMenu:
                    optionMenu.UiOnEnter();
                    break;
                case States.LoadingMenu:
                    loadingMenu.UiOnEnter();
                    break;
                case States.NewGame:
                    newGame.UiOnEnter();
                    break;
                default:
                    break;
            }
        }

        internal static void UiRescale(Point windowSize)
        {
            AssertUiOwnerThread();

            HUDManager.Rescale();
            HUDManager.InvalidateUi();
            HUDManager.InvalidatePlates();
            switch (currentStateEnum)
            {
                case States.StartScreen:
                    startScreen.UiRescale();
                    break;
                case States.PauseMenu:
                    pauseMenu.UiRescale();
                    break;
                case States.OptionMenu:
                    optionMenu.UiRescale();
                    break;
                case States.LoadingMenu:
                    loadingMenu.UiRescale();
                    break;
                case States.NewGame:
                    newGame.UiRescale();
                    break;
                case States.MoveHUD:
                    moveHUD.UiRescale();
                    break;
                default:
                    break;
            }
        }

        static void HandleShopPurchaseRequested(ShopPurchaseRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;

            Items.Item item = ItemFactory.CreateItem(e.ItemId, e.Count);
            if (item == null) return;
            if (item.Cost > player.Gold) return;

            player.ChangeGold(-item.Cost);
            player.Inventory.AddItem(item);
        }

        static void HandleSpellCastRequested(SpellCastRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            if (!player.SpellBook.TryGetSpell(e.SpellName, out Spell spell)) return;
            player.StartCast(spell);
        }

        static void HandleTargetRequested(TargetRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            if (!e.TargetRenderId.HasValue)
            {
                player.SetTarget(player);
                return;
            }

            if (!TryResolveEntityByRenderId(e.TargetRenderId.Value, out Entity target))
            {
                player.SetTarget(player);
                return;
            }

            player.SetTarget(target);
        }

        static void HandleLogicWindowSnapshotRequested(LogicWindowSnapshotRequested e)
        {
            ThreadAffinity.AssertSimThread();
            if (!ObjectManager.TryGetGuildMemberByRenderId(e.MemberRenderId, out GuildMember member))
            {
                Mailboxes.PublishUiEvent(new LogicWindowSnapshotSet(e.MemberRenderId, Array.Empty<LogicNodeUiSnapshot>()));
                return;
            }

            LogicNode root = member.AttackLogic?.RootNode;
            Mailboxes.PublishUiEvent(new LogicWindowSnapshotSet(e.MemberRenderId, BuildLogicNodeSnapshot(root)));
        }

        static LogicNodeUiSnapshot[] BuildLogicNodeSnapshot(LogicNode root)
        {
            if (root == null) return Array.Empty<LogicNodeUiSnapshot>();

            Dictionary<LogicNode, int> ids = new Dictionary<LogicNode, int>();
            List<LogicNodeUiSnapshot> snapshots = new List<LogicNodeUiSnapshot>();
            Queue<(LogicNode node, int parentId, int depth)> queue = new Queue<(LogicNode node, int parentId, int depth)>();
            queue.Enqueue((root, -1, 0));

            while (queue.Count > 0)
            {
                (LogicNode node, int parentId, int depth) = queue.Dequeue();
                if (node == null) continue;

                if (!ids.TryGetValue(node, out int nodeId))
                {
                    nodeId = ids.Count;
                    ids[node] = nodeId;
                    string resultName = GetLogicNodeName(node);
                    snapshots.Add(new LogicNodeUiSnapshot(nodeId, parentId, depth, resultName));

                    IReadOnlyList<LogicNode> children = node.Children;
                    for (int i = 0; i < children.Count; i++)
                    {
                        queue.Enqueue((children[i], nodeId, depth + 1));
                    }
                    continue;
                }

                // If the tree reuses node instances, keep an explicit edge for this parent.
                if (parentId >= 0)
                {
                    snapshots.Add(new LogicNodeUiSnapshot(nodeId, parentId, depth, GetLogicNodeName(node)));
                }
            }

            return snapshots.ToArray();
        }

        static string GetLogicNodeName(LogicNode node)
        {
            ILogicResult raw = node.RawResult;
            if (raw == null || raw is Continue) return "Branch";
            string name = raw.GetType().Name;
            const string suffix = "Result";
            if (name.EndsWith(suffix))
            {
                name = name.Substring(0, name.Length - suffix.Length);
            }
            return name;
        }

        static void HandlePartyMemberInviteRequested(PartyMemberInviteRequested e)
        {
            ThreadAffinity.AssertSimThread();
            if (!ObjectManager.TryGetGuildMemberByRenderId(e.MemberRenderId, out GuildMember member)) return;
            ObjectManager.SpawnGuildMemberToParty(member, null);
        }

        static void HandlePartyMemberKickRequested(PartyMemberKickRequested e)
        {
            ThreadAffinity.AssertSimThread();
            if (!ObjectManager.TryGetGuildMemberByRenderId(e.MemberRenderId, out GuildMember member)) return;
            ObjectManager.RemoveGuildMemberFromParty(member);
        }

        static void HandleResetToMainMenuRequested()
        {
            ThreadAffinity.AssertSimThread();
            SetState(States.StartScreen);
            ObjectManager.Reset();
        }

        static void HandleCreateNewPlayerRequested(CreateNewPlayerRequested e)
        {
            ThreadAffinity.AssertSimThread();
            if (string.IsNullOrWhiteSpace(e.Name) || string.IsNullOrWhiteSpace(e.ClassName)) return;
            ObjectManager.CreateNewPlayer(e.Name, e.ClassName);
            SaveManager.CreateNewSave(e.Name);
            SetState(States.Game);
        }

        static void HandleSaveDataRequested()
        {
            ThreadAffinity.AssertSimThread();
            SaveManager.SaveData();
        }

        static void HandleLoadSaveRequested(LoadSaveRequested e)
        {
            ThreadAffinity.AssertSimThread();
            if (!SaveManager.TryGetSaveByName(e.SaveName, out Save save)) return;
            bool async = SaveManager.RequestLoadData(save);
            if (async)
            {
                SetState(States.LoadingMenu);
                return;
            }
            SetState(States.Game);
            RedrawGame();
        }

        static void HandleContinueLastSaveRequested()
        {
            ThreadAffinity.AssertSimThread();
            bool async = SaveManager.RequestContinueLastSave();
            if (async)
            {
                SetState(States.LoadingMenu);
                return;
            }
            SetState(States.Game);
            RedrawGame();
        }

        static void HandleNewGameRequested()
        {
            ThreadAffinity.AssertSimThread();
            TileManager.New();
            SetState(States.NewGame);
        }

        static void HandleSaveLoadParsed(SaveLoadParsed e)
        {
            ThreadAffinity.AssertSimThread();
            if (e.Payload == null) return;
            SaveManager.ApplyLoadPayload(e.Payload);
            SetState(States.Game);
            RedrawGame();
        }

        static void HandleWorldClickRequested(WorldClickRequested e)
        {
            ThreadAffinity.AssertSimThread();
            if (currentState == null || currentState.GetStateEnum != States.Game) return;
            RouteWorldClick(e);
        }

        static void HandleWorldReleaseRequested(WorldReleaseRequested e)
        {
            ThreadAffinity.AssertSimThread();
            ReleaseEvent releaseEvent = new ReleaseEvent(null, e.RelativePos, e.Button.ToInputClickType(), e.ModifiersMask);
            Release(releaseEvent);
        }

        static void HandleWorldScrollRequested(WorldScrollRequested e)
        {
            ThreadAffinity.AssertSimThread();
            ScrollEvent.Direction direction = e.Up ? ScrollEvent.Direction.Up : ScrollEvent.Direction.Down;
            ScrollEvent scrollEvent = new ScrollEvent(e.RelativePos, e.Steps, direction, e.ModifiersMask);
            Scroll(scrollEvent);
        }

        static void RouteWorldClick(in WorldClickRequested clickEvent)
        {
            WorldSpace worldPos = WorldSpace.FromRelativeScreenSpace(clickEvent.RelativePos);

            if (ObjectManager.TryGetEntityAt(worldPos, out Entity entity))
            {
                HandleEntityWorldClick(entity, clickEvent);
                return;
            }

            if (SpawnerManager.TryGetSpawnAt(worldPos, out Entity spawn))
            {
                HandleEntityWorldClick(spawn, clickEvent);
                return;
            }

            if (CorpseManager.TryGetCorpseAt(worldPos, out Corpse corpse))
            {
                Mailboxes.PublishSimCommand(new InteractRequested(corpse.RenderId, clickEvent.Button));
                return;
            }

            if (DoodadManager.TryGetDoodadAt(worldPos, out Doodad doodad))
            {
                Mailboxes.PublishSimCommand(new InteractRequested(doodad.RenderId, clickEvent.Button));
                return;
            }

            HandleGroundWorldClick(worldPos, clickEvent);
        }

        static void HandleEntityWorldClick(Entity entity, in WorldClickRequested clickEvent)
        {
            bool noModifiers = clickEvent.NoModifiers();
            bool rightClick = clickEvent.Button == ClickKind.Right;

            if (noModifiers)
            {
                Mailboxes.PublishSimCommand(new TargetRequested(entity.RenderId));
                if (rightClick)
                {
                    Mailboxes.PublishSimCommand(new PartyTargetOrderRequested(entity.RenderId));
                }
            }
            else if (entity is GuildMember member)
            {
                if (clickEvent.Modifier(InputManager.HoldModifier.Shift))
                {
                    Mailboxes.PublishSimCommand(new PartyCommandRequested(PartyCommandAction.Add, member.RenderId));
                }
                else if (clickEvent.Modifier(InputManager.HoldModifier.Ctrl))
                {
                    Mailboxes.PublishSimCommand(new PartyCommandRequested(PartyCommandAction.NeedyAdd, member.RenderId));
                }
            }

            if (entity is Npc npc)
            {
                Mailboxes.PublishSimCommand(new InteractRequested(npc.RenderId, clickEvent.Button));
            }
        }

        static void HandleGroundWorldClick(WorldSpace worldPos, in WorldClickRequested clickEvent)
        {
            if (clickEvent.Button == ClickKind.Left)
            {
                if (clickEvent.Modifier(InputManager.HoldModifier.Shift) || clickEvent.Modifier(InputManager.HoldModifier.Ctrl))
                {
                    Mailboxes.PublishSimCommand(new PartyCommandRequested(PartyCommandAction.Clear, null));
                    return;
                }

                Mailboxes.PublishSimCommand(new TargetClearedRequested());
                return;
            }

            if (clickEvent.Button == ClickKind.Right)
            {
                bool append = clickEvent.Modifier(InputManager.HoldModifier.Shift);
                Mailboxes.PublishSimCommand(new MoveOrderRequested(worldPos, append));
            }
        }

        static void HandlePlayerMovementRequested(PlayerMovementRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            player.ApplyMoveInput(e.Left, e.Right, e.Up, e.Down);
        }

        static void HandleMoveOrderRequested(MoveOrderRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            player.Party.IssueMoveOrder(e.Destination, e.Append);
        }

        static void HandlePartyTargetOrderRequested(PartyTargetOrderRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            if (!TryResolveEntityByRenderId(e.TargetRenderId, out Entity target)) return;
            player.Party.IssueTargetOrder(target);
        }

        static void HandleTargetClearedRequested()
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            player.RemoveTarget();
        }

        static void HandlePartyCommandRequested(PartyCommandRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            switch (e.Action)
            {
                case PartyCommandAction.Clear:
                    player.Party.ClearCommand();
                    break;
                case PartyCommandAction.Add:
                    if (e.MemberRenderId.HasValue &&
                        ObjectManager.TryGetGuildMemberByRenderId(e.MemberRenderId.Value, out GuildMember addMember))
                    {
                        player.Party.AddToCommand(addMember);
                    }
                    break;
                case PartyCommandAction.NeedyAdd:
                    if (e.MemberRenderId.HasValue &&
                        ObjectManager.TryGetGuildMemberByRenderId(e.MemberRenderId.Value, out GuildMember needyMember))
                    {
                        player.Party.NeedyAddToCommand(needyMember);
                    }
                    break;
            }
        }

        static void HandleInteractRequested(InteractRequested e)
        {
            ThreadAffinity.AssertSimThread();
            if (CorpseManager.TryGetCorpseByRenderId(e.TargetRenderId, out Corpse corpse))
            {
                if (e.Button != ClickKind.Right) return;
                corpse.TryOpenLoot();
                return;
            }

            if (DoodadManager.TryGetDoodadByRenderId(e.TargetRenderId, out Doodad doodad))
            {
                if (doodad is Chest chest)
                {
                    chest.TryOpenLoot();
                }
                return;
            }

            if (!ObjectManager.TryGetEntityByRenderId(e.TargetRenderId, out Entity entity)) return;
            if (entity is Npc npc)
            {
                npc.TryBeginConversation();
            }
        }

        static void HandleInventorySwapItemsRequested(InventorySwapItemsRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            player.Inventory.SwapItems(e.From, e.To);
        }

        static void HandleInventorySwapEquipmentRequested(InventorySwapEquipmentRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            Friendly target = ResolveFriendlyTarget(e.TargetRenderId, player);
            player.Inventory.SwapEquipment(e.From, e.EquipmentSlot, target);
        }

        static void HandleInventoryEquipBagRequested(InventoryEquipBagRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            player.Inventory.EquipBag(e.From);
        }

        static void HandleInventoryUnequipBagRequested(InventoryUnequipBagRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            if (e.ToSlot.HasValue)
            {
                player.Inventory.UnequipBag(e.BagSlot, e.ToSlot.Value);
                return;
            }
            player.Inventory.UnequipBag(e.BagSlot);
        }

        static void HandleInventorySwapBagsRequested(InventorySwapBagsRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            player.Inventory.SwapBags(e.From, e.BagSlot);
        }

        static void HandleInventorySwapBagSlotsRequested(InventorySwapBagSlotsRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            player.Inventory.SwapPlacesOfBags(e.FromBagSlot, e.ToBagSlot);
        }

        static void HandleLootItemRequested(LootItemRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            if (e.ToSlot.HasValue)
            {
                player.Inventory.LootItem(e.LootSlotIndex, e.ToSlot.Value);
                return;
            }
            player.Inventory.LootItem(e.LootSlotIndex);
        }

        static void HandleInventoryEquipRequested(InventoryEquipRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            Friendly target = ResolveFriendlyTarget(e.TargetRenderId, player);
            player.Inventory.Equip(e.Index, target);
        }

        static void HandleInventoryConsumeRequested(InventoryConsumeRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            Friendly target = ResolveFriendlyTarget(e.TargetRenderId, player);
            player.Inventory.ConsumeItem(e.Index, target);
        }

        static void HandleEquipmentSwapRequested(EquipmentSwapRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            Friendly target = ResolveFriendlyTarget(e.TargetRenderId, player);

            Equipment fromEquip = target.Equipment.EquipedInSlot((GameObjects.Unit.Equipment.Slot)e.FromSlot) as Equipment;
            if (fromEquip == null) return;
            if (!GameObjects.Unit.Equipment.FitsInSlot(fromEquip.type, (GameObjects.Unit.Equipment.Slot)e.ToSlot)) return;

            Equipment toEquip = target.Equipment.EquipedInSlot((GameObjects.Unit.Equipment.Slot)e.ToSlot) as Equipment;
            if (toEquip == null)
            {
                target.EquipInParticularSlot(fromEquip, (GameObjects.Unit.Equipment.Slot)e.ToSlot);
                target.EquipInParticularSlot(null, (GameObjects.Unit.Equipment.Slot)e.FromSlot);
                return;
            }

            if (fromEquip.type != toEquip.type) return;
            if (fromEquip.type >= Equipment.Type.MainHander) return;
            if (toEquip.type >= Equipment.Type.MainHander) return;

            target.EquipInParticularSlot(fromEquip, (GameObjects.Unit.Equipment.Slot)e.ToSlot);
            target.EquipInParticularSlot(toEquip, (GameObjects.Unit.Equipment.Slot)e.FromSlot);
        }

        static void HandleEquipmentMoveToInventoryRequested(EquipmentMoveToInventoryRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            Friendly target = ResolveFriendlyTarget(e.TargetRenderId, player);

            Equipment fromEquip = target.Equipment.EquipedInSlot((GameObjects.Unit.Equipment.Slot)e.EquipmentSlot) as Equipment;
            if (fromEquip == null) return;

            Item destItem = player.Inventory.GetItemInSlot(e.InventorySlot);
            if (destItem == null)
            {
                player.Inventory.AddItem(fromEquip, e.InventorySlot);
                target.EquipInParticularSlot(null, (GameObjects.Unit.Equipment.Slot)e.EquipmentSlot);
                return;
            }

            Equipment destEquip = destItem as Equipment;
            if (destEquip == null) return;
            if (!GameObjects.Unit.Equipment.FitsInSlot(destEquip.type, (GameObjects.Unit.Equipment.Slot)e.EquipmentSlot)) return;

            player.Inventory.AssignItem(fromEquip, e.InventorySlot);
            target.EquipInParticularSlot(destEquip, (GameObjects.Unit.Equipment.Slot)e.EquipmentSlot);
        }

        static Friendly ResolveFriendlyTarget(int? targetRenderId, Player fallback)
        {
            ThreadAffinity.AssertSimThread();
            if (!targetRenderId.HasValue) return fallback;
            if (ObjectManager.TryGetFriendlyByRenderId(targetRenderId.Value, out Friendly target))
            {
                return target;
            }
            return fallback;
        }

        static bool TryResolveEntityByRenderId(int renderId, out Entity entity)
        {
            ThreadAffinity.AssertSimThread();
            if (ObjectManager.TryGetEntityByRenderId(renderId, out entity))
            {
                return true;
            }

            if (SpawnerManager.TryGetSpawnByRenderId(renderId, out entity))
            {
                return true;
            }

            entity = null;
            return false;
        }

        static void HandleUiStateChanged(StateChanged e)
        {
            AssertUiOwnerThread();
            UiOnLeave(e.Previous.ToStateManagerState());
            UiOnEnter(e.Current.ToStateManagerState());
        }
    }
}
