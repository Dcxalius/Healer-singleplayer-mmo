using System;

namespace Project_1.Messaging
{
    internal interface IMailboxChannel
    {
        Type MessageType { get; }
        bool TryDispatchOne(string mailboxName, out bool hadSubscribers, out int handlerInvocations, out int handlerFailures);
    }
}
