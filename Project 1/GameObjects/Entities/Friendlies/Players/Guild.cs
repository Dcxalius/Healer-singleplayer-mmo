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
using Project_1.Managers;
using Project_1.GameObjects.Entities.Friendlies.GuildMembers;
using Project_1.GameObjects.Entities.Friendlies.Players;

namespace Project_1.GameObjects.Entities.Friendlies.Players
{
    internal class Guild
    {
        Friendly owner;
        List<Friendly> guildMembers;

        public Guild(Friendly aOwner)
        {
            ThreadAffinity.AssertSimThread();
            owner = aOwner;
            guildMembers = new List<Friendly>();
            guildMembers.Add(aOwner);
            guildMembers.AddRange(ObjectManager.GetGuildMembers());

            SetRosterWindow();
        }

        public GuildMember GetGuildMemberByName(string aName)
        {
            ThreadAffinity.AssertSimThread();
            Debug.Assert(aName != null);
            Debug.Assert(aName != owner.Name, "Tried to get Player.");

            return guildMembers.Single(guildMember =>  guildMember.Name == aName) as GuildMember;
        }

        void SetRosterWindow()
        {
            ThreadAffinity.AssertSimThread();
            EntityUiSnapshot[] data = new EntityUiSnapshot[guildMembers.Count];
            for (int i = 0; i < guildMembers.Count; i++)
            {
                data[i] = guildMembers[i].BuildUiSnapshot();
            }
            Mailboxes.PublishUiEvent(new GuildMembersSet(data));
        }
    }
}
