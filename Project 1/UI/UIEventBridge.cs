using System.Linq;
using Project_1.GameObjects.Entities.GuildMembers;
using Project_1.Managers;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.UI.HUD.Managers;

namespace Project_1.UI
{
    /// <summary>
    /// Temporary bridge: routes UI mailbox events into the existing HUDManager until full UI threading is in place.
    /// </summary>
    internal static class UIEventBridge
    {
        public static void Init()
        {
            ThreadAffinity.AssertMainThread();

            Mailboxes.Ui.Subscribe<InventorySlotChanged>(e => HUDManager.RefreshInventorySlot(e.BagIndex, e.SlotIndex, e.Inventory));
            Mailboxes.Ui.Subscribe<CastChannelStarted>(e => HUDManager.ChannelSpell(e.Spell));
            Mailboxes.Ui.Subscribe<CastChannelProgress>(e => HUDManager.UpdateChannelSpell(e.Progress01));
            Mailboxes.Ui.Subscribe<CastChannelCancelled>(_ => HUDManager.CancelChannel());
            Mailboxes.Ui.Subscribe<CastChannelFinished>(_ => HUDManager.FinishChannel());
            Mailboxes.Ui.Subscribe<EquipmentSlotChanged>(e =>
            {
                HUDManager.windowHandler.RefreshCharacterWindowSlot(e.Slot, e.Equipment, e.Friendly);
                HUDManager.InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<EquipmentSlotsRefreshed>(e =>
            {
                HUDManager.windowHandler.RefreshAllCharacterWindowSlots(e.Equipment, e.Friendly);
                HUDManager.InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<StatsRefreshed>(e =>
            {
                HUDManager.windowHandler.RefreshCharacterWindowStats(e.Stats, e.Friendly);
                HUDManager.InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<ExperienceRefreshed>(e =>
            {
                HUDManager.windowHandler.RefreshCharacterWindowExpBar(e.Friendly);
                HUDManager.InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<TargetChanged>(e =>
            {
                HUDManager.plateBoxHandler.SetNewTarget(e.Owner, e.Target);
                HUDManager.InvalidateUi();
                HUDManager.PlatesInvalidated?.Invoke();
            });
            Mailboxes.Ui.Subscribe<PlateRefreshRequested>(e =>
            {
                HUDManager.plateBoxHandler.RefreshPlates(e.Entity);
                HUDManager.InvalidateUi();
                HUDManager.PlatesInvalidated?.Invoke();
            });
            Mailboxes.Ui.Subscribe<NamePlateAdded>(e =>
            {
                HUDManager.namePlateHandler.AddNamePlate(e.Entity, e.Plate);
                HUDManager.InvalidateUi();
                HUDManager.PlatesInvalidated?.Invoke();
            });
            Mailboxes.Ui.Subscribe<NamePlateRemoved>(e =>
            {
                HUDManager.namePlateHandler.RemoveNamePlate(e.Entity);
                HUDManager.InvalidateUi();
                HUDManager.PlatesInvalidated?.Invoke();
            });
            Mailboxes.Ui.Subscribe<InventoryAssigned>(e =>
            {
                HUDManager.SetInventory(e.Inventory);
            });
            Mailboxes.Ui.Subscribe<SpellbookRefreshed>(e =>
            {
                HUDManager.windowHandler.RefreshSpellBook(e.Spells);
                HUDManager.InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<SpellbarLoaded>(e =>
            {
                HUDManager.LoadSpellBar(e.Spells);
            });
            Mailboxes.Ui.Subscribe<CharacterWindowSet>(e =>
            {
                HUDManager.windowHandler.SetCharacterWindow(e.Owner);
                HUDManager.InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<PlayerPlateSet>(e =>
            {
                HUDManager.plateBoxHandler.SetPlayerPlateBox(e.Owner);
                HUDManager.InvalidateUi();
                HUDManager.PlatesInvalidated?.Invoke();
            });
            Mailboxes.Ui.Subscribe<GoldChanged>(e =>
            {
                HUDManager.RefreshGold(e.Gold);
            });
            Mailboxes.Ui.Subscribe<GuildInviteStatusUpdated>(e =>
            {
                HUDManager.windowHandler.SetGuildMemberInviteStatus(e.MemberNames as System.Collections.Generic.List<string> ?? new System.Collections.Generic.List<string>(e.MemberNames),
                    e.Statuses as System.Collections.Generic.List<Project_1.UI.UIElements.Buttons.TwoStateGFXButton.State> ?? new System.Collections.Generic.List<Project_1.UI.UIElements.Buttons.TwoStateGFXButton.State>(e.Statuses));
                HUDManager.InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<BuffAdded>(e =>
            {
                HUDManager.plateBoxHandler.AddBuff(e.Buff, e.Owner);
                HUDManager.InvalidateUi();
                HUDManager.PlatesInvalidated?.Invoke();
            });
            Mailboxes.Ui.Subscribe<GossipOpened>(e =>
            {
                HUDManager.windowHandler.OpenGossipWindow(e.Start, e.Npc);
                HUDManager.InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<ShopOpened>(e =>
            {
                HUDManager.windowHandler.OpenShopWindow(e.Shop, e.Npc);
                HUDManager.InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<DescriptorBoxSet>(e =>
            {
                if (e.Position.HasValue) HUDManager.SetDescriptorBox(e.Item, e.Position.Value.ToRelativeScreenPosition());
                else HUDManager.SetDescriptorBox(e.Item);
            });
            Mailboxes.Ui.Subscribe<DescriptorBoxClear>(_ => HUDManager.SetDescriptorBox(null));
            Mailboxes.Ui.Subscribe<PartyControlCleared>(e =>
            {
                HUDManager.plateBoxHandler.RemoveWalkerFromControl(e.Members.ToArray());
                HUDManager.InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<PartyWalkerAdded>(e =>
            {
                HUDManager.plateBoxHandler.AddGuildMemberToControl(e.Member);
                HUDManager.InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<PartyWalkerRemoved>(e =>
            {
                HUDManager.plateBoxHandler.RemoveWalkerFromControl(new GuildMember[] { e.Member });
                HUDManager.InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<PartyMemberAdded>(e =>
            {
                HUDManager.plateBoxHandler.AddGuildMemberToParty(e.Member);
                HUDManager.InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<PartyMemberRemoved>(e =>
            {
                HUDManager.plateBoxHandler.RemoveGuildMemberFromParty(e.Member);
                HUDManager.InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<GuildMembersSet>(e =>
            {
                HUDManager.windowHandler.SetGuildMembers(e.Members);
                HUDManager.InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<GuildMemberAdded>(e =>
            {
                HUDManager.windowHandler.AddGuildMember(e.Member);
                HUDManager.InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<PartyCleared>(_ =>
            {
                HUDManager.plateBoxHandler.ClearParty();
                HUDManager.InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<InspectWindowToggled>(e =>
            {
                HUDManager.windowHandler.ToggleInspectWindow(e.Member);
                HUDManager.InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<CharacterWindowToggled>(_ =>
            {
                HUDManager.windowHandler.ToggleCharacterWindow();
                HUDManager.InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<DescriptorBoxClear>(_ => HUDManager.SetDescriptorBox(null));
            Mailboxes.Ui.Subscribe<HeldItemStart>(e => HUDManager.HoldItem(e.Source as Project_1.UI.HUD.Inventory.Item, e.GrabOffset));
            Mailboxes.Ui.Subscribe<HeldItemEnd>(_ => HUDManager.ReleaseItem());
            Mailboxes.Ui.Subscribe<HeldSpellStart>(e => HUDManager.HoldSpell(e.Spell, e.GrabOffset));
            Mailboxes.Ui.Subscribe<HeldSpellEnd>(_ => HUDManager.ReleaseSpell());
            Mailboxes.Ui.Subscribe<HudMovableChanged>(e =>
            {
                if (e.Enabled) HUDManager.SetHudMoveable(true);
                else HUDManager.ResetHudMoveable();
                HUDManager.InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<HudSizeChangeRequested>(e =>
            {
                if (e.Enabled) HUDManager.ChangeSizes();
                else HUDManager.DisableSizeChanges();
                HUDManager.InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<DialogueOpened>(e => HUDManager.AddDialogueBox(e.Box));
            Mailboxes.Ui.Subscribe<DialogueClosed>(e => HUDManager.RemoveDialogueBox(e.Box));
        }
    }
}
