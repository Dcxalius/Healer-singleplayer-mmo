using Microsoft.Xna.Framework.Graphics;
using Project_1.UI.HUD;
using Project_1.UI.HUD.Inventory;
using Project_1.UI.HUD.SpellBook;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Boxes;
using Project_1.GameObjects.Entities.Friendlies.Players;

namespace Project_1.UI
{
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
}
