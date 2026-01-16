using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Entities.GuildMembers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.HUD.Windows
{
    internal class InspectWindow : CharacterWindow
    {
        protected override int BagIndexForItem => -4;
        public bool BelongsTo(GuildMember aGuildMember) => guildMemberSetTo == aGuildMember; //TODO: Ponder if this is the best way

        public static InspectWindow Current { get; private set; }
        public static GuildMember CurrentTarget { get; private set; }

        public GuildMember GuildMember => guildMemberSetTo;
        GuildMember guildMemberSetTo;

        public InspectWindow() : base()
        {
            visibleKey = null;
            Current = this;
        }
        public override void SetData(Friendly aFriendly)
        {
            base.SetData(aFriendly);
            guildMemberSetTo = aFriendly as GuildMember;
            CurrentTarget = guildMemberSetTo;
        }

        public void RemoveData()
        {
            guildMemberSetTo = null;
            CurrentTarget = null;
        }
    }
}
