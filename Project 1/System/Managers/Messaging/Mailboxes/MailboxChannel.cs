using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Project_1.Messaging
{
    internal sealed class MailboxChannel<T> : IMailboxChannel
    {
        readonly ConcurrentQueue<T> queue = new ConcurrentQueue<T>();
        readonly SubscriberList<T> subscribers = new SubscriberList<T>();
        readonly bool coalesced;
        readonly object coalescedGate = new object();
        bool hasPendingCoalescedMessage;
        T latestCoalescedMessage;

        public MailboxChannel(int id, bool coalesced)
        {
            Id = id;
            this.coalesced = coalesced;
        }

        public int Id { get; }
        public Type MessageType => typeof(T);

        public void Enqueue(in T message, out bool enqueueDispatchToken, out bool replacedPendingMessage)
        {
            if (!coalesced)
            {
                queue.Enqueue(message);
                enqueueDispatchToken = true;
                replacedPendingMessage = false;
                return;
            }

            lock (coalescedGate)
            {
                replacedPendingMessage = hasPendingCoalescedMessage;
                latestCoalescedMessage = message;
                hasPendingCoalescedMessage = true;
                enqueueDispatchToken = !replacedPendingMessage;
            }
        }

        public void Subscribe(Action<T> handler)
        {
            subscribers.Add(handler);
        }

        public bool TryDispatchOne(string mailboxName, out bool hadSubscribers, out int handlerInvocations, out int handlerFailures)
        {
            if (!TryDequeueMessage(out T message))
            {
                hadSubscribers = false;
                handlerInvocations = 0;
                handlerFailures = 0;
                return false;
            }

            Action<T>[] handlers = subscribers.Snapshot();
            hadSubscribers = handlers.Length > 0;
            if (!hadSubscribers)
            {
                handlerInvocations = 0;
                handlerFailures = 0;
                return true;
            }

            int failures = 0;
            for (int i = 0; i < handlers.Length; i++)
            {
                try
                {
                    handlers[i](message);
                }
                catch (Exception ex)
                {
                    failures++;
                    Console.WriteLine($"Mailbox '{mailboxName}' handler failure '{typeof(T).Name}': {ex.GetType().Name}: {ex.Message}");
                    Debug.WriteLine($"Mailbox '{mailboxName}' handler for '{typeof(T).Name}' threw and was skipped: {ex}");
                }
            }

            handlerInvocations = handlers.Length;
            handlerFailures = failures;
            return true;
        }

        bool TryDequeueMessage(out T message)
        {
            if (!coalesced)
            {
                return queue.TryDequeue(out message);
            }

            lock (coalescedGate)
            {
                if (!hasPendingCoalescedMessage)
                {
                    message = default;
                    return false;
                }

                message = latestCoalescedMessage;
                latestCoalescedMessage = default;
                hasPendingCoalescedMessage = false;
                return true;
            }
        }
    }
}
