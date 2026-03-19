namespace Project_1.Messaging
{
    internal readonly struct MailboxStats
    {
        public MailboxStats(
            string name,
            int pending,
            int peak,
            int lastDispatchCount,
            double lastDispatchMs,
            long totalDispatched,
            long totalPublished,
            long totalDispatchDequeued,
            long totalDispatchMisses,
            long totalWithoutSubscribers,
            long totalHandlerInvocations,
            long totalHandlerFailures,
            long totalCoalesced,
            long totalDropped,
            string topHandlerFailureType,
            long topHandlerFailureCount,
            string topCoalescedType,
            long topCoalescedCount,
            double lastOldestMessageAgeMs,
            double avgMessageAgeMs,
            double maxMessageAgeMs)
        {
            Name = name;
            Pending = pending;
            Peak = peak;
            LastDispatchCount = lastDispatchCount;
            LastDispatchMs = lastDispatchMs;
            TotalDispatched = totalDispatched;
            TotalPublished = totalPublished;
            TotalDispatchDequeued = totalDispatchDequeued;
            TotalDispatchMisses = totalDispatchMisses;
            TotalWithoutSubscribers = totalWithoutSubscribers;
            TotalHandlerInvocations = totalHandlerInvocations;
            TotalHandlerFailures = totalHandlerFailures;
            TotalCoalesced = totalCoalesced;
            TotalDropped = totalDropped;
            TopHandlerFailureType = topHandlerFailureType;
            TopHandlerFailureCount = topHandlerFailureCount;
            TopCoalescedType = topCoalescedType;
            TopCoalescedCount = topCoalescedCount;
            LastOldestMessageAgeMs = lastOldestMessageAgeMs;
            AvgMessageAgeMs = avgMessageAgeMs;
            MaxMessageAgeMs = maxMessageAgeMs;
        }

        public string Name { get; }
        public int Pending { get; }
        public int Peak { get; }
        public int LastDispatchCount { get; }
        public double LastDispatchMs { get; }
        public long TotalDispatched { get; }
        public long TotalPublished { get; }
        public long TotalDispatchDequeued { get; }
        public long TotalDispatchMisses { get; }
        public long TotalWithoutSubscribers { get; }
        public long TotalHandlerInvocations { get; }
        public long TotalHandlerFailures { get; }
        public long TotalCoalesced { get; }
        public long TotalDropped { get; }
        public string TopHandlerFailureType { get; }
        public long TopHandlerFailureCount { get; }
        public string TopCoalescedType { get; }
        public long TopCoalescedCount { get; }
        public double LastOldestMessageAgeMs { get; }
        public double AvgMessageAgeMs { get; }
        public double MaxMessageAgeMs { get; }
    }
}
