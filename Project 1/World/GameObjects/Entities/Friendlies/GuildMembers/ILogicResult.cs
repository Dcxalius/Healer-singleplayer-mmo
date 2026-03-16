using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_1.GameObjects.Entities.Friendlies.GuildMembers;

namespace Project_1.GameObjects.Entities.Friendlies.GuildMembers
{
    internal interface ILogicResult
    {
        void ExecuteResult(GuildMember guildMember);

    }
}
