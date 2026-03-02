using Project_1.Camera;
using Project_1.GameObjects.Entities.Friendlies.Players;
using Project_1.Input;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.UI.UIElements;
using System;

namespace Project_1.UI.HUD.Managers
{
    internal static partial class HUDManager
    {
        static void RegisterUiSubscriptions()
        {
            RegisterChatSubscriptions();
            RegisterLootSubscriptions();
            RegisterInventorySubscriptions();
            RegisterSpellSubscriptions();
            RegisterWindowSubscriptions();
            RegisterPlateAndPartySubscriptions();
            RegisterHeldDragSubscriptions();
            RegisterHudMoveSubscriptions();
            RegisterDialogueSubscriptions();
            RegisterSaveIndicatorSubscriptions();
            RegisterMiscSubscriptions();
        }

        static void RegisterChatSubscriptions()
        {
            Mailboxes.Ui.Subscribe<ChatMessagePosted>(OnChatMessagePosted);
            Mailboxes.Ui.Subscribe<ChatFiltersChanged>(OnChatFiltersChanged);
            Mailboxes.Ui.Subscribe<ChatCleared>(OnChatCleared);
        }

        static void RegisterLootSubscriptions()
        {
            Mailboxes.Ui.Subscribe<LootOpened>(HandleLootOpened);
            Mailboxes.Ui.Subscribe<LootSlotChanged>(OnLootSlotChanged);
            Mailboxes.Ui.Subscribe<LootSlotRemoved>(OnLootSlotRemoved);
            Mailboxes.Ui.Subscribe<LootClosed>(OnLootClosed);
        }

        static void RegisterInventorySubscriptions()
        {
            Mailboxes.Ui.Subscribe<InventoryAssigned>(OnInventoryAssigned);
            Mailboxes.Ui.Subscribe<InventorySlotChanged>(OnInventorySlotChanged);
            Mailboxes.Ui.Subscribe<GoldChanged>(OnGoldChanged);
            Mailboxes.Ui.Subscribe<DescriptorBoxSet>(OnDescriptorBoxSet);
            Mailboxes.Ui.Subscribe<DescriptorBoxClear>(OnDescriptorBoxClear);
        }

        static void RegisterSpellSubscriptions()
        {
            Mailboxes.Ui.Subscribe<CastChannelStarted>(OnCastChannelStarted);
            Mailboxes.Ui.Subscribe<CastChannelProgress>(OnCastChannelProgress);
            Mailboxes.Ui.Subscribe<CastChannelCancelled>(OnCastChannelCancelled);
            Mailboxes.Ui.Subscribe<CastChannelFinished>(OnCastChannelFinished);
            Mailboxes.Ui.Subscribe<SpellbookRefreshed>(OnSpellbookRefreshed);
            Mailboxes.Ui.Subscribe<SpellbarLoaded>(OnSpellbarLoaded);
        }

        static void RegisterWindowSubscriptions()
        {
            Mailboxes.Ui.Subscribe<EquipmentSlotChanged>(OnEquipmentSlotChanged);
            Mailboxes.Ui.Subscribe<EquipmentSlotsRefreshed>(OnEquipmentSlotsRefreshed);
            Mailboxes.Ui.Subscribe<StatsRefreshed>(OnStatsRefreshed);
            Mailboxes.Ui.Subscribe<ExperienceRefreshed>(OnExperienceRefreshed);
            Mailboxes.Ui.Subscribe<CharacterWindowSet>(OnCharacterWindowSet);
            Mailboxes.Ui.Subscribe<GuildInviteStatusUpdated>(OnGuildInviteStatusUpdated);
            Mailboxes.Ui.Subscribe<GuildMembersSet>(OnGuildMembersSet);
            Mailboxes.Ui.Subscribe<GuildMemberAdded>(OnGuildMemberAdded);
            Mailboxes.Ui.Subscribe<GossipOpened>(OnGossipOpened);
            Mailboxes.Ui.Subscribe<GossipClosed>(OnGossipClosed);
            Mailboxes.Ui.Subscribe<ShopOpened>(OnShopOpened);
            Mailboxes.Ui.Subscribe<ShopClosed>(OnShopClosed);
            Mailboxes.Ui.Subscribe<InspectWindowToggled>(OnInspectWindowToggled);
            Mailboxes.Ui.Subscribe<LogicWindowOpened>(OnLogicWindowOpened);
            Mailboxes.Ui.Subscribe<LogicWindowSnapshotSet>(OnLogicWindowSnapshotSet);
            Mailboxes.Ui.Subscribe<CharacterWindowToggled>(OnCharacterWindowToggled);
        }

