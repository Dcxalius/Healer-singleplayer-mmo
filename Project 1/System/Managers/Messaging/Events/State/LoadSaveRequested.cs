namespace Project_1.Messaging.Events
{
    internal readonly struct LoadSaveRequested
    {
        public LoadSaveRequested(string saveName)
        {
            SaveName = saveName;
        }

        public string SaveName { get; }
    }
}
