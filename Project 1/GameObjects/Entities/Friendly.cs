using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects.Unit;
using Project_1.GameObjects.Unit.Classes;
using Project_1.GameObjects.Entities.GuildMembers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Entities
{
    internal class Friendly : Entity
    {
        public override Color MinimapColor => Color.Pink;
        public new FriendlyClassData ClassData => base.ClassData as FriendlyClassData;
        public Friendly(UnitData aUnitData) : base(aUnitData)
        {

        }


        public GuildMembers.GuildMember.GuildMemberData CreateGuildMemberData()
        {
            return new GuildMembers.GuildMember.GuildMemberData(Name, CurrentLevel, Class);
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
