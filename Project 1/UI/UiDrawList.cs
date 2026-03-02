using Microsoft.Xna.Framework.Graphics;
using Project_1.Managers;
using Project_1.UI.HUD;
using Project_1.UI.HUD.Inventory;
using Project_1.UI.HUD.PlateBoxes;
using Project_1.UI.HUD.SpellBook;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Boxes;
using System;
using System.Collections.Generic;

namespace Project_1.UI
{
    internal sealed class UiElementDrawList
    {
        UIElement[] elements = Array.Empty<UIElement>();
        int elementCount;

        public UiElementDrawList()
        {
        }

        public UiElementDrawList(UIElement[] elements)
        {
            Set(elements, elements?.Length ?? 0);
        }

        public void SetSingle(UIElement element)
        {
            if (element == null)
            {
                elementCount = 0;
                return;
            }

            EnsureCapacity(1);
            elements[0] = element;
            elementCount = 1;
        }

        public void Set(UIElement first, IReadOnlyList<UIElement> tail)
        {
            int tailCount = tail?.Count ?? 0;
            int count = first == null ? tailCount : tailCount + 1;
            EnsureCapacity(count);

            int index = 0;
            if (first != null)
            {
                elements[index++] = first;
            }

            for (int i = 0; i < tailCount; i++)
            {
                elements[index++] = tail[i];
            }

            elementCount = count;
        }

        public void Set(IReadOnlyList<UIElement> source)
        {
            int count = source?.Count ?? 0;
            EnsureCapacity(count);
            for (int i = 0; i < count; i++)
            {
                elements[i] = source[i];
            }
            elementCount = count;
        }

        public void Set(UIElement[] source, int count)
        {
            int safeCount = Math.Clamp(count, 0, source?.Length ?? 0);
            EnsureCapacity(safeCount);
            for (int i = 0; i < safeCount; i++)
            {
                elements[i] = source[i];
            }
            elementCount = safeCount;
        }

        void EnsureCapacity(int count)
        {
            if (count <= elements.Length) return;
            int capacity = Math.Max(count, Math.Max(8, elements.Length * 2));
            elements = new UIElement[capacity];
        }

        public void Draw(SpriteBatch batch)
        {
            ThreadAffinity.AssertMainThread();
            for (int i = 0; i < elementCount; i++)
            {
                elements[i].Draw(batch);
            }
        }
    }

    internal sealed class UiDrawList
    {
        UIElement[] hudElements = Array.Empty<UIElement>();
        int hudElementCount;
        DialogueBox[] dialogueBoxes = Array.Empty<DialogueBox>();
        int dialogueBoxCount;
        DescriptorBox descriptorBox;
        HeldItem heldItem;
        HeldSpell heldSpell;

        public UiDrawList()
        {
        }

        public UiDrawList(UIElement[] hudElements, DialogueBox[] dialogueBoxes, DescriptorBox descriptorBox, HeldItem heldItem, HeldSpell heldSpell)
        {
            Set(hudElements, dialogueBoxes, descriptorBox, heldItem, heldSpell);
        }

        public void Set(IReadOnlyList<UIElement> hudElements, IReadOnlyList<DialogueBox> dialogueBoxes, DescriptorBox descriptorBox, HeldItem heldItem, HeldSpell heldSpell)
        {
            int hudCount = hudElements?.Count ?? 0;
            EnsureHudCapacity(hudCount);
            for (int i = 0; i < hudCount; i++)
            {
                this.hudElements[i] = hudElements[i];
            }
            hudElementCount = hudCount;

            int dialogueCount = dialogueBoxes?.Count ?? 0;
            EnsureDialogueCapacity(dialogueCount);
            for (int i = 0; i < dialogueCount; i++)
            {
                this.dialogueBoxes[i] = dialogueBoxes[i];
            }
            dialogueBoxCount = dialogueCount;

            this.descriptorBox = descriptorBox;
            this.heldItem = heldItem;
            this.heldSpell = heldSpell;
        }

        public void Set(UIElement[] hudElements, DialogueBox[] dialogueBoxes, DescriptorBox descriptorBox, HeldItem heldItem, HeldSpell heldSpell)
        {
            Set((IReadOnlyList<UIElement>)hudElements, (IReadOnlyList<DialogueBox>)dialogueBoxes, descriptorBox, heldItem, heldSpell);
        }