        static void RegisterPlateAndPartySubscriptions()
        {
            Mailboxes.Ui.Subscribe<PlayerPlateSet>(OnPlayerPlateSet);
            Mailboxes.Ui.Subscribe<TargetChanged>(OnTargetChanged);
            Mailboxes.Ui.Subscribe<PlateRefreshRequested>(OnPlateRefreshRequested);
            Mailboxes.Ui.Subscribe<NamePlateAdded>(OnNamePlateAdded);
            Mailboxes.Ui.Subscribe<NamePlateRemoved>(OnNamePlateRemoved);
            Mailboxes.Ui.Subscribe<BuffAdded>(OnBuffAdded);
            Mailboxes.Ui.Subscribe<PartyControlCleared>(OnPartyControlCleared);
            Mailboxes.Ui.Subscribe<PartyWalkerAdded>(OnPartyWalkerAdded);
            Mailboxes.Ui.Subscribe<PartyWalkerRemoved>(OnPartyWalkerRemoved);
            Mailboxes.Ui.Subscribe<PartyMemberAdded>(OnPartyMemberAdded);
            Mailboxes.Ui.Subscribe<PartyMemberRemoved>(OnPartyMemberRemoved);
            Mailboxes.Ui.Subscribe<PartyCleared>(OnPartyCleared);
        }

        static void RegisterHeldDragSubscriptions()
        {
            Mailboxes.Ui.Subscribe<HeldItemStart>(OnHeldItemStart);
            Mailboxes.Ui.Subscribe<HeldItemEnd>(OnHeldItemEnd);
            Mailboxes.Ui.Subscribe<HeldSpellStart>(OnHeldSpellStart);
            Mailboxes.Ui.Subscribe<HeldSpellEnd>(OnHeldSpellEnd);
        }

        static void RegisterHudMoveSubscriptions()
        {
            Mailboxes.Ui.Subscribe<HudMovableChanged>(OnHudMovableChanged);
            Mailboxes.Ui.Subscribe<HudSizeChangeRequested>(OnHudSizeChangeRequested);
            Mailboxes.Ui.Subscribe<HudSizeChangerSet>(OnHudSizeChangerSet);
        }

        static void RegisterDialogueSubscriptions()
        {
            Mailboxes.Ui.Subscribe<DialogueOpened>(AddDialogueBox);
            Mailboxes.Ui.Subscribe<DialogueClosed>(OnDialogueClosed);
        }

        static void RegisterSaveIndicatorSubscriptions()
        {
            Mailboxes.Ui.Subscribe<SaveDataStarted>(OnSaveDataStarted);
            Mailboxes.Ui.Subscribe<SaveDataFinished>(OnSaveDataFinished);
        }

        static void RegisterMiscSubscriptions()
        {
            Mailboxes.Ui.Subscribe<PlayerUiSnapshot>(OnPlayerUiSnapshot);
        }

        static void OnChatMessagePosted(ChatMessagePosted e)
        {
            chatPanel.AddMessage(e.ChatMessage);
            InvalidateUi();
        }

        static void OnChatFiltersChanged(ChatFiltersChanged _)
        {
            chatPanel.RefreshFilters();
            InvalidateUi();
        }

        static void OnChatCleared(ChatCleared _)
        {
            chatPanel.ClearMessages();
            InvalidateUi();
        }

        static void OnLootSlotChanged(LootSlotChanged e)
        {
            lootBox.RefreshSlot(e.Slot, e.ItemSnapshot);
            InvalidateUi();
        }

        static void OnLootSlotRemoved(LootSlotRemoved e)
        {
            lootBox.RefreshSlot(e.Slot, ItemUiSnapshot.Empty);
            InvalidateUi();
        }

        static void OnLootClosed(LootClosed e)
        {
            lootBox.CloseIfContext(e.ContextId);
            InvalidateUi();
        }

        static void OnInventoryAssigned(InventoryAssigned e)
        {
            SetInventory(e.Snapshot);
        }

        static void OnInventorySlotChanged(InventorySlotChanged e)
        {
            RefreshInventorySlot(e.BagIndex, e.SlotIndex, e.Snapshot);
        }

        static void OnGoldChanged(GoldChanged e)
        {
            RefreshGold(e.Gold);
        }

