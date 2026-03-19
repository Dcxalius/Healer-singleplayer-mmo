namespace Project_1.Messaging.Events
{
    internal readonly struct CreateNewPlayerRequested
    {
        public CreateNewPlayerRequested(string name, string className)
        {
            Name = name;
            ClassName = className;
        }

        public string Name { get; }
        public string ClassName { get; }
    }
}
