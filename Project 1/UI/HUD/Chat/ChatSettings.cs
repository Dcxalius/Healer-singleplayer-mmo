using Project_1.Managers;
using Project_1.Messaging.Events;

namespace Project_1.UI.HUD.Chat
{
    internal static class ChatSettings
    {
        static readonly bool[] visibleByType = BuildDefaultVisibility();

        static bool[] BuildDefaultVisibility()
        {
            bool[] values = new bool[(int)ChatMessageType.Count];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = true;
            }
            return values;
        }

        public static bool IsVisible(ChatMessageType aType)
        {
            if (!InRange(aType)) return false;
            return visibleByType[(int)aType];
        }

        public static void SetVisible(ChatMessageType aType, bool aVisible)
        {
            ThreadAffinity.AssertUiThread();
            if (!InRange(aType)) return;
            visibleByType[(int)aType] = aVisible;
        }

        static bool InRange(ChatMessageType aType)
        {
            int index = (int)aType;
            return index >= 0 && index < visibleByType.Length;
        }

        public static void ExportSettings()
        {
            ThreadAffinity.AssertUiThread();
            // TODO: Persist chat settings when settings persistence is implemented for chat.
        }
    }
}
