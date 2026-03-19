using System;
using System.Threading;

namespace Project_1.Messaging
{
    internal sealed class SubscriberList<T>
    {
        readonly object gate = new object();
        Action<T>[] handlers = Array.Empty<Action<T>>();

        public void Add(Action<T> handler)
        {
            lock (gate)
            {
                Action<T>[] next = new Action<T>[handlers.Length + 1];
                Array.Copy(handlers, next, handlers.Length);
                next[handlers.Length] = handler;
                Volatile.Write(ref handlers, next);
            }
        }

        public Action<T>[] Snapshot()
        {
            return Volatile.Read(ref handlers);
        }
    }
}
