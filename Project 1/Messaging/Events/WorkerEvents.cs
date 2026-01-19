using System;

namespace Project_1.Messaging.Events
{
    internal readonly struct WorkerCallback
    {
        public WorkerCallback(Action action)
        {
            Action = action;
        }

        public Action Action { get; }
    }
}
