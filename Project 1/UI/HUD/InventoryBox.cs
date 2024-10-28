using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.GameObjects;
using Project_1.Input;
using Project_1.Items;
using Project_1.Textures;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.HUD
{
    internal class InventoryBox : Box
    {
        public bool visible = false;

        Inventory inventory;

        BagBox bagBox;
        public static Vector2 itemSize = Camera.GetRelativeSquare(0.02f);
        public static Vector2 spacing = Camera.GetRelativeSquare(0.005f);
        public InventoryBox(Vector2 aPos, Vector2 aSize) : base(new UITexture("WhiteBackground",new Color(80, 80, 80, 80)), aPos, aSize)
        {
            inventory = ObjectManager.Player.Inventory;
            bagBox = new BagBox(new Vector2(spacing.X, aSize.Y - (itemSize.Y + spacing.Y * 3)), new Vector2(itemSize.X * (inventory.bagSlots + 1) + spacing.X * (inventory.bagSlots), itemSize.Y + spacing.Y * 2));
            bagBox.SetBags(inventory.GetBags());
            children.Add(bagBox);

        }

        public override void Update(in UIElement aParent)
        {
            base.Update(aParent);

            if (KeyBindManager.GetPress(KeyBindManager.KeyListner.Inventory))
            {
                visible = !visible;
            }

        }

        public override void Rescale()
        {
            base.Rescale();

            itemSize = Camera.GetRelativeSquare(0.015f);
            spacing = Camera.GetRelativeSquare(0.01f);
        }

        public override void Draw(SpriteBatch aBatch)
        {
            if (!visible) return;

            base.Draw(aBatch);
        }
    }
}
