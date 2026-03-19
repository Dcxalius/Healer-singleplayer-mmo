using Microsoft.Xna.Framework.Input;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using System;
using System.Diagnostics;

namespace Project_1.Input
{
    internal static partial class InputManager
    {
        static void PublishEscapePressed()
        {
            if (!oldKeyboardState.IsKeyDown(Keys.Escape) && newKeyboardState.IsKeyDown(Keys.Escape))
            {
                MailboxManager.PublishUiEvent(new EscapePressed());
            }
        }

        static void PublishKeyboardSnapshots()
        {
            Keys[] downKeys = newKeyboardState.GetPressedKeys();
            MailboxManager.PublishUiEvent(new KeyboardSnapshot(downKeys));

            int count = (int)KeyBindManager.KeyListner.Count;
            Debug.Assert(count <= KeyBindMaskBitCount, $"KeyBindSnapshot currently supports up to {KeyBindMaskBitCount} key listeners.");

            ulong pressedMask = 0;
            ulong heldMask = 0;
            ulong releasedMask = 0;
            if (!UiTextInputManager.IsActive)
            {
                int max = Math.Min(count, KeyBindMaskBitCount);
                for (int i = 0; i < max; i++)
                {
                    KeyBindManager.KeyListner key = (KeyBindManager.KeyListner)i;
                    ulong bit = 1UL << i;
                    if (KeyBindManager.GetPress(key)) pressedMask |= bit;
                    if (KeyBindManager.GetHold(key)) heldMask |= bit;
                    if (KeyBindManager.GetRelease(key)) releasedMask |= bit;
                }
            }

            MailboxManager.PublishUiEvent(new KeyBindSnapshot(pressedMask, heldMask, releasedMask));
        }
    }
}