        void EnsureHudCapacity(int count)
        {
            if (count <= hudElements.Length) return;
            int capacity = Math.Max(count, Math.Max(8, hudElements.Length * 2));
            hudElements = new UIElement[capacity];
        }

        void EnsureDialogueCapacity(int count)
        {
            if (count <= dialogueBoxes.Length) return;
            int capacity = Math.Max(count, Math.Max(8, dialogueBoxes.Length * 2));
            dialogueBoxes = new DialogueBox[capacity];
        }

        public void Draw(SpriteBatch batch)
        {
            ThreadAffinity.AssertMainThread();
            for (int i = 0; i < hudElementCount; i++)
            {
                hudElements[i].Draw(batch);
            }

            for (int i = 0; i < dialogueBoxCount; i++)
            {
                dialogueBoxes[i].Draw(batch);
            }

            descriptorBox.Draw(batch);
            heldItem.Draw(batch);
            heldSpell.Draw(batch);
        }
    }

    internal sealed class PlateDrawList
    {
        NamePlateRenderSnapshot[] namePlates = Array.Empty<NamePlateRenderSnapshot>();
        int namePlateCount;
        PlateBoxRenderSnapshot[] plateBoxes = Array.Empty<PlateBoxRenderSnapshot>();
        int plateBoxCount;
        BuffBoxRenderSnapshot[] buffBoxes = Array.Empty<BuffBoxRenderSnapshot>();
        int buffBoxCount;
        BuffRenderSnapshot[] buffEntries = Array.Empty<BuffRenderSnapshot>();
        int buffEntryCount;

        public PlateDrawList()
        {
        }

        public PlateDrawList(
            NamePlateRenderSnapshot[] namePlates,
            PlateBoxRenderSnapshot[] plateBoxes,
            BuffBoxRenderSnapshot[] buffBoxes,
            BuffRenderSnapshot[] buffEntries)
        {
            Set(
                namePlates,
                namePlates?.Length ?? 0,
                plateBoxes,
                plateBoxes?.Length ?? 0,
                buffBoxes,
                buffBoxes?.Length ?? 0,
                buffEntries,
                buffEntries?.Length ?? 0);
        }

        public void Set(
            NamePlateRenderSnapshot[] namePlates,
            int namePlateCount,
            PlateBoxRenderSnapshot[] plateBoxes,
            int plateBoxCount,
            BuffBoxRenderSnapshot[] buffBoxes,
            int buffBoxCount,
            BuffRenderSnapshot[] buffEntries,
            int buffEntryCount)
        {
            int safeNameCount = Math.Clamp(namePlateCount, 0, namePlates?.Length ?? 0);
            EnsureNameCapacity(safeNameCount);
            for (int i = 0; i < safeNameCount; i++)
            {
                this.namePlates[i] = namePlates[i];
            }
            this.namePlateCount = safeNameCount;

            int safePlateCount = Math.Clamp(plateBoxCount, 0, plateBoxes?.Length ?? 0);
            EnsurePlateCapacity(safePlateCount);
            for (int i = 0; i < safePlateCount; i++)
            {
                this.plateBoxes[i] = plateBoxes[i];
            }
            this.plateBoxCount = safePlateCount;

            int safeBuffBoxCount = Math.Clamp(buffBoxCount, 0, buffBoxes?.Length ?? 0);
            EnsureBuffBoxCapacity(safeBuffBoxCount);
            for (int i = 0; i < safeBuffBoxCount; i++)
            {
                this.buffBoxes[i] = buffBoxes[i];
            }
            this.buffBoxCount = safeBuffBoxCount;

            int safeBuffEntryCount = Math.Clamp(buffEntryCount, 0, buffEntries?.Length ?? 0);
            EnsureBuffEntryCapacity(safeBuffEntryCount);
            for (int i = 0; i < safeBuffEntryCount; i++)
            {
                this.buffEntries[i] = buffEntries[i];
            }
            this.buffEntryCount = safeBuffEntryCount;
        }

        void EnsureNameCapacity(int count)
        {
            if (count <= namePlates.Length) return;
            int capacity = Math.Max(count, Math.Max(8, namePlates.Length * 2));
            namePlates = new NamePlateRenderSnapshot[capacity];
        }

        void EnsurePlateCapacity(int count)
        {
            if (count <= plateBoxes.Length) return;
            int capacity = Math.Max(count, Math.Max(8, plateBoxes.Length * 2));
            plateBoxes = new PlateBoxRenderSnapshot[capacity];
        }

