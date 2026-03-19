using Project_1.Managers;
using Project_1.Messaging;
using Project_1.Messaging.Events;

namespace Project_1.Input
{
    internal static partial class UiInputBridge
    {
        static void HandleKeyboardSnapshot(KeyboardSnapshot snapshot)
        {
            ThreadAffinity.AssertUiThread();
            UiKeyboardStateCache.Update(snapshot);
            MailboxManager.PublishSimCommand(snapshot);
        }

        static void HandleKeyBindSnapshot(KeyBindSnapshot snapshot)
        {
            ThreadAffinity.AssertUiThread();
            UiKeyBindStateCache.Update(snapshot);
            MailboxManager.PublishSimCommand(snapshot);
            if (UiTextInputManager.IsActive)
            {
                return;
            }

            MailboxManager.PublishSimCommand(new PlayerMovementRequested(
                UiKeyBindStateCache.GetHold(KeyBindManager.KeyListner.MoveCharacterLeft),
                UiKeyBindStateCache.GetHold(KeyBindManager.KeyListner.MoveCharacterRight),
                UiKeyBindStateCache.GetHold(KeyBindManager.KeyListner.MoveCharacterUp),
                UiKeyBindStateCache.GetHold(KeyBindManager.KeyListner.MoveCharacterDown)));
        }

        static void HandleMouseSnapshot(MouseSnapshot snapshot)
        {
            ThreadAffinity.AssertUiThread();
            UiMouseStateCache.Update(snapshot);
            MailboxManager.PublishSimCommand(snapshot);
        }
    }
}
