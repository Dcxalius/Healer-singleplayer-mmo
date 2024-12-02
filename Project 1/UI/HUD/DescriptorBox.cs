using Microsoft.VisualBasic.Devices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.GameObjects;
using Project_1.Input;
using Project_1.Managers;
using Project_1.Textures;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Inventory;
using SharpDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.HUD
{
    internal class DescriptorBox : Box
    {
        DescriptorText descriptedName;
        DescriptorText description;

        float maxX;

        public DescriptorBox() : base(new UITexture("GrayBackground", Color.White), Vector2.Zero, Vector2.Zero)
        {
            maxX = Camera.Camera.TransformRelativeToAbsoluteScreenSpace(new Vector2(0.15f)).X;
            descriptedName = new DescriptorText(maxX, "Gloryse", Color.White);
            description = new DescriptorText(maxX, "Gloryse", Color.White);

            ToggleVisibilty();
        }

        public void SetToItem(Item aItem)
        {

            if (aItem == null)
            {
                ResetDescriptor();
                return;
            }

            Items.Item item = aItem.GetActualItem;

            if (item == null)
            {
                ResetDescriptor();
                return;
            }

            ToggleVisibilty();
            descriptedName.Value = item.Name;
            description.Value = item.Description;
            Vector2 spacing = Camera.Camera.GetRelativeXSquare(0.005f);
            int y = (int)(descriptedName.Offset.Y + description.Offset.Y);
            int x = (int)Math.Max(descriptedName.Offset.X, description.Offset.X);
            Resize(Camera.Camera.TransformAbsoluteToRelativeScreenSpace(new Point(x, y)) + spacing * 2 + new Vector2(0, spacing.Y));
            Move(InputManager.GetMousePosRelative() - Camera.Camera.TransformAbsoluteToRelativeScreenSpace(Size));
        }

        void ResetDescriptor()
        {
            ToggleVisibilty();
            descriptedName.Value = null;
            description.Value = null;
            Resize(Vector2.Zero);
            return;
        }

        public override void Update(in UIElement aParent)
        {
            base.Update(aParent);

            if (Visible)
            {
                Move(InputManager.GetMousePosRelative() - Camera.Camera.TransformAbsoluteToRelativeScreenSpace(Size));
            }
        }

        public override void Rescale()
        {
            base.Rescale();
            maxX = Camera.Camera.TransformRelativeToAbsoluteScreenSpace(new Vector2(0.15f)).X;
        }

        public override void Draw(SpriteBatch aBatch)
        {
            base.Draw(aBatch);
            DrawText(aBatch);
        }

        void DrawText(SpriteBatch aBatch)
        {

            Vector2 spacing = Camera.Camera.TransformRelativeToAbsoluteScreenSpace(Camera.Camera.GetRelativeXSquare(0.005f)).ToVector2();
            Vector2 offsetName = new Vector2(0, descriptedName.Offset.Y / 2);
            Vector2 pos = (AbsolutePos.Location.ToVector2() + spacing + offsetName);
            Vector2 offsetDesc = new Vector2(0, description.Offset.Y / 2);
            descriptedName.LeftAllignedDraw(aBatch, pos);
            description.LeftAllignedDraw(aBatch, pos + offsetDesc + new Vector2(0, spacing.Y) * (1 + descriptedName.NameLines));
        }
    }
}
