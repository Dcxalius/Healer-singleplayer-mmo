using Project_1.Camera;
using Project_1.Messaging.Events;

namespace Project_1.UI.HUD.Managers
{
    internal static partial class HUDManager
    {
        public static void SetInventory(InventoryUiSnapshot snapshot)
        {
            AssertUiThreadOrMainFallback();
            inventoryBox.SetInventory(snapshot);
            InvalidateUi();
        }

        public static void RefreshInventorySlot(int aBag, int aSlot, InventoryUiSnapshot snapshot)
        {
            AssertUiThreadOrMainFallback();
            inventoryBox.RefreshSlot(aBag, aSlot, snapshot);
            InvalidateUi();
        }

        public static void RefreshInventorySlot((int, int) aBagAndSlot, InventoryUiSnapshot snapshot) => RefreshInventorySlot(aBagAndSlot.Item1, aBagAndSlot.Item2, snapshot);

        public static void SetDescriptorBox(in ItemDescriptorSnapshot snapshot, RelativeScreenPosition aPos)
        {
            AssertUiThreadOrMainFallback();
            descriptorBox.SetToSnapshot(snapshot, aPos);
            InvalidateUi();
        }

        public static void SetDescriptorBox(in SpellDescriptorSnapshot snapshot, RelativeScreenPosition aPos)
        {
            AssertUiThreadOrMainFallback();
            descriptorBox.SetToSnapshot(snapshot, aPos);
            InvalidateUi();
        }

        public static void ClearDescriptorBox()
        {
            AssertUiThreadOrMainFallback();
            descriptorBox.Clear();
            InvalidateUi();
        }

        public static void RefreshGold(int aGoldAmount)
        {
            AssertUiThreadOrMainFallback();
            inventoryBox.RefreshGold(aGoldAmount);
            InvalidateUi();
        }
    }
}
