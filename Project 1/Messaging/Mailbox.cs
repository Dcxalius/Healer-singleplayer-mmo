using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Project_1.Messaging
{
    /// <summary>
    /// Thread-safe mailbox for passing messages to a designated thread. Dispatch should be called on the owner thread.
    /// </summary>
    internal sealed class Mailbox
    {
        readonly ConcurrentQueue<object> queue = new ConcurrentQueue<object>();
        readonly Dictionary<Type, List<Action<object>>> subscribers = new Dictionary<Type, List<Action<object>>>();
        readonly object subscriberLock = new object();

        public Mailbox(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public void Publish<T>(T message)
        {
            queue.Enqueue(message!);
        }

        public void Subscribe<T>(Action<T> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            Action<object> wrapper = msg => handler((T)msg);

            lock (subscriberLock)
            {
                if (!subscribers.TryGetValue(typeof(T), out var list))
                {
                    list = new List<Action<object>>();
                    subscribers.Add(typeof(T), list);
                }
                list.Add(wrapper);
            }
        }

        public void DispatchAll()
        {
            while (queue.TryDequeue(out var msg))
            {
                if (msg == null) continue;
                var type = msg.GetType();
                if (!subscribers.TryGetValue(type, out var list)) continue;

                for (int i = 0; i < list.Count; i++)
                {
                    list[i](msg);
                }
            }
        }
    }
}
