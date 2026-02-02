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
using Project_1.GameObjects.Spells;
using Project_1.UI.HUD.Windows.Logic;
using Project_1.GameObjects.Entities.Friendlies;
using Project_1.GameObjects.Entities.Friendlies.GuildMembers;
using Project_1.GameObjects.Entities.Friendlies.Players;
using Project_1.GameObjects.Entities.Friendlies.Npcs;
using Project_1.Managers;

namespace Project_1.UI.HUD.Managers
{
    internal class WindowHandler
    {
        static CharacterWindow characterWindow;
        static SpellBookWindow spellBookWindow;
        static GuildWindow guildWindow;
        static InspectWindow inspectWindow;
        static GossipWindow gossipWindow;
        static ShopWindow shopWindow;
        static LogicWindow logicWindow;

        public void InitWindows(ref List<UIElement> aHudElements)
        {
            ThreadAffinity.AssertMainThread();
            Window.Init(new RelativeScreenPosition(0.05f, 0.2f), new RelativeScreenPosition(0.1f, 0f), new RelativeScreenPosition(0.2f, 0.6f));

            characterWindow = new CharacterWindow();
            aHudElements.Add(characterWindow);

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

            logicWindow = new LogicWindow();
            aHudElements.Add(logicWindow);
        }

        public void OpenShopWindow(int[] itemIds)
        {
            ThreadAffinity.AssertUiThread();
            shopWindow.OpenWindow();
            shopWindow.OpenShop(itemIds);
            HUDManager.InvalidateUi();
        }

        public void OpenGossipWindow(GossipData aData)
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

        public void AddGuildMember(Friendly aData)
        {
            //guildWindow.
        }

        public void SetGuildMembers(Friendly[] aData)
        {
            ThreadAffinity.AssertUiThread();
            guildWindow.SetRoster(aData);
            HUDManager.InvalidateUi();
        }

        public void SetGuildMemberInviteStatus(List<string> aName, List<TwoStateGFXButton.State> aState)
        {
            ThreadAffinity.AssertUiThread();
            guildWindow.SetGuildMemberInviteStatus(aName, aState);
            HUDManager.InvalidateUi();
        }
        public void SetCharacterWindow(Player aPlayer)
        {
            ThreadAffinity.AssertUiThread();
            characterWindow.SetData(aPlayer);
        }
        public void ToggleCharacterWindow()
        {
            ThreadAffinity.AssertUiThread();
            characterWindow.ToggleVisibilty();
            HUDManager.InvalidateUi();
        }

        public void RefreshAllCharacterWindowSlots(Equipment aEquipment, Friendly aFriendly)
        {
            ThreadAffinity.AssertUiThread();
            for (int i = 0; i < (int)Equipment.Slot.Count; i++)
            {
                RefreshCharacterWindowSlot((Equipment.Slot)i, aEquipment, aFriendly);
            }
            HUDManager.InvalidateUi();
        }

        public void RefreshCharacterWindowSlot(Equipment.Slot aSlot, Equipment aEquipment, Friendly aFriendly)
        {
            ThreadAffinity.AssertUiThread();
            if (aFriendly == null) return;
            if (aFriendly.RelationToPlayer == Relation.RelationToPlayer.Self)
            {
                characterWindow.SetSlot(aSlot, aEquipment);
                HUDManager.InvalidateUi();
                return;
            }

            if (!inspectWindow.BelongsTo(aFriendly as GuildMember)) return;

            inspectWindow.SetSlot(aSlot, aEquipment);
            HUDManager.InvalidateUi();
        }

        public void RefreshCharacterWindowStats(PairReport aReport, Friendly aFriendly)
        {
            ThreadAffinity.AssertUiThread();
            if (aFriendly == null) return;

            if (aFriendly.RelationToPlayer == Relation.RelationToPlayer.Self)
            {
                characterWindow.SetReportBox(aReport);
                HUDManager.InvalidateUi();
                return;
            }
            if (!inspectWindow.BelongsTo(aFriendly as GuildMember)) return;

            inspectWindow.SetReportBox(aReport);
            HUDManager.InvalidateUi();
        }

        public void RefreshCharacterWindowExpBar(Friendly aFriendly)
        {
            ThreadAffinity.AssertUiThread();
            if (aFriendly.RelationToPlayer == Relation.RelationToPlayer.Self)
            {
                characterWindow.RefreshExp(aFriendly.Level);
                HUDManager.InvalidateUi();
                return;
            }

            if (!inspectWindow.BelongsTo(aFriendly as GuildMember)) return;

            inspectWindow.RefreshExp(aFriendly.Level);
            HUDManager.InvalidateUi();

        }

        public GuildMember GetGuildMemberInspectWindowTarget()
        {
            ThreadAffinity.AssertUiThread();
            return inspectWindow.GuildMember;
        }

        public bool PlayerCharacterPaneOpen
        {
            get
            {
                ThreadAffinity.AssertUiThread();
                return characterWindow.Visible;
            }
        }

        public void ToggleInspectWindow(GuildMember aGuildMember)
        {
            ThreadAffinity.AssertUiThread();
            if (inspectWindow.Visible == true && inspectWindow.BelongsTo(aGuildMember))
            {
                inspectWindow.ToggleVisibilty();
                inspectWindow.RemoveData();
                return;
            }
            inspectWindow.SetData(aGuildMember);
            if (inspectWindow.Visible == false)
            {
                inspectWindow.ToggleVisibilty();
            }
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

        public void AddSpellToSpellBook(Spell aSpell)
        {
            ThreadAffinity.AssertUiThread();
            spellBookWindow.AssignSpell(aSpell);
        }
        public void RefreshSpellBook(Spell[] aSpells)
        {
            ThreadAffinity.AssertUiThread();
            spellBookWindow.RefreshSpells(aSpells);
        }
    }
}
