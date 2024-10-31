using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Input;
using Project_1.Textures;
using Project_1.UI.HUD;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.Inventory
{
    internal class Item : GFXButton
    {
        bool isHeld = false;
        bool isEmpty = true;
        public bool IsEmpty { get => isEmpty; }

        public Item(GfxPath aPath, Vector2 aPos, Vector2 aSize) : base(aPath, aPos, aSize, Color.DarkGray)
        {
            if (aPath.Name != null) isEmpty = false;
        }

        protected override void ClickedOnMe(ClickEvent aClick)
        {
            base.ClickedOnMe(aClick);

            if (isEmpty == false)
            {
                HUDManager.HoldItem(this, (InputManager.GetMousePosAbsolute() - AbsolutePos.Location).ToVector2());
            }
        }

        public void AssignItem(Items.Item aItem)
        {
            gfxOnButton = new UITexture(aItem.Gfx, Color.White);
            isEmpty = false;
        }

        public void HoldMe()
        {
            Debug.Assert(!isHeld, "Tried to hold me twice.");


            isHeld = true;
            Color = Color.Gray;
        }

        public void ReleaseMe()
        {
            Debug.Assert(isHeld, "Tried to release me without holding me");
            isHeld = false;
            Color = Color.White;
        }

        public override void HoldReleaseOnMe()
        {
            base.HoldReleaseOnMe();

            if (isEmpty == false)
            {
                HUDManager.ReleaseItem();
            }
        }

        protected override void HoldReleaseAwayFromMe()
        {
            base.HoldReleaseAwayFromMe();

        }

        public override void Draw(SpriteBatch aBatch)
        {

            base.Draw(aBatch);
        }
    }
}
