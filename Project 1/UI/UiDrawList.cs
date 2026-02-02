using Microsoft.Xna.Framework.Graphics;
using Project_1.Managers;
using Project_1.UI.HUD;
using Project_1.UI.HUD.Inventory;
using Project_1.UI.HUD.SpellBook;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Boxes;
using Project_1.GameObjects.Entities.Friendlies.Players;

namespace Project_1.UI
{
    internal sealed class UiElementDrawList
    {
        readonly UIElement[] elements;

        public UiElementDrawList(UIElement[] elements)
        {
            this.elements = elements;
        }

        public void Draw(SpriteBatch batch)
        {
            ThreadAffinity.AssertMainThread();
            for (int i = 0; i < elements.Length; i++)
            {
                elements[i].Draw(batch);
            }
        }
    }

    internal sealed class UiDrawList
    {
        readonly UIElement[] hudElements;
        readonly DialogueBox[] dialogueBoxes;
        readonly DescriptorBox descriptorBox;
        readonly HeldItem heldItem;
        readonly HeldSpell heldSpell;

        public UiDrawList(UIElement[] hudElements, DialogueBox[] dialogueBoxes, DescriptorBox descriptorBox, HeldItem heldItem, HeldSpell heldSpell)
        {
            this.hudElements = hudElements;
            this.dialogueBoxes = dialogueBoxes;
            this.descriptorBox = descriptorBox;
            this.heldItem = heldItem;
            this.heldSpell = heldSpell;
        }

        public void Draw(SpriteBatch batch)
        {
            ThreadAffinity.AssertMainThread();
            for (int i = 0; i < hudElements.Length; i++)
            {
                hudElements[i].Draw(batch);
            }

            for (int i = 0; i < dialogueBoxes.Length; i++)
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
        readonly NamePlate[] namePlates;
        readonly UIElement[] plateBoxes;

        public PlateDrawList(NamePlate[] namePlates, UIElement[] plateBoxes)
        {
            this.namePlates = namePlates;
            this.plateBoxes = plateBoxes;
        }

        public void Draw(SpriteBatch batch)
        {
            ThreadAffinity.AssertMainThread();
            for (int i = 0; i < namePlates.Length; i++)
            {
                namePlates[i].Draw(batch);
            }

            for (int i = 0; i < plateBoxes.Length; i++)
            {
                plateBoxes[i].Draw(batch);
            }
        }
    }

    internal sealed class HudMoveDrawList
    {
        readonly UIElement[] plateBoxes;
        readonly UIElement[] hudElements;
        readonly DialogueBox[] dialogueBoxes;
        readonly SizeChanger sizeChanger;

        public HudMoveDrawList(UIElement[] plateBoxes, UIElement[] hudElements, DialogueBox[] dialogueBoxes, SizeChanger sizeChanger)
        {
            this.plateBoxes = plateBoxes;
            this.hudElements = hudElements;
            this.dialogueBoxes = dialogueBoxes;
            this.sizeChanger = sizeChanger;
        }

        public void Draw(SpriteBatch batch)
        {
            ThreadAffinity.AssertMainThread();
            for (int i = 0; i < plateBoxes.Length; i++)
            {
                plateBoxes[i].HudMovableDraw(batch);
            }

            for (int i = 0; i < hudElements.Length; i++)
            {
                hudElements[i].HudMovableDraw(batch);
            }

            for (int i = 0; i < dialogueBoxes.Length; i++)
            {
                dialogueBoxes[i].HudMovableDraw(batch);
            }

            sizeChanger.Draw(batch);
        }
    }
}
