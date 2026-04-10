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
            MailboxManager.Ui.Subscribe<ChatMessagePosted>(OnChatMessagePosted);
            MailboxManager.Ui.Subscribe<ChatFiltersChanged>(OnChatFiltersChanged);
            MailboxManager.Ui.Subscribe<ChatCleared>(OnChatCleared);
        }

        static void RegisterLootSubscriptions()
        {
            MailboxManager.Ui.Subscribe<LootOpened>(HandleLootOpened);
            MailboxManager.Ui.Subscribe<LootSlotChanged>(OnLootSlotChanged);
            MailboxManager.Ui.Subscribe<LootSlotRemoved>(OnLootSlotRemoved);
            MailboxManager.Ui.Subscribe<LootClosed>(OnLootClosed);
        }

        static void RegisterInventorySubscriptions()
        {
            MailboxManager.Ui.Subscribe<InventoryAssigned>(OnInventoryAssigned);
            MailboxManager.Ui.Subscribe<InventorySlotChanged>(OnInventorySlotChanged);
            MailboxManager.Ui.Subscribe<GoldChanged>(OnGoldChanged);
            MailboxManager.Ui.Subscribe<DescriptorBoxSet>(OnDescriptorBoxSet);
            MailboxManager.Ui.Subscribe<SpellDescriptorBoxSet>(OnSpellDescriptorBoxSet);
            MailboxManager.Ui.Subscribe<DescriptorBoxClear>(OnDescriptorBoxClear);
        }

        static void RegisterSpellSubscriptions()
        {
            MailboxManager.Ui.Subscribe<CastChannelStarted>(OnCastChannelStarted);
            MailboxManager.Ui.Subscribe<CastChannelProgress>(OnCastChannelProgress);
            MailboxManager.Ui.Subscribe<CastChannelCancelled>(OnCastChannelCancelled);
            MailboxManager.Ui.Subscribe<CastChannelFinished>(OnCastChannelFinished);
            MailboxManager.Ui.Subscribe<SpellbookRefreshed>(OnSpellbookRefreshed);
            MailboxManager.Ui.Subscribe<SpellbarLoaded>(OnSpellbarLoaded);
        }

        static void RegisterWindowSubscriptions()
        {
            MailboxManager.Ui.Subscribe<EquipmentSlotChanged>(OnEquipmentSlotChanged);
            MailboxManager.Ui.Subscribe<EquipmentSlotsRefreshed>(OnEquipmentSlotsRefreshed);
            MailboxManager.Ui.Subscribe<StatsRefreshed>(OnStatsRefreshed);
            MailboxManager.Ui.Subscribe<ExperienceRefreshed>(OnExperienceRefreshed);
            MailboxManager.Ui.Subscribe<CharacterWindowSet>(OnCharacterWindowSet);
            MailboxManager.Ui.Subscribe<GuildInviteStatusUpdated>(OnGuildInviteStatusUpdated);
            MailboxManager.Ui.Subscribe<GuildMembersSet>(OnGuildMembersSet);
            MailboxManager.Ui.Subscribe<GuildMemberAdded>(OnGuildMemberAdded);
            MailboxManager.Ui.Subscribe<GossipOpened>(OnGossipOpened);
            MailboxManager.Ui.Subscribe<GossipClosed>(OnGossipClosed);
            MailboxManager.Ui.Subscribe<ShopOpened>(OnShopOpened);
            MailboxManager.Ui.Subscribe<ShopClosed>(OnShopClosed);
            MailboxManager.Ui.Subscribe<SpellTrainingOpened>(OnSpellTrainingOpened);
            MailboxManager.Ui.Subscribe<SpellTrainingClosed>(OnSpellTrainingClosed);
            MailboxManager.Ui.Subscribe<InspectWindowToggled>(OnInspectWindowToggled);
            MailboxManager.Ui.Subscribe<LogicWindowOpened>(OnLogicWindowOpened);
            MailboxManager.Ui.Subscribe<LogicWindowSnapshotSet>(OnLogicWindowSnapshotSet);
            MailboxManager.Ui.Subscribe<CharacterWindowToggled>(OnCharacterWindowToggled);
            MailboxManager.Ui.Subscribe<TalentWindowToggled>(OnTalentWindowToggled);
            MailboxManager.Ui.Subscribe<TalentWindowSet>(OnTalentWindowSet);
        }

        static void RegisterPlateAndPartySubscriptions()
        {
            MailboxManager.Ui.Subscribe<PlayerPlateSet>(OnPlayerPlateSet);
            MailboxManager.Ui.Subscribe<TargetChanged>(OnTargetChanged);
            MailboxManager.Ui.Subscribe<PlateRefreshRequested>(OnPlateRefreshRequested);
            MailboxManager.Ui.Subscribe<NamePlateAdded>(OnNamePlateAdded);
            MailboxManager.Ui.Subscribe<NamePlateRemoved>(OnNamePlateRemoved);
            MailboxManager.Ui.Subscribe<PlateLayerCleared>(OnPlateLayerCleared);
            MailboxManager.Ui.Subscribe<BuffAdded>(OnBuffAdded);
            MailboxManager.Ui.Subscribe<BuffRemoved>(OnBuffRemoved);
            MailboxManager.Ui.Subscribe<PartyControlCleared>(OnPartyControlCleared);
            MailboxManager.Ui.Subscribe<PartyWalkerAdded>(OnPartyWalkerAdded);
            MailboxManager.Ui.Subscribe<PartyWalkerRemoved>(OnPartyWalkerRemoved);
            MailboxManager.Ui.Subscribe<PartyMemberAdded>(OnPartyMemberAdded);
            MailboxManager.Ui.Subscribe<PartyMemberRemoved>(OnPartyMemberRemoved);
            MailboxManager.Ui.Subscribe<PartyCleared>(OnPartyCleared);
        }

        static void RegisterHeldDragSubscriptions()
        {
            MailboxManager.Ui.Subscribe<HeldItemStart>(OnHeldItemStart);
            MailboxManager.Ui.Subscribe<HeldItemEnd>(OnHeldItemEnd);
            MailboxManager.Ui.Subscribe<HeldSpellStart>(OnHeldSpellStart);
            MailboxManager.Ui.Subscribe<HeldSpellEnd>(OnHeldSpellEnd);
        }

        static void RegisterHudMoveSubscriptions()
        {
            MailboxManager.Ui.Subscribe<HudMovableChanged>(OnHudMovableChanged);
            MailboxManager.Ui.Subscribe<HudSizeChangeRequested>(OnHudSizeChangeRequested);
            MailboxManager.Ui.Subscribe<HudSizeChangerSet>(OnHudSizeChangerSet);
        }

        static void RegisterDialogueSubscriptions()
        {
            MailboxManager.Ui.Subscribe<DialogueOpened>(AddDialogueBox);
            MailboxManager.Ui.Subscribe<DialogueClosed>(OnDialogueClosed);
        }

        static void RegisterSaveIndicatorSubscriptions()
        {
            MailboxManager.Ui.Subscribe<SaveDataStarted>(OnSaveDataStarted);
            MailboxManager.Ui.Subscribe<SaveDataFinished>(OnSaveDataFinished);
        }

        static void RegisterMiscSubscriptions()
        {
            MailboxManager.Ui.Subscribe<PlayerUiSnapshot>(OnPlayerUiSnapshot);
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

        static void OnSpellDescriptorBoxSet(SpellDescriptorBoxSet e)
        {
            if (e.Position.HasValue)
            {
                SetDescriptorBox(e.Snapshot, e.Position.Value.ToRelativeScreenPosition());
                return;
            }

            SetDescriptorBox(e.Snapshot, UiMouseStateCache.Relative);
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

        static void OnSpellTrainingOpened(SpellTrainingOpened e)
        {
            windowHandler.OpenSpellTrainingWindow(e.Entries, e.TrainerName);
            InvalidateUi();
        }

        static void OnSpellTrainingClosed(SpellTrainingClosed _)
        {
            windowHandler.CloseSpellTrainingWindow();
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
            MailboxManager.PublishSimCommand(new LogicWindowSnapshotRequested(e.MemberRenderId));
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

        static void OnTalentWindowToggled(TalentWindowToggled e)
        {
            windowHandler.ToggleTalentWindow(e.Member);
            InvalidateUi();
        }

        static void OnTalentWindowSet(TalentWindowSet e)
        {
            windowHandler.SetTalentWindow(e.Snapshot);
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

        static void OnPlateLayerCleared(PlateLayerCleared _)
        {
            namePlateHandler.Clear();
            plateBoxHandler.Clear();
            InvalidateUi();
            InvalidatePlates();
        }

        static void OnBuffAdded(BuffAdded e)
        {
            plateBoxHandler.AddBuff(e.Buff, e.OwnerRenderId);
            InvalidateUi();
            InvalidatePlates();
        }

        static void OnBuffRemoved(BuffRemoved e)
        {
            plateBoxHandler.RemoveBuff(e.BuffId, e.OwnerRenderId);
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
