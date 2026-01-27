using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_1.GameObjects.Entities.Friendlies.GuildMembers;

namespace Project_1.GameObjects.Entities.Friendlies.GuildMembers
{
    internal class Continue : ILogicResult
    {
        public void ExecuteResult(GuildMember guildMember)
        {
            // Do nothing, just continue current behavior
        }
    }
}
