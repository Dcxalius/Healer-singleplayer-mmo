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
using Project_1.UI;
using Project_1.UI.HUD.Managers;
using Project_1.UI.OptionMenu;
using Project_1.UI.PauseMenu;
using Project_1.UI.UIElements.Boxes;
using Project_1.Tiles;
using Project_1.GameObjects.Entities.Friendlies;
using Project_1.GameObjects.Entities.Friendlies.GuildMembers;
using Project_1.GameObjects.Entities.Friendlies.Players;
using Project_1.GameObjects.Entities.Friendlies.Npcs;

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
            Mailboxes.Main.Subscribe<StateChangeRequested>(e => SetState(e.State));
            Mailboxes.Main.Subscribe<ResetToMainMenuRequested>(_ => HandleResetToMainMenuRequested());
            Mailboxes.Main.Subscribe<CreateNewPlayerRequested>(HandleCreateNewPlayerRequested);
            Mailboxes.Main.Subscribe<SaveDataRequested>(_ => HandleSaveDataRequested());
            Mailboxes.Main.Subscribe<LoadSaveRequested>(HandleLoadSaveRequested);
            Mailboxes.Main.Subscribe<ContinueLastSaveRequested>(_ => HandleContinueLastSaveRequested());
            Mailboxes.Main.Subscribe<NewGameRequested>(_ => HandleNewGameRequested());
            Mailboxes.Main.Subscribe<ShopPurchaseRequested>(HandleShopPurchaseRequested);
            Mailboxes.Main.Subscribe<SpellCastRequested>(HandleSpellCastRequested);
            Mailboxes.Main.Subscribe<TargetRequested>(HandleTargetRequested);
            Mailboxes.Main.Subscribe<SaveLoadParsed>(HandleSaveLoadParsed);
            Mailboxes.Main.Subscribe<PartyMemberInviteRequested>(HandlePartyMemberInviteRequested);
            Mailboxes.Main.Subscribe<PartyMemberKickRequested>(HandlePartyMemberKickRequested);
            Mailboxes.Main.Subscribe<InventorySwapItemsRequested>(HandleInventorySwapItemsRequested);
            Mailboxes.Main.Subscribe<InventorySwapEquipmentRequested>(HandleInventorySwapEquipmentRequested);
            Mailboxes.Main.Subscribe<InventoryEquipBagRequested>(HandleInventoryEquipBagRequested);
            Mailboxes.Main.Subscribe<InventoryUnequipBagRequested>(HandleInventoryUnequipBagRequested);
            Mailboxes.Main.Subscribe<InventorySwapBagsRequested>(HandleInventorySwapBagsRequested);
            Mailboxes.Main.Subscribe<InventorySwapBagSlotsRequested>(HandleInventorySwapBagSlotsRequested);
            Mailboxes.Main.Subscribe<LootItemRequested>(HandleLootItemRequested);
            Mailboxes.Main.Subscribe<InventoryEquipRequested>(HandleInventoryEquipRequested);
            Mailboxes.Main.Subscribe<InventoryConsumeRequested>(HandleInventoryConsumeRequested);
            Mailboxes.Main.Subscribe<EquipmentSwapRequested>(HandleEquipmentSwapRequested);
            Mailboxes.Main.Subscribe<EquipmentMoveToInventoryRequested>(HandleEquipmentMoveToInventoryRequested);
            Mailboxes.Main.Subscribe<WorldClickRequested>(HandleWorldClickRequested);
            Mailboxes.Main.Subscribe<WorldReleaseRequested>(HandleWorldReleaseRequested);
            Mailboxes.Main.Subscribe<WorldScrollRequested>(HandleWorldScrollRequested);
            Mailboxes.Main.Subscribe<PlayerMovementRequested>(HandlePlayerMovementRequested);
            Mailboxes.Main.Subscribe<MoveOrderRequested>(HandleMoveOrderRequested);
            Mailboxes.Main.Subscribe<PartyTargetOrderRequested>(HandlePartyTargetOrderRequested);
            Mailboxes.Main.Subscribe<TargetClearedRequested>(_ => HandleTargetClearedRequested());
            Mailboxes.Main.Subscribe<PartyCommandRequested>(HandlePartyCommandRequested);
            Mailboxes.Main.Subscribe<InteractRequested>(HandleInteractRequested);
            Mailboxes.Main.Subscribe<KeyboardSnapshot>(e => KeyboardStateCache.Update(e));
            Mailboxes.Main.Subscribe<KeyBindSnapshot>(e => KeyBindStateCache.Update(e));
            Mailboxes.Main.Subscribe<MouseSnapshot>(e => MouseStateCache.Update(e));
            Mailboxes.Main.Subscribe<EscapePressed>(_ => HandleEscapePressed());
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
            Mailboxes.Main.Publish(new StateChangeRequested(aState));
        }

        static void ApplyPendingStateChange()
        {
            if (!stateChangePending) return;
            stateChangePending = false;
            ApplyStateChange(pendingState);
        }

        static void ApplyStateChange(States aState)
        {
            lock (HUDManager.UiLock)
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
                if (UiThread.IsRunning)
                {
                    Mailboxes.Ui.Publish(new StateChanged(leavingState, aState));
                }
                else
                {
                    UiOnLeave(leavingState);
                    UiOnEnter(aState);
                }
            }
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
            if (currentState == null) return;
            game.Rescale();
            optionMenu.Rescale();
            pauseMenu.Rescale();
            startScreen.Rescale();
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

        internal static bool UiClick(ClickEvent aClick)
        {
            ThreadAffinity.AssertUiThread();
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
            ThreadAffinity.AssertUiThread();
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
            ThreadAffinity.AssertUiThread();
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
            ThreadAffinity.AssertUiThread();
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

        internal static void UiUpdate()
        {
            ThreadAffinity.AssertUiThread();
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
            ThreadAffinity.AssertUiThread();
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
            ThreadAffinity.AssertUiThread();
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
            if (e.Spell == null) return;
            ObjectManager.Player?.StartCast(e.Spell);
        }

        static void HandleTargetRequested(TargetRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            player.SetTarget(e.Target ?? player);
        }

        static void HandlePartyMemberInviteRequested(PartyMemberInviteRequested e)
        {
            ThreadAffinity.AssertSimThread();
            GuildMember member = e.Member;
            if (member == null) return;
            ObjectManager.SpawnGuildMemberToParty(member, null);
        }

        static void HandlePartyMemberKickRequested(PartyMemberKickRequested e)
        {
            ThreadAffinity.AssertSimThread();
            GuildMember member = e.Member;
            if (member == null) return;
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
            if (e.Save == null) return;
            bool async = SaveManager.RequestLoadData(e.Save);
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
            if (e.ClickEvent == null) return;
            if (currentState == null || currentState.GetStateEnum != States.Game) return;
            RouteWorldClick(e.ClickEvent);
        }

        static void HandleWorldReleaseRequested(WorldReleaseRequested e)
        {
            ThreadAffinity.AssertSimThread();
            if (e.ReleaseEvent == null) return;
            Release(e.ReleaseEvent);
        }

        static void HandleWorldScrollRequested(WorldScrollRequested e)
        {
            ThreadAffinity.AssertSimThread();
            if (e.ScrollEvent == null) return;
            Scroll(e.ScrollEvent);
        }

        static void RouteWorldClick(ClickEvent clickEvent)
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
                Mailboxes.Main.Publish(new InteractRequested(corpse, clickEvent.ButtonPressed));
                return;
            }

            if (DoodadManager.TryGetDoodadAt(worldPos, out Doodad doodad))
            {
                Mailboxes.Main.Publish(new InteractRequested(doodad, clickEvent.ButtonPressed));
                return;
            }

            HandleGroundWorldClick(worldPos, clickEvent);
        }

        static void HandleEntityWorldClick(Entity entity, ClickEvent clickEvent)
        {
            bool noModifiers = clickEvent.NoModifiers();
            bool rightClick = clickEvent.ButtonPressed == InputManager.ClickType.Right;

            if (noModifiers)
            {
                Mailboxes.Main.Publish(new TargetRequested(entity));
                if (rightClick)
                {
                    Mailboxes.Main.Publish(new PartyTargetOrderRequested(entity));
                }
            }
            else if (entity is GuildMember member)
            {
                if (clickEvent.Modifier(InputManager.HoldModifier.Shift))
                {
                    Mailboxes.Main.Publish(new PartyCommandRequested(PartyCommandAction.Add, member));
                }
                else if (clickEvent.Modifier(InputManager.HoldModifier.Ctrl))
                {
                    Mailboxes.Main.Publish(new PartyCommandRequested(PartyCommandAction.NeedyAdd, member));
                }
            }

            if (entity is Npc npc)
            {
                Mailboxes.Main.Publish(new InteractRequested(npc, clickEvent.ButtonPressed));
            }
        }

        static void HandleGroundWorldClick(WorldSpace worldPos, ClickEvent clickEvent)
        {
            if (clickEvent.ButtonPressed == InputManager.ClickType.Left)
            {
                if (clickEvent.ModifiersOr(new InputManager.HoldModifier[] { InputManager.HoldModifier.Shift, InputManager.HoldModifier.Ctrl }))
                {
                    Mailboxes.Main.Publish(new PartyCommandRequested(PartyCommandAction.Clear, null));
                    return;
                }

                Mailboxes.Main.Publish(new TargetClearedRequested());
                return;
            }

            if (clickEvent.ButtonPressed == InputManager.ClickType.Right)
            {
                bool append = clickEvent.Modifier(InputManager.HoldModifier.Shift);
                Mailboxes.Main.Publish(new MoveOrderRequested(worldPos, append));
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
            if (player == null || e.Target == null) return;
            player.Party.IssueTargetOrder(e.Target);
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
                    if (e.Member != null) player.Party.AddToCommand(e.Member);
                    break;
                case PartyCommandAction.NeedyAdd:
                    if (e.Member != null) player.Party.NeedyAddToCommand(e.Member);
                    break;
            }
        }

        static void HandleInteractRequested(InteractRequested e)
        {
            ThreadAffinity.AssertSimThread();
            if (e.Target == null) return;
            switch (e.Target)
            {
                case Corpse corpse:
                    if (e.Button != InputManager.ClickType.Right) return;
                    corpse.TryOpenLoot();
                    break;
                case Chest chest:
                    chest.TryOpenLoot();
                    break;
                case Npc npc:
                    npc.TryBeginConversation();
                    break;
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
            Friendly target = e.Target ?? player;
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
            Friendly target = e.Target ?? player;
            player.Inventory.Equip(e.Index, target);
        }

        static void HandleInventoryConsumeRequested(InventoryConsumeRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            Friendly target = e.Target ?? player;
            player.Inventory.ConsumeItem(e.Index, target);
        }

        static void HandleEquipmentSwapRequested(EquipmentSwapRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            Friendly target = e.Target ?? player;

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
            Friendly target = e.Target ?? player;

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

        static void HandleUiStateChanged(StateChanged e)
        {
            UiOnLeave(e.Previous);
            UiOnEnter(e.Current);
        }
    }
}
