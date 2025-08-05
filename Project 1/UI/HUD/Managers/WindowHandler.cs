using Project_1.Camera;
using Project_1.GameObjects.Entities.Npcs;
using Project_1.UI.HUD.Windows.Gossip;
using Project_1.UI.HUD.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_1.UI.UIElements;
using Project_1.GameObjects.Entities;
using Project_1.UI.UIElements.Buttons;
using Project_1.GameObjects.Entities.Players;
using Project_1.GameObjects.Unit;
using Project_1.GameObjects.Spells;

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
        public void InitWindows(ref List<UIElement> aHudElements)
        {
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

        }

        public void OpenShopWindow(ShopGossipOption aShop, Npc aNpc)
        {
            shopWindow.OpenWindow();
            shopWindow.OpenShop(aShop, aNpc);
        }

        public void OpenGossipWindow(ChatGossipOption aIntro, Npc aNpc)
        {
            gossipWindow.OpenWindow();
            gossipWindow.ResetOptions();
            gossipWindow.Set(aIntro, aNpc);
            //gossipWindow.SetIntro(aIntro);
            //gossipWindow.AddOptions(aGossipOption);
        }

        public void CloseGossipWindow()
        {
            gossipWindow.CloseWindow();
        }

        public void AddGuildMember(Friendly aData)
        {
            //guildWindow.
        }

        public void SetGuildMembers(Friendly[] aData)
        {
            guildWindow.SetRoster(aData);
        }

        public void SetGuildMemberInviteStatus(List<string> aName, List<TwoStateGFXButton.State> aState)
        {
            guildWindow.SetGuildMemberInviteStatus(aName, aState);
        }
        public void SetCharacterWindow(Player aPlayer) => characterWindow.SetData(aPlayer);
        public void ToggleCharacterWindow() => characterWindow.ToggleVisibilty();

        public void RefreshAllCharacterWindowSlots(Equipment aEquipment, Friendly aFriendly)
        {
            for (int i = 0; i < (int)Equipment.Slot.Count; i++)
            {
                RefreshCharacterWindowSlot((Equipment.Slot)i, aEquipment, aFriendly);
            }
        }

        public void RefreshCharacterWindowSlot(Equipment.Slot aSlot, Equipment aEquipment, Friendly aFriendly)
        {
            if (aFriendly == null) return;
            if (aFriendly.RelationToPlayer == Relation.RelationToPlayer.Self)
            {
                characterWindow.SetSlot(aSlot, aEquipment);
                return;
            }

            if (!inspectWindow.BelongsTo(aFriendly as GuildMember)) return;

            inspectWindow.SetSlot(aSlot, aEquipment);
        }

        public void RefreshCharacterWindowStats(PairReport aReport, Friendly aFriendly)
        {
            if (aFriendly == null) return;

            if (aFriendly.RelationToPlayer == Relation.RelationToPlayer.Self)
            {
                characterWindow.SetReportBox(aReport);
                return;
            }
            if (!inspectWindow.BelongsTo(aFriendly as GuildMember)) return;

            inspectWindow.SetReportBox(aReport);
        }

        public void RefreshCharacterWindowExpBar(Friendly aFriendly)
        {
            if (aFriendly.RelationToPlayer == Relation.RelationToPlayer.Self)
            {
                characterWindow.RefreshExp(aFriendly.Level);
                return;
            }

            if (!inspectWindow.BelongsTo(aFriendly as GuildMember)) return;

            inspectWindow.RefreshExp(aFriendly.Level);

        }

        public GuildMember GetGuildMemberInspectWindowTarget() => inspectWindow.GuildMember;

        public bool PlayerCharacterPaneOpen => characterWindow.Visible;

        public void ToggleInspectWindow(GuildMember aGuildMember)
        {
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
            if (inspectWindow.Visible == false) return;

            inspectWindow.ToggleVisibilty();
        }

        public bool IsShopOpen()
        {
            return shopWindow.Visible;
        }

        public void AddSpellToSpellBook(Spell aSpell) => spellBookWindow.AssignSpell(aSpell);
        public void RefreshSpellBook(Spell[] aSpells) => spellBookWindow.RefreshSpells(aSpells);
    }
}
