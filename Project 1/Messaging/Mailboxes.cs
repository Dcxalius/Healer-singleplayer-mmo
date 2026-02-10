using Project_1.Managers;
using System;
using System.Collections.Concurrent;

namespace Project_1.Messaging
{
    /// <summary>
    /// Central access to mailboxes for main, UI, and simulation threads.
    /// </summary>
    internal static class Mailboxes
    {
        static readonly ConcurrentDictionary<Type, byte> simCommandTypes = new ConcurrentDictionary<Type, byte>();

        public static Mailbox Main { get; } = new Mailbox("Main");
        public static Mailbox Ui { get; } = new Mailbox("UI");
        public static Mailbox Sim { get; } = new Mailbox("Sim");

        public static MailboxStats MainStats => Main.GetStats();
        public static MailboxStats UiStats => Ui.GetStats();
        public static MailboxStats SimStats => Sim.GetStats();

        public static void RegisterSimCommandType<T>()
        {
            simCommandTypes.TryAdd(typeof(T), 0);
        }

        public static void PublishMainEvent<T>(in T message)
        {
            AssertNotSimCommand<T>(nameof(PublishMainEvent));
            Main.Publish(message);
        }

        public static void PublishUiEvent<T>(in T message)
        {
            AssertNotSimCommand<T>(nameof(PublishUiEvent));
            Ui.Publish(message);
        }

        public static void PublishSimCommand<T>(in T message)
        {
            Sim.Publish(message);
        }

        static void AssertNotSimCommand<T>(string publishApi)
        {
            if (!simCommandTypes.ContainsKey(typeof(T))) return;
            throw new InvalidOperationException($"{publishApi} cannot publish registered sim-command '{typeof(T).Name}'. Use {nameof(PublishSimCommand)}.");
        }

        public static void InitMainThread()
        {
            ThreadAffinity.AssertMainThread();
            _ = Main;
            _ = Ui;
            _ = Sim;
        }
    }
}
