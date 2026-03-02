using Project_1.Messaging.Events;
using Project_1.UI.HUD.Chat;

namespace Project_1.UI.HUD.Managers
{
    internal static partial class HUDManager
    {
        static void HandlePartyMemberAdded(in EntityUiSnapshot member)
        {
            plateBoxHandler.AddGuildMemberToParty(member);
            chatPanel.AddMessage(ChatMessageType.Party, $"{member.Name} joined the party.");
            InvalidateUi();
        }

        static void HandlePartyMemberRemoved(int memberRenderId)
        {
            plateBoxHandler.RemoveGuildMemberFromParty(memberRenderId);
            chatPanel.AddMessage(ChatMessageType.Party, "A party member left the party.");
            InvalidateUi();
        }

        static void HandleGuildMemberAdded(in EntityUiSnapshot member)
        {
            windowHandler.AddGuildMember(member);
            chatPanel.AddMessage(ChatMessageType.Guild, $"{member.Name} came online.");
            InvalidateUi();
        }
    }
}
