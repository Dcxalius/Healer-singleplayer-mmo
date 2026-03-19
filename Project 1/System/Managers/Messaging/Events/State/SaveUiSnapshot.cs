namespace Project_1.Messaging.Events
{
    internal readonly struct SaveUiSnapshot
    {
        public SaveUiSnapshot(string saveName, string detailsText, string imagePath)
        {
            SaveName = saveName ?? string.Empty;
            DetailsText = detailsText ?? string.Empty;
            ImagePath = imagePath ?? string.Empty;
        }

        public string SaveName { get; }
        public string DetailsText { get; }
        public string ImagePath { get; }
    }
}
