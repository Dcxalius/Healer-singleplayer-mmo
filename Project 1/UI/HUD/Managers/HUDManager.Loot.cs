using Project_1.Camera;
using Project_1.Messaging.Events;
using Project_1.UI.HUD.Chat;

namespace Project_1.UI.HUD.Managers
{
    internal static partial class HUDManager
    {
        public static void Loot(ItemUiSnapshot[] snapshot, LootContext context)
        {
            AssertUiThreadOrMainFallback();
            lootBox.Loot(context, snapshot);
            InvalidateUi();
        }

        public static void RefreshLootSlot(int slot, ItemUiSnapshot snapshot)
        {
            AssertUiThreadOrMainFallback();
            lootBox.RefreshSlot(slot, snapshot);
            InvalidateUi();
        }

        static void HandleLootOpened(LootOpened e)
        {
            Loot(e.Snapshot, e.Context);
        }

        static void HandleSelfExperienceForChat(int aCurrentLevel, int aCurrentExperience)
        {
            if (!hasSelfExpSnapshot)
            {
                hasSelfExpSnapshot = true;
                lastSelfLevel = aCurrentLevel;
                lastSelfExperience = aCurrentExperience;
                return;
            }

            if (aCurrentLevel > lastSelfLevel)
            {
                chatPanel.AddMessage(ChatMessageType.Experience, $"Level up! You are now level {aCurrentLevel}.");
            }
            else if (aCurrentLevel == lastSelfLevel)
            {
                int delta = aCurrentExperience - lastSelfExperience;
                if (delta > 0)
                {
                    chatPanel.AddMessage(ChatMessageType.Experience, $"+{delta} experience.");
                }
            }

            lastSelfLevel = aCurrentLevel;
            lastSelfExperience = aCurrentExperience;
        }

        public static void HoldItem(Project_1.UI.HUD.Inventory.Item aItem, AbsoluteScreenPosition aGrabOffset)
        {
            AssertUiThreadOrMainFallback();
            heldItem.HoldItem(aItem, aGrabOffset);
            InvalidateUi();
        }

        public static void ReleaseItem()
        {
            AssertUiThreadOrMainFallback();
            heldItem.ReleaseMe();
            InvalidateUi();
        }
    }
}