        void EnsureBuffBoxCapacity(int count)
        {
            if (count <= buffBoxes.Length) return;
            int capacity = Math.Max(count, Math.Max(8, buffBoxes.Length * 2));
            buffBoxes = new BuffBoxRenderSnapshot[capacity];
        }

        void EnsureBuffEntryCapacity(int count)
        {
            if (count <= buffEntries.Length) return;
            int capacity = Math.Max(count, Math.Max(8, buffEntries.Length * 2));
            buffEntries = new BuffRenderSnapshot[capacity];
        }

        public void Draw(SpriteBatch batch)
        {
            ThreadAffinity.AssertMainThread();
            for (int i = 0; i < namePlateCount; i++)
            {
                namePlates[i].Draw(batch);
            }

            for (int i = 0; i < plateBoxCount; i++)
            {
                plateBoxes[i].Draw(batch);
            }

            for (int i = 0; i < buffBoxCount; i++)
            {
                buffBoxes[i].Draw(batch, buffEntries);
            }
        }
    }

    internal sealed class HudMoveDrawList
    {
        UIElement[] plateBoxes = Array.Empty<UIElement>();
        int plateBoxCount;
        UIElement[] hudElements = Array.Empty<UIElement>();
        int hudElementCount;
        DialogueBox[] dialogueBoxes = Array.Empty<DialogueBox>();
        int dialogueBoxCount;
        SizeChanger sizeChanger;

        public HudMoveDrawList()
        {
        }

        public HudMoveDrawList(UIElement[] plateBoxes, UIElement[] hudElements, DialogueBox[] dialogueBoxes, SizeChanger sizeChanger)
        {
            Set(plateBoxes, plateBoxes?.Length ?? 0, hudElements, dialogueBoxes, sizeChanger);
        }

        public void Set(UIElement[] plateBoxes, int plateBoxCount, IReadOnlyList<UIElement> hudElements, IReadOnlyList<DialogueBox> dialogueBoxes, SizeChanger sizeChanger)
        {
            int safePlateCount = Math.Clamp(plateBoxCount, 0, plateBoxes?.Length ?? 0);
            EnsurePlateCapacity(safePlateCount);
            for (int i = 0; i < safePlateCount; i++)
            {
                this.plateBoxes[i] = plateBoxes[i];
            }
            this.plateBoxCount = safePlateCount;

            int hudCount = hudElements?.Count ?? 0;
            EnsureHudCapacity(hudCount);
            for (int i = 0; i < hudCount; i++)
            {
                this.hudElements[i] = hudElements[i];
            }
            hudElementCount = hudCount;

            int dialogueCount = dialogueBoxes?.Count ?? 0;
            EnsureDialogueCapacity(dialogueCount);
            for (int i = 0; i < dialogueCount; i++)
            {
                this.dialogueBoxes[i] = dialogueBoxes[i];
            }
            dialogueBoxCount = dialogueCount;
            this.sizeChanger = sizeChanger;
        }

        void EnsurePlateCapacity(int count)
        {
            if (count <= plateBoxes.Length) return;
            int capacity = Math.Max(count, Math.Max(8, plateBoxes.Length * 2));
            plateBoxes = new UIElement[capacity];
        }

        void EnsureHudCapacity(int count)
        {
            if (count <= hudElements.Length) return;
            int capacity = Math.Max(count, Math.Max(8, hudElements.Length * 2));
            hudElements = new UIElement[capacity];
        }

        void EnsureDialogueCapacity(int count)
        {
            if (count <= dialogueBoxes.Length) return;
            int capacity = Math.Max(count, Math.Max(8, dialogueBoxes.Length * 2));
            dialogueBoxes = new DialogueBox[capacity];
        }

        public void Draw(SpriteBatch batch)
        {
            ThreadAffinity.AssertMainThread();
            for (int i = 0; i < plateBoxCount; i++)
            {
                plateBoxes[i].HudMovableDraw(batch);
            }

            for (int i = 0; i < hudElementCount; i++)
            {
                hudElements[i].HudMovableDraw(batch);
            }

            for (int i = 0; i < dialogueBoxCount; i++)
            {
                dialogueBoxes[i].HudMovableDraw(batch);
            }

            sizeChanger.Draw(batch);
        }
    }
}
