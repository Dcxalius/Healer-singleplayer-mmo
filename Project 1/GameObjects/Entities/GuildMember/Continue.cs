using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Entities.GuildMembers
{
    internal class Continue : ILogicResult
    {
        public void ExecuteResult(GuildMember guildMember)
        {
            // Do nothing, just continue current behavior
        }
    }
}
