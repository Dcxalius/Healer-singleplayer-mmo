using Project_1.Camera;
using Project_1.GameObjects.Unit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Entities
{
    internal class Friendly : Entity
    {
        public Friendly(UnitData aUnitData) : base(aUnitData)
        {

        }


        public GuildMember.GuildMemberData CreateGuildMemberData()
        {
            return new GuildMember.GuildMemberData(Name, CurrentLevel, Class);
        }
        public override void ExpToParty(int aExpAmount)
        {
            throw new NotImplementedException();
        }

        protected override bool CheckForRelation()
        {
            throw new NotImplementedException();
        }
    }
}
