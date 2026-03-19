using Project_1.Managers.Saves;

namespace Project_1.Messaging.Events
{
    internal readonly struct SaveLoadParsed
    {
        public SaveLoadParsed(SaveLoadPayload payload, int requestId)
        {
            Payload = payload;
            RequestId = requestId;
        }

        public SaveLoadPayload Payload { get; }
        public int RequestId { get; }
    }
}
