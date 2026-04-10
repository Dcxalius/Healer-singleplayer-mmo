using Project_1.Camera;
using Project_1.UI.HUD.Windows.Gossip;
using Project_1.UI.HUD.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Buttons;
using Project_1.GameObjects.Unit;
using Project_1.UI.HUD.Windows.Logic;
using Project_1.Managers;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.Items;

namespace Project_1.UI.HUD.Managers
{
    internal class WindowHandler
    {
        static CharacterWindow characterWindow;
        static TalentWindow talentWindow;
        static SpellBookWindow spellBookWindow;
        static GuildWindow guildWindow;
        static InspectWindow inspectWindow;
        static GossipWindow gossipWindow;
        static ShopWindow shopWindow;
        static SpellTrainingWindow spellTrainingWindow;
        static LogicWindow logicWindow;

        public void InitWindows(ref List<UIElement> aHudElements)
        {
            ThreadAffinity.AssertMainThread();
            Window.Init(new RelativeScreenPosition(0.05f, 0.2f), new RelativeScreenPosition(0.1f, 0f), new RelativeScreenPosition(0.2f, 0.6f));

            characterWindow = new CharacterWindow();
            aHudElements.Add(characterWindow);

            talentWindow = new TalentWindow();
            aHudElements.Add(talentWindow);

            spellBookWindow = new SpellBookWindow();
            aHudElements.Add(spellBookWindow);

            inspectWindow = new InspectWindow();
            aHudElements.Add(inspectWindow);

            guildWindow = new GuildWindow();
            aHudElements.Add(guildWindow);

            gossipWindow = new GossipWindow();
            aHudElements.Add(gossipWindow);
            ChatGossipOption.SetGossipWindow = gossipWindow.GetSet();
            ShopGossipOption.CloseGossipWindowAndOpenShop = gossipWindow.GetClose();

            shopWindow = new ShopWindow();
            aHudElements.Add(shopWindow);

            spellTrainingWindow = new SpellTrainingWindow();
            aHudElements.Add(spellTrainingWindow);

            logicWindow = new LogicWindow();
            aHudElements.Add(logicWindow);
        }

        public void OpenShopWindow(int[] itemIds, string shopkeeperName)
        {
            ThreadAffinity.AssertUiThread();
            shopWindow.OpenWindow();
            shopWindow.OpenShop(itemIds, shopkeeperName);
            HUDManager.InvalidateUi();
        }

        public void OpenGossipWindow(in GossipUiSnapshot aData)
        {
            ThreadAffinity.AssertUiThread();
            gossipWindow.OpenWindow();
            gossipWindow.ResetOptions();
            gossipWindow.Set(aData);
            HUDManager.InvalidateUi();
            //gossipWindow.SetIntro(aIntro);
            //gossipWindow.AddOptions(aGossipOption);
        }

        public void CloseGossipWindow()
        {
            ThreadAffinity.AssertUiThread();
            gossipWindow.CloseWindow();
            HUDManager.InvalidateUi();
        }

        public void CloseShopWindow()
        {
            ThreadAffinity.AssertUiThread();
            shopWindow.ClearShop();
            shopWindow.CloseWindow();
            HUDManager.InvalidateUi();
        }

        public void OpenSpellTrainingWindow(SpellTrainingEntrySnapshot[] entries, string trainerName)
        {
            ThreadAffinity.AssertUiThread();
            gossipWindow.CloseWindow();
            spellTrainingWindow.OpenWindow();
            spellTrainingWindow.OpenTrainer(entries, trainerName);
            HUDManager.InvalidateUi();
        }

        public void CloseSpellTrainingWindow()
        {
            ThreadAffinity.AssertUiThread();
            spellTrainingWindow.ClearTrainer();
            spellTrainingWindow.CloseWindow();
            HUDManager.InvalidateUi();
        }

        public void AddGuildMember(EntityUiSnapshot aData)
        {
            ThreadAffinity.AssertUiThread();
            guildWindow.AddMember(aData);
            HUDManager.InvalidateUi();
        }

        public void SetGuildMembers(EntityUiSnapshot[] aData)
        {
            ThreadAffinity.AssertUiThread();
            guildWindow.SetRoster(aData);
            HUDManager.InvalidateUi();
        }

        public void SetGuildMemberInviteStatus(string[] names, InviteStatus[] statuses)
        {
            ThreadAffinity.AssertUiThread();
            List<string> memberNames = names == null ? new List<string>() : new List<string>(names);
            int statusCount = statuses?.Length ?? 0;
            var buttonStates = new List<TwoStateGFXButton.State>(statusCount);
            for (int i = 0; i < statusCount; i++)
            {
                buttonStates.Add(statuses[i] == InviteStatus.Accepted
                    ? TwoStateGFXButton.State.Second
                    : TwoStateGFXButton.State.First);
            }
            guildWindow.SetGuildMemberInviteStatus(memberNames, buttonStates);
            HUDManager.InvalidateUi();
        }
        public void SetCharacterWindow(CharacterWindowSnapshot snapshot)
        {
            ThreadAffinity.AssertUiThread();
            characterWindow.SetData(snapshot);
        }
        public void ToggleCharacterWindow()
        {
            ThreadAffinity.AssertUiThread();
            characterWindow.ToggleVisibilty();
            HUDManager.InvalidateUi();
        }

