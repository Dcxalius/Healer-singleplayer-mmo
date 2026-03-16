using System;
using System.Collections.Generic;

namespace Project_1.GameObjects.Entities
{
    internal sealed class EntityEventBus
    {
        readonly Dictionary<Type, List<Delegate>> subscribers = new Dictionary<Type, List<Delegate>>();

        public IDisposable Subscribe<T>(Action<T> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            var type = typeof(T);
            if (!subscribers.TryGetValue(type, out var list))
            {
                list = new List<Delegate>();
                subscribers.Add(type, list);
            }

            list.Add(handler);
            return new EventSubscription(() => Unsubscribe(handler));
        }

        public void SubscribeTo<T>(List<IDisposable> subscriptions, Action<T> handler)
        {
            if (subscriptions == null) throw new ArgumentNullException(nameof(subscriptions));
            subscriptions.Add(Subscribe(handler));
        }

        public void Unsubscribe<T>(Action<T> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            var type = typeof(T);
            if (!subscribers.TryGetValue(type, out var list)) return;

            list.Remove(handler);
            if (list.Count == 0) subscribers.Remove(type);
        }

        public void Publish<T>(T message)
        {
            if (!subscribers.TryGetValue(typeof(T), out var list)) return;

            var snapshot = list.ToArray();
            for (int i = 0; i < snapshot.Length; i++)
            {
                ((Action<T>)snapshot[i])(message);
            }
        }

        public void Clear()
        {
            subscribers.Clear();
        }
    }

    internal sealed class EventSubscription : IDisposable
    {
        Action unsubscribe;
        bool disposed;

        public EventSubscription(Action unsubscribe)
        {
            this.unsubscribe = unsubscribe ?? throw new ArgumentNullException(nameof(unsubscribe));
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            unsubscribe();
        }
    }
}
