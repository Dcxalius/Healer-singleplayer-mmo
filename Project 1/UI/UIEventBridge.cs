using System.Linq;
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
            Mailboxes.Ui.Subscribe<EquipmentSlotChanged>(e => HUDManager.windowHandler.RefreshCharacterWindowSlot(e.Slot, e.Equipment, e.Friendly));
            Mailboxes.Ui.Subscribe<EquipmentSlotsRefreshed>(e => HUDManager.windowHandler.RefreshAllCharacterWindowSlots(e.Equipment, e.Friendly));
            Mailboxes.Ui.Subscribe<StatsRefreshed>(e => HUDManager.windowHandler.RefreshCharacterWindowStats(e.Stats, e.Friendly));
            Mailboxes.Ui.Subscribe<ExperienceRefreshed>(e => HUDManager.windowHandler.RefreshCharacterWindowExpBar(e.Friendly));
            Mailboxes.Ui.Subscribe<TargetChanged>(e => HUDManager.plateBoxHandler.SetNewTarget(e.Owner, e.Target));
            Mailboxes.Ui.Subscribe<PlateRefreshRequested>(e => HUDManager.plateBoxHandler.RefreshPlates(e.Entity));
            Mailboxes.Ui.Subscribe<NamePlateAdded>(e => HUDManager.namePlateHandler.AddNamePlate(e.Entity, e.Plate));
            Mailboxes.Ui.Subscribe<NamePlateRemoved>(e => HUDManager.namePlateHandler.RemoveNamePlate(e.Entity));
            Mailboxes.Ui.Subscribe<InventoryAssigned>(e => HUDManager.SetInventory(e.Inventory));
            Mailboxes.Ui.Subscribe<SpellbookRefreshed>(e => HUDManager.windowHandler.RefreshSpellBook(e.Spells));
            Mailboxes.Ui.Subscribe<SpellbarLoaded>(e => HUDManager.LoadSpellBar(e.Spells));
            Mailboxes.Ui.Subscribe<CharacterWindowSet>(e => HUDManager.windowHandler.SetCharacterWindow(e.Owner));
            Mailboxes.Ui.Subscribe<PlayerPlateSet>(e => HUDManager.plateBoxHandler.SetPlayerPlateBox(e.Owner));
            Mailboxes.Ui.Subscribe<GoldChanged>(e => HUDManager.RefreshGold(e.Gold));
            Mailboxes.Ui.Subscribe<GuildInviteStatusUpdated>(e => HUDManager.windowHandler.SetGuildMemberInviteStatus(e.MemberNames, e.Statuses));
            Mailboxes.Ui.Subscribe<BuffAdded>(e => HUDManager.plateBoxHandler.AddBuff(e.Buff, e.Owner));
            Mailboxes.Ui.Subscribe<GossipOpened>(e => HUDManager.windowHandler.OpenGossipWindow(e.Start, e.Npc));
            Mailboxes.Ui.Subscribe<ShopOpened>(e => HUDManager.windowHandler.OpenShopWindow(e.Shop, e.Npc));
            Mailboxes.Ui.Subscribe<DescriptorBoxSet>(e =>
            {
                if (e.Position.HasValue) HUDManager.SetDescriptorBox(e.Item, e.Position.Value.ToRelativeScreenPosition());
                else HUDManager.SetDescriptorBox(e.Item);
            });
            Mailboxes.Ui.Subscribe<DescriptorBoxClear>(_ => HUDManager.SetDescriptorBox(null));
            Mailboxes.Ui.Subscribe<PartyControlCleared>(e => HUDManager.plateBoxHandler.RemoveWalkerFromControl(e.Members.ToArray()));
            Mailboxes.Ui.Subscribe<PartyWalkerAdded>(e => HUDManager.plateBoxHandler.AddGuildMemberToControl(e.Member));
            Mailboxes.Ui.Subscribe<PartyWalkerRemoved>(e => HUDManager.plateBoxHandler.RemoveWalkerFromControl(new Project_1.GameObjects.Entities.Players.GuildMember[] { e.Member }));
            Mailboxes.Ui.Subscribe<PartyMemberAdded>(e => HUDManager.plateBoxHandler.AddGuildMemberToParty(e.Member));
            Mailboxes.Ui.Subscribe<PartyMemberRemoved>(e => HUDManager.plateBoxHandler.RemoveGuildMemberFromParty(e.Member));
            Mailboxes.Ui.Subscribe<GuildMembersSet>(e => HUDManager.windowHandler.SetGuildMembers(e.Members));
            Mailboxes.Ui.Subscribe<GuildMemberAdded>(e => HUDManager.windowHandler.AddGuildMember(e.Member));
            Mailboxes.Ui.Subscribe<PartyCleared>(_ => HUDManager.plateBoxHandler.ClearParty());
            Mailboxes.Ui.Subscribe<InspectWindowToggled>(e => HUDManager.windowHandler.ToggleInspectWindow(e.Member));
            Mailboxes.Ui.Subscribe<CharacterWindowToggled>(_ => HUDManager.windowHandler.ToggleCharacterWindow());
            Mailboxes.Ui.Subscribe<DescriptorBoxClear>(_ => HUDManager.SetDescriptorBox(null));
            Mailboxes.Ui.Subscribe<HeldItemStart>(e => HUDManager.HoldItem(e.Source as Project_1.UI.HUD.Inventory.Item, e.GrabOffset));
            Mailboxes.Ui.Subscribe<HeldItemEnd>(_ => HUDManager.ReleaseItem());
            Mailboxes.Ui.Subscribe<HeldSpellStart>(e => HUDManager.HoldSpell(e.Spell, e.GrabOffset));
            Mailboxes.Ui.Subscribe<HeldSpellEnd>(_ => HUDManager.ReleaseSpell());
            Mailboxes.Ui.Subscribe<HudMovableChanged>(e =>
            {
                if (e.Enabled) HUDManager.SetHudMoveable(true);
                else HUDManager.ResetHudMoveable();
            });
            Mailboxes.Ui.Subscribe<HudSizeChangeRequested>(e =>
            {
                if (e.Enabled) HUDManager.ChangeSizes();
                else HUDManager.DisableSizeChanges();
            });
            Mailboxes.Ui.Subscribe<DialogueOpened>(e => HUDManager.AddDialogueBox(e.Box));
            Mailboxes.Ui.Subscribe<DialogueClosed>(e => HUDManager.RemoveDialogueBox(e.Box));
        }
    }
}
