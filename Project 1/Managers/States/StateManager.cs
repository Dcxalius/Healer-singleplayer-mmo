using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Project_1.GameObjects;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Entities.GuildMembers;
using Project_1.GameObjects.Entities.Players;
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
            Mailboxes.Main.Subscribe<ShopPurchaseRequested>(HandleShopPurchaseRequested);
            Mailboxes.Main.Subscribe<SpellCastRequested>(HandleSpellCastRequested);
            Mailboxes.Main.Subscribe<TargetRequested>(HandleTargetRequested);
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
        }

        public static void Update()
        {
            ThreadAffinity.AssertSimThread();
            currentState.Update();
        }

        public static void PopUp(DialogueBox aDialogueBox) => currentState.PopUp(aDialogueBox);
        public static void RemovePopUp(DialogueBox aDialogueBox) => currentState.RemovePopUp(aDialogueBox);

        public static void SetState(States aState)
        {
            ThreadAffinity.AssertSimThread();
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

        public static void RequestStateChange(States aState)
        {
            if (!SimThread.IsRunning || ThreadAffinity.IsSimThread)
            {
                SetState(aState);
                return;
            }
            Mailboxes.Main.Publish(new StateChangeRequested(aState));
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


        public static bool Click(ClickEvent aClick)
        {
            ThreadAffinity.AssertSimThread();
            return currentState.Click(aClick);
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