        public void SetTalentWindow(TalentWindowSnapshot snapshot)
        {
            ThreadAffinity.AssertUiThread();
            if (!talentWindow.Matches(snapshot.OwnerSnapshot)) return;
            talentWindow.SetData(snapshot);
            HUDManager.InvalidateUi();
        }

        public void ToggleTalentWindow(in EntityUiSnapshot member)
        {
            ThreadAffinity.AssertUiThread();
            bool shouldRequestSnapshot = talentWindow.ToggleForMember(member);
            if (shouldRequestSnapshot)
            {
                MailboxManager.PublishSimCommand(new TalentWindowSnapshotRequested(
                    member.RelationToPlayer == RelationToPlayerKind.Self ? null : member.RenderId));
            }

            HUDManager.InvalidateUi();
        }

        public void RefreshAllCharacterWindowSlots(int ownerRenderId, RelationToPlayerKind ownerRelation, ItemUiSnapshot[] itemSnapshots)
        {
            ThreadAffinity.AssertUiThread();
            if (itemSnapshots == null) return;
            int count = Math.Min(itemSnapshots.Length, (int)EquipmentSlotKind.Count);
            for (int i = 0; i < count; i++)
            {
                RefreshCharacterWindowSlot(ownerRenderId, ownerRelation, (EquipmentSlotKind)i, itemSnapshots[i]);
            }
            HUDManager.InvalidateUi();
        }

        public void RefreshCharacterWindowSlot(int ownerRenderId, RelationToPlayerKind ownerRelation, EquipmentSlotKind slot, ItemUiSnapshot itemSnapshot)
        {
            ThreadAffinity.AssertUiThread();
            if (ownerRelation == RelationToPlayerKind.Self)
            {
                characterWindow.SetSlot(slot.ToEquipmentSlot(), itemSnapshot);
                HUDManager.InvalidateUi();
                return;
            }

            if (!inspectWindow.BelongsTo(ownerRenderId)) return;

            inspectWindow.SetSlot(slot.ToEquipmentSlot(), itemSnapshot);
            HUDManager.InvalidateUi();
        }

        public void RefreshCharacterWindowStats(int ownerRenderId, RelationToPlayerKind ownerRelation, StatReportSnapshot primaryReport, StatReportSnapshot secondaryReport)
        {
            ThreadAffinity.AssertUiThread();
            if (ownerRelation == RelationToPlayerKind.Self)
            {
                characterWindow.SetReportBox(primaryReport, secondaryReport);
                HUDManager.InvalidateUi();
                return;
            }
            if (!inspectWindow.BelongsTo(ownerRenderId)) return;

            inspectWindow.SetReportBox(primaryReport, secondaryReport);
            HUDManager.InvalidateUi();
        }

        public void RefreshCharacterWindowExpBar(int ownerRenderId, RelationToPlayerKind ownerRelation, int currentLevel, int currentExperience)
        {
            ThreadAffinity.AssertUiThread();
            if (ownerRelation == RelationToPlayerKind.Self)
            {
                characterWindow.RefreshExp(currentLevel, currentExperience);
                HUDManager.InvalidateUi();
                return;
            }

            if (!inspectWindow.BelongsTo(ownerRenderId)) return;

            inspectWindow.RefreshExp(currentLevel, currentExperience);
            HUDManager.InvalidateUi();

        }

        public int? GetGuildMemberInspectWindowTarget()
        {
            ThreadAffinity.AssertUiThread();
            return inspectWindow.GuildMemberRenderId;
        }

        public bool PlayerCharacterPaneOpen
        {
            get
            {
                ThreadAffinity.AssertUiThread();
                return characterWindow.Visible;
            }
        }

        public void ToggleInspectWindow(in EntityUiSnapshot member)
        {
            ThreadAffinity.AssertUiThread();
            if (inspectWindow.Visible == true && inspectWindow.BelongsTo(member.RenderId))
            {
                inspectWindow.ToggleVisibilty();
                inspectWindow.RemoveData();
                return;
            }
            inspectWindow.SetData(member);
            if (inspectWindow.Visible == false)
            {
                inspectWindow.ToggleVisibilty();
            }
        }

        public void OpenLogicWindow(int memberRenderId)
        {
            ThreadAffinity.AssertUiThread();
            logicWindow.SetData(memberRenderId);
            logicWindow.OpenWindow();
            HUDManager.InvalidateUi();
        }

        public void SetLogicWindowSnapshot(int memberRenderId, LogicNodeUiSnapshot[] nodes)
        {
            ThreadAffinity.AssertUiThread();
            logicWindow.SetSnapshot(memberRenderId, nodes);
            HUDManager.InvalidateUi();
        }

        public void CloseGuildWindow()
        {
            ThreadAffinity.AssertUiThread();
            if (inspectWindow.Visible == false) return;

            inspectWindow.ToggleVisibilty();
        }

        public bool IsShopOpen()
        {
            ThreadAffinity.AssertUiThread();
            return shopWindow.Visible;
        }

        public void AddSpellToSpellBook(string spellName)
        {
            ThreadAffinity.AssertUiThread();
            spellBookWindow.AssignSpell(spellName);
        }
        public void RefreshSpellBook(string[] spellNames)
        {
            ThreadAffinity.AssertUiThread();
            spellBookWindow.RefreshSpells(spellNames);
        }
    }
}
