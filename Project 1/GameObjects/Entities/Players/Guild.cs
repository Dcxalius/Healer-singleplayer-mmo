using Project_1.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Project_1.UI.HUD.Managers;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.GameObjects.Entities.GuildMembers;

namespace Project_1.GameObjects.Entities.Players
{
    internal class Guild
    {
        Friendly owner;
        List<Friendly> guildMembers;

        public Guild(Friendly aOwner)
        {
            owner = aOwner;
            guildMembers = new List<Friendly>();
            guildMembers.Add(aOwner);
            guildMembers.AddRange(ObjectManager.GetGuildMembers());

            SetRosterWindow();
        }

        public GuildMembers.GuildMember GetGuildMemberByName(string aName)
        {
            Debug.Assert(aName != null);
            Debug.Assert(aName != owner.Name, "Tried to get Player.");

            return guildMembers.Single(guildMember =>  guildMember.Name == aName) as GuildMembers.GuildMember;
        }

        void SetRosterWindow()
        {
            GuildMembers.GuildMember.GuildMemberData[] data = new GuildMembers.GuildMember.GuildMemberData[guildMembers.Count];
            for (int i = 0; i < guildMembers.Count; i++)
            {
                data[i] = guildMembers[i].CreateGuildMemberData();
            }
            Mailboxes.Ui.Publish(new GuildMembersSet(guildMembers.ToArray()));
        }
    }
}
