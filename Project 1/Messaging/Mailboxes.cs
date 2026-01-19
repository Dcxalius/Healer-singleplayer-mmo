using Project_1.Managers;

namespace Project_1.Messaging
{
    /// <summary>
    /// Central access to mailboxes for main, UI, and simulation threads.
    /// </summary>
    internal static class Mailboxes
    {
        public static Mailbox Main { get; } = new Mailbox("Main");
        public static Mailbox Ui { get; } = new Mailbox("UI");
        public static Mailbox Sim { get; } = new Mailbox("Sim");

        public static MailboxStats MainStats => Main.GetStats();
        public static MailboxStats UiStats => Ui.GetStats();
        public static MailboxStats SimStats => Sim.GetStats();

        public static void InitMainThread()
        {
            ThreadAffinity.AssertMainThread();
            _ = Main;
            _ = Ui;
            _ = Sim;
        }
    }
}
