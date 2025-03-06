using Microsoft.VisualBasic.Devices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.Input;
using Project_1.Managers;
using Project_1.Textures;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Boxes;
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
        readonly RelativeScreenPosition spacingFromItem = RelativeScreenPosition.GetSquareFromX(0.0005f);

        public DescriptorBox() : base(new UITexture("GrayBackground", Color.White), RelativeScreenPosition.Zero, RelativeScreenPosition.Zero)
        {
            maxX = (new RelativeScreenPosition(0.15f).ToAbsoluteScreenPos()).X;
            descriptedName = new DescriptorText(maxX, "Gloryse", Color.White);
            description = new DescriptorText(maxX, "Gloryse", Color.White);

            Visible = false;
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

            Visible = true;
            descriptedName.Value = item.Name;
            description.Value = item.Description;
            RelativeScreenPosition spacing = RelativeScreenPosition.GetSquareFromX(0.005f);
            int y = (int)(descriptedName.Offset.Y + description.Offset.Y);
            int x = (int)Math.Max(descriptedName.Offset.X, description.Offset.X);
            Resize(new AbsoluteScreenPosition(x, y).ToRelativeScreenPosition() + spacing * 2 + new RelativeScreenPosition(0, spacing.Y));
            Move(aItem.RelativePositionOnScreen - spacingFromItem - RelativeSize);
        }

        void ResetDescriptor()
        {
            Visible = false;
            descriptedName.Value = null;
            description.Value = null;
            Resize(RelativeScreenPosition.Zero);
            return;
        }

        public override void Update()
        {
            base.Update();

        }

        public override void Rescale()
        {
            base.Rescale();
            maxX = (new RelativeScreenPosition(0.15f).ToAbsoluteScreenPos()).X;
        }

        public override void Draw(SpriteBatch aBatch)
        {
            base.Draw(aBatch);
            DrawText(aBatch);
        }

        void DrawText(SpriteBatch aBatch)
        {

            AbsoluteScreenPosition spacing = (RelativeScreenPosition.GetSquareFromX(0.005f).ToAbsoluteScreenPos());
            AbsoluteScreenPosition offsetName = new AbsoluteScreenPosition(0, (int)(descriptedName.Offset.Y / 2f));
            AbsoluteScreenPosition pos = ((AbsoluteScreenPosition)AbsolutePos.Location + spacing + offsetName);
            AbsoluteScreenPosition offsetDesc = new AbsoluteScreenPosition(0, (int)(description.Offset.Y / 2));
            descriptedName.CentreLeftDraw(aBatch, pos);
            description.CentreLeftDraw(aBatch, pos + offsetDesc + new AbsoluteScreenPosition(0, spacing.Y) * (1 + descriptedName.NameLines));
        }
    }
}
