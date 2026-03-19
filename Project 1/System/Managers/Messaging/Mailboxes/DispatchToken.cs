namespace Project_1.Messaging
{
    internal readonly struct DispatchToken
    {
        public DispatchToken(int channelId, long enqueueTicks)
        {
            ChannelId = channelId;
            EnqueueTicks = enqueueTicks;
        }

        public int ChannelId { get; }
        public long EnqueueTicks { get; }
    }
}
