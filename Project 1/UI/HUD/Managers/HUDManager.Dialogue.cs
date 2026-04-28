using Microsoft.Xna.Framework;
using Project_1.Messaging.Events;
using Project_1.Textures;
using Project_1.UI.UIElements.Boxes;
using System.Collections.Generic;
using System;

namespace Project_1.UI.HUD.Managers
{
    internal static partial class HUDManager
    {
        public static void AddDialogueBox(DialogueBox aDialogueBox)
        {
            AssertUiThreadOrMainFallback();
            dialogueBoxes.Add(aDialogueBox);
            InvalidateUi();
        }

        static void AddDialogueBox(DialogueOpened e)
        {
            AssertUiThreadOrMainFallback();
            UITexture background = e.Background == null ? UITexture.Null : new UITexture(e.Background, Color.White);
            DialogueBox box = new DialogueBox(null, e.Text, e.TextColor, e.Location.ToDialogueBoxLocation(), e.Pauses.ToDialogueBoxPause(), new List<Action>(), background, e.Pos, e.Size, e.CloseText);
            dialogueBoxes.Add(box);
            InvalidateUi();
        }

        public static void RemoveDialogueBox(int dialogueBoxId)
        {
            AssertUiThreadOrMainFallback();
            DialogueBox toRemove = dialogueBoxes.Find(x => x.UiElementId == dialogueBoxId);
            if (toRemove == null) return;
            dialogueBoxes.Remove(toRemove);
            InvalidateUi();
        }
    }
}
