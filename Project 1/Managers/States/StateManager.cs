using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.GameObjects.Spells;
using Project_1.Managers;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.UI.UIElements.Boxes;

namespace Project_1.Managers.States
{

    internal static partial class StateManager
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
        const float GroundTargetGraceRangeRatio = 0.10f;
        static Spell groundTargetPendingSpell;
        static readonly GfxPath groundTargetCircleIndicatorPath = new GfxPath(GfxType.UI, "AoECircle");
        static readonly GfxPath groundTargetRectangleIndicatorPath = new GfxPath(GfxType.UI, "WhiteBackground");
        static readonly GfxPath groundTargetInvalidIndicatorPath = new GfxPath(GfxType.UI, "AoEOutOfRange");
        static volatile GroundTargetPreviewSnapshot groundTargetPreviewSnapshot = GroundTargetPreviewSnapshot.Inactive;
        static readonly List<GroundSpellVisualSnapshot> activeGroundSpellVisuals = new List<GroundSpellVisualSnapshot>();
        static volatile GroundSpellVisualSnapshot[] groundSpellVisualSnapshots = Array.Empty<GroundSpellVisualSnapshot>();
        static readonly ChatCommandSpec[] chatCommandSpecs =
        {
            new ChatCommandSpec("help", "/help", "Shows available chat commands.", ChatCommandAccess.System),
            new ChatCommandSpec("clear", "/clear", "Clears the chat panel.", ChatCommandAccess.System),
            new ChatCommandSpec("where", "/where <friendly name>", "Prints world position for a friendly.", ChatCommandAccess.System),
            new ChatCommandSpec("chunklevels", "/chunklevels", "Prints chunk average levels (10x10 near player, or all generated if under 100 chunks).", ChatCommandAccess.System),
            new ChatCommandSpec("tp", "/tp <friendly name> <x> <y>", "Teleports a friendly to world coordinates.", ChatCommandAccess.Debug),
            new ChatCommandSpec("createitem", "/createitem <friendly name> <item id> <count>", "Creates item(s) and gives them to a friendly with inventory.", ChatCommandAccess.Debug)
        };
        const int ChunkLevelsWindowSize = 10;

        public static void Init()
        {
            ThreadAffinity.AssertMainThread();
            if (initialized) return;
            initialized = true;

            finalBatch = GraphicsManager.CreateSpriteBatch();
            CreateStates();
            RegisterStateAndSaveSubscriptions();
            RegisterWorldInteractionSubscriptions();
            RegisterInventorySubscriptions();
            RegisterChatSubscriptions();
            RegisterUiAndInputSubscriptions();
        }

        static void CreateStates()
        {
            startScreen = new StartScreen();
            game = new Game();
            moveHUD = new MoveHUD();
            pauseMenu = new PauseMenu();
            optionMenu = new OptionMenu();
            loadingMenu = new LoadingMenu();
            newGame = new NewGame();

            currentState = startScreen;
            currentStateEnum = States.StartScreen;
        }

        static void RegisterStateAndSaveSubscriptions()
        {
            SubscribeSimCommand<StateChangeRequested>(e => SetState(e.State.ToStateManagerState()));
            SubscribeSimCommand<ResetToMainMenuRequested>(_ => HandleResetToMainMenuRequested());
            SubscribeSimCommand<CreateNewPlayerRequested>(HandleCreateNewPlayerRequested);
            SubscribeSimCommand<SaveDataRequested>(_ => HandleSaveDataRequested());
            SubscribeSimCommand<LoadSaveRequested>(HandleLoadSaveRequested);
            SubscribeSimCommand<ContinueLastSaveRequested>(_ => HandleContinueLastSaveRequested());
            SubscribeSimCommand<NewGameRequested>(_ => HandleNewGameRequested());
            SubscribeSimCommand<SaveLoadParsed>(HandleSaveLoadParsed);
            SubscribeSimCommand<LogicWindowSnapshotRequested>(HandleLogicWindowSnapshotRequested);
            SubscribeSimCommand<SpellCastRequested>(HandleSpellCastRequested);
            SubscribeSimCommand<ShopPurchaseRequested>(HandleShopPurchaseRequested);
        }

        static void RegisterWorldInteractionSubscriptions()
        {
            SubscribeSimCommand<TargetRequested>(HandleTargetRequested);
            SubscribeSimCommand<PartyMemberInviteRequested>(HandlePartyMemberInviteRequested);
            SubscribeSimCommand<PartyMemberKickRequested>(HandlePartyMemberKickRequested);
            SubscribeSimCommand<WorldClickRequested>(HandleWorldClickRequested);
            SubscribeSimCommand<WorldReleaseRequested>(HandleWorldReleaseRequested);
            SubscribeSimCommand<WorldScrollRequested>(HandleWorldScrollRequested);
            SubscribeSimCommand<PlayerMovementRequested>(HandlePlayerMovementRequested);
            SubscribeSimCommand<MoveOrderRequested>(HandleMoveOrderRequested);
            SubscribeSimCommand<PartyTargetOrderRequested>(HandlePartyTargetOrderRequested);
            SubscribeSimCommand<TargetClearedRequested>(_ => HandleTargetClearedRequested());
            SubscribeSimCommand<PartyCommandRequested>(HandlePartyCommandRequested);
            SubscribeSimCommand<InteractRequested>(HandleInteractRequested);
        }

        static void RegisterInventorySubscriptions()
        {
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
        }

        static void RegisterChatSubscriptions()
        {
            SubscribeSimCommand<ChatCommandRequested>(HandleChatCommandRequested);
            SubscribeSimCommand<ChatSayRequested>(HandleChatSayRequested);
        }

        static void RegisterUiAndInputSubscriptions()
        {
            Mailboxes.Ui.Subscribe<StateChanged>(HandleUiStateChanged);
            Mailboxes.Ui.Subscribe<HudRescaleRequested>(e => UiRescale(e.WindowSize));
            SubscribeSimMailbox<KeyboardSnapshot>(e => KeyboardStateCache.Update(e));
            SubscribeSimMailbox<KeyBindSnapshot>(e => KeyBindStateCache.Update(e));
            SubscribeSimMailbox<MouseSnapshot>(e => MouseStateCache.Update(e));
            SubscribeSimCommand<EscapeRequested>(_ => HandleEscapePressed());
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
            UpdateGroundTargetPreview();
            UpdateGroundSpellVisuals();
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
            if (aState != States.Game)
            {
                CancelGroundTargeting();
            }

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

    }
}
