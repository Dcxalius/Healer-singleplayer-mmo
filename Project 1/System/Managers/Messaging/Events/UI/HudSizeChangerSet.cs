namespace Project_1.Messaging.Events
{
    internal readonly struct HudSizeChangerSet
    {
        public HudSizeChangerSet(int uiElementId)
        {
            UiElementId = uiElementId;
        }

        public int UiElementId { get; }
    }
}