        static void OnDescriptorBoxSet(DescriptorBoxSet e)
        {
            if (e.Position.HasValue)
            {
                SetDescriptorBox(e.Snapshot, e.Position.Value.ToRelativeScreenPosition());
                return;
            }

            SetDescriptorBox(e.Snapshot, UiMouseStateCache.Relative);
        }

        static void OnDescriptorBoxClear(DescriptorBoxClear _)
        {
            ClearDescriptorBox();
        }

        static void OnCastChannelStarted(CastChannelStarted e)
        {
            ChannelSpell(e.SpellGfxPath, e.DurationMs);
        }

        static void OnCastChannelProgress(CastChannelProgress e)
        {
            UpdateChannelSpell(e.Progress01);
        }

        static void OnCastChannelCancelled(CastChannelCancelled _)
        {
            CancelChannel();
        }

        static void OnCastChannelFinished(CastChannelFinished _)
        {
            FinishChannel();
        }

        static void OnSpellbookRefreshed(SpellbookRefreshed e)
        {
            windowHandler.RefreshSpellBook(e.SpellNames ?? Array.Empty<string>());
            InvalidateUi();
        }

        static void OnSpellbarLoaded(SpellbarLoaded e)
        {
            LoadSpellBar(e.SpellNames ?? Array.Empty<string>());
        }

        static void OnEquipmentSlotChanged(EquipmentSlotChanged e)
        {
            windowHandler.RefreshCharacterWindowSlot(e.OwnerRenderId, e.OwnerRelation, e.Slot, e.ItemSnapshot);
            InvalidateUi();
        }

        static void OnEquipmentSlotsRefreshed(EquipmentSlotsRefreshed e)
        {
            windowHandler.RefreshAllCharacterWindowSlots(e.OwnerRenderId, e.OwnerRelation, e.ItemSnapshots);
            InvalidateUi();
        }

        static void OnStatsRefreshed(StatsRefreshed e)
        {
            windowHandler.RefreshCharacterWindowStats(e.OwnerRenderId, e.OwnerRelation, e.PrimaryStats, e.SecondaryStats);
            InvalidateUi();
        }

        static void OnExperienceRefreshed(ExperienceRefreshed e)
        {
            windowHandler.RefreshCharacterWindowExpBar(e.OwnerRenderId, e.OwnerRelation, e.CurrentLevel, e.CurrentExperience);
            if (e.OwnerRelation == RelationToPlayerKind.Self)
            {
                HandleSelfExperienceForChat(e.CurrentLevel, e.CurrentExperience);
            }
            InvalidateUi();
        }

        static void OnCharacterWindowSet(CharacterWindowSet e)
        {
            windowHandler.SetCharacterWindow(e.Snapshot);
            InvalidateUi();
        }

        static void OnGuildInviteStatusUpdated(GuildInviteStatusUpdated e)
        {
            windowHandler.SetGuildMemberInviteStatus(e.MemberNames, e.Statuses);
            InvalidateUi();
        }

        static void OnGuildMembersSet(GuildMembersSet e)
        {
            windowHandler.SetGuildMembers(e.Members);
            InvalidateUi();
        }

        static void OnGuildMemberAdded(GuildMemberAdded e)
        {
            HandleGuildMemberAdded(e.Member);
        }

        static void OnGossipOpened(GossipOpened e)
        {
            windowHandler.OpenGossipWindow(e.Data);
            InvalidateUi();
        }

        static void OnGossipClosed(GossipClosed _)
        {
            windowHandler.CloseGossipWindow();
            InvalidateUi();
        }

        static void OnShopOpened(ShopOpened e)
        {
            windowHandler.OpenShopWindow(e.ItemIds, e.ShopkeeperName);
            InvalidateUi();
        }

        static void OnShopClosed(ShopClosed _)
        {
            windowHandler.CloseShopWindow();
            InvalidateUi();
        }

        static void OnInspectWindowToggled(InspectWindowToggled e)
        {
            windowHandler.ToggleInspectWindow(e.Member);
            InvalidateUi();
        }

        static void OnLogicWindowOpened(LogicWindowOpened e)
        {
            windowHandler.OpenLogicWindow(e.MemberRenderId);
            Mailboxes.PublishSimCommand(new LogicWindowSnapshotRequested(e.MemberRenderId));
            InvalidateUi();
        }

        static void OnLogicWindowSnapshotSet(LogicWindowSnapshotSet e)
        {
            windowHandler.SetLogicWindowSnapshot(e.MemberRenderId, e.Nodes);
            InvalidateUi();
        }

        static void OnCharacterWindowToggled(CharacterWindowToggled _)
        {
            windowHandler.ToggleCharacterWindow();
            InvalidateUi();
        }

