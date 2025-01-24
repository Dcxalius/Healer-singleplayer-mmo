using Project_1.UI.HUD;
using Project_1.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Entities.Players
{
    internal class Guild
    {
        Friendly owner;
        List<Friendly> guildMembers;

        public Guild(Friendly aOwner)
        {
            guildMembers = new List<Friendly>();
            guildMembers.Add(aOwner);

            guildMembers.AddRange(ObjectManager.GetGuildMembers());
            SetRosterWindow();
        }

        void SetRosterWindow()
        {
            GuildMember.GuildMemberData[] data = new GuildMember.GuildMemberData[guildMembers.Count];
            for (int i = 0; i < guildMembers.Count; i++)
            {
                data[i] = guildMembers[i].CreateGuildMemberData();
            }
            HUDManager.SetGuildMembers(guildMembers.ToArray());
        }
    }
}
