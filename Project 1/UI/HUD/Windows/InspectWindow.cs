using Project_1.Messaging.Events;

namespace Project_1.UI.HUD.Windows
{
    internal class InspectWindow : CharacterWindow
    {
        protected override int BagIndexForItem => -4;

        public static InspectWindow Current { get; private set; }
        public static int? CurrentTargetRenderId { get; private set; }

        public int? GuildMemberRenderId => guildMemberRenderId;
        int? guildMemberRenderId;

        public InspectWindow() : base()
        {
            visibleKey = null;
            Current = this;
        }

        public bool BelongsTo(int aGuildMemberRenderId)
        {
            return guildMemberRenderId.HasValue && guildMemberRenderId.Value == aGuildMemberRenderId;
        }

        public void SetData(int aRenderId)
        {
            guildMemberRenderId = aRenderId;
            CurrentTargetRenderId = aRenderId;
        }

        public void SetData(in EntityUiSnapshot aSnapshot)
        {
            guildMemberRenderId = aSnapshot.RenderId;
            CurrentTargetRenderId = aSnapshot.RenderId;
            nameLabel.Text = aSnapshot.Name;
        }

        public void RemoveData()
        {
            guildMemberRenderId = null;
            CurrentTargetRenderId = null;
        }
    }
}