        static void OnPlayerPlateSet(PlayerPlateSet e)
        {
            plateBoxHandler.SetPlayerPlateBox(e.Snapshot);
            InvalidateUi();
            InvalidatePlates();
        }

        static void OnTargetChanged(TargetChanged e)
        {
            plateBoxHandler.SetNewTarget(e.OwnerRelation, e.TargetSnapshot);
            InvalidateUi();
            InvalidatePlates();
        }

        static void OnPlateRefreshRequested(PlateRefreshRequested e)
        {
            plateBoxHandler.RefreshPlates(e.Snapshot);
            namePlateHandler.RefreshNamePlate(e.Snapshot);
            InvalidateUi();
            InvalidatePlates();
        }

        static void OnNamePlateAdded(NamePlateAdded e)
        {
            namePlateHandler.AddNamePlate(e.Snapshot);
            InvalidateUi();
            InvalidatePlates();
        }

        static void OnNamePlateRemoved(NamePlateRemoved e)
        {
            namePlateHandler.RemoveNamePlate(e.RenderId);
            InvalidateUi();
            InvalidatePlates();
        }

        static void OnBuffAdded(BuffAdded e)
        {
            plateBoxHandler.AddBuff(e.Buff, e.OwnerRenderId);
            InvalidateUi();
            InvalidatePlates();
        }

        static void OnPartyControlCleared(PartyControlCleared e)
        {
            for (int i = 0; i < e.MemberCount; i++)
            {
                plateBoxHandler.RemoveWalkerFromControl(e.GetMemberRenderId(i));
            }
            InvalidateUi();
        }

        static void OnPartyWalkerAdded(PartyWalkerAdded e)
        {
            plateBoxHandler.AddGuildMemberToControl(e.MemberRenderId);
            InvalidateUi();
        }

        static void OnPartyWalkerRemoved(PartyWalkerRemoved e)
        {
            plateBoxHandler.RemoveWalkerFromControl(e.MemberRenderId);
            InvalidateUi();
        }

        static void OnPartyMemberAdded(PartyMemberAdded e)
        {
            HandlePartyMemberAdded(e.Member);
        }

        static void OnPartyMemberRemoved(PartyMemberRemoved e)
        {
            HandlePartyMemberRemoved(e.MemberRenderId);
        }

        static void OnPartyCleared(PartyCleared _)
        {
            plateBoxHandler.ClearParty();
            InvalidateUi();
        }

        static void OnHeldItemStart(HeldItemStart e)
        {
            if (!UIElement.TryResolve(e.SourceUiElementId, out UIElement element)) return;
            HoldItem(element as Inventory.Item, e.GrabOffset);
        }

        static void OnHeldItemEnd(HeldItemEnd _)
        {
            ReleaseItem();
        }

        static void OnHeldSpellStart(HeldSpellStart e)
        {
            if (string.IsNullOrWhiteSpace(e.SpellName)) return;
            if (!UiPlayerStateCache.TryGetSpellSnapshot(e.SpellName, out SpellUiSnapshot snapshot)) return;
            HoldSpell(snapshot.GfxPath, e.GrabOffset);
        }

        static void OnHeldSpellEnd(HeldSpellEnd _)
        {
            ReleaseSpell();
        }

        static void OnHudMovableChanged(HudMovableChanged e)
        {
            if (e.Enabled) SetHudMoveable(true);
            else SetHudMoveable(false);
            InvalidateUi();
        }

        static void OnHudSizeChangeRequested(HudSizeChangeRequested e)
        {
            if (e.Enabled) ChangeSizes();
            else DisableSizeChanges();
            InvalidateUi();
        }

        static void OnHudSizeChangerSet(HudSizeChangerSet e)
        {
            if (!UIElement.TryResolve(e.UiElementId, out UIElement element)) return;
            SetSizeChanger(element);
        }

        static void OnDialogueClosed(DialogueClosed e)
        {
            RemoveDialogueBox(e.DialogueBoxId);
        }

        static void OnSaveDataStarted(SaveDataStarted _)
        {
            saveStatusIndicator.NotifySaveStarted();
            InvalidateUi();
        }

        static void OnSaveDataFinished(SaveDataFinished _)
        {
            saveStatusIndicator.NotifySaveFinished();
            InvalidateUi();
        }

        static void OnPlayerUiSnapshot(PlayerUiSnapshot e)
        {
            UiPlayerStateCache.Update(e);
        }
    }
}
