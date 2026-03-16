namespace Project_1.Messaging.Events
{
    internal readonly struct WorkerCompletionReady
    {
        public WorkerCompletionReady(int completionId)
        {
            CompletionId = completionId;
        }

        public int CompletionId { get; }
    }
}
