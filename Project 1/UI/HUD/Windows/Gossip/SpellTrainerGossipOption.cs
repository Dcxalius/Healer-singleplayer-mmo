using Project_1.Messaging;
using Project_1.Messaging.Events;

namespace Project_1.UI.HUD.Windows.Gossip
{
    internal class SpellTrainerGossipOption : GossipOption
    {
        public SpellTrainerGossipOption(string descriptor) : base(descriptor)
        {
            Actions.Add(() => MailboxManager.PublishSimCommand(SpellTrainingWindowRequested.Instance));
        }
    }
}
