using Microsoft.VisualBasic.Devices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.Input;
using Project_1.Items.SubTypes;
using Project_1.Managers;
using Project_1.Textures;
using Project_1.UI.HUD.Inventory;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Boxes;
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
        DescriptorText statSheet;

        void CalculateMaxX(RelativeScreenPosition aScreenSize) => maxX = (aScreenSize.ToAbsoluteScreenPos()).X;
        float maxX;
        readonly RelativeScreenPosition spacingFromItem = RelativeScreenPosition.GetSquareFromX(0.0005f);
        readonly RelativeScreenPosition spacing = RelativeScreenPosition.GetSquareFromX(0.005f);


        public DescriptorBox() : base(new UITexture("GrayBackground", Color.White), RelativeScreenPosition.Zero, RelativeScreenPosition.Zero)
        {
            CalculateMaxX(new RelativeScreenPosition(0.15f));
            descriptedName = new DescriptorText(maxX, "Gloryse", Color.White);
            description = new DescriptorText(maxX, "Gloryse", Color.White);
            statSheet = new DescriptorText(maxX, "Gloryse", Color.White);

            Visible = false;
            AlwaysFullyOnScreen = true;
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
            int spacingNeeded = 3;
            if (item.ItemType == Items.ItemData.ItemType.Equipment || item.ItemType == Items.ItemData.ItemType.Weapon)
            {
                statSheet.Value = (item as Equipment).StatReport.Value;
                spacingNeeded += 1;
            }
            else { statSheet.Value = null; }
            int y = (int)(descriptedName.Offset.Y + description.Offset.Y + statSheet.Offset.Y);
            int x = (int)Math.Max(descriptedName.Offset.X, Math.Max(description.Offset.X, statSheet.Offset.X));
            Resize(new AbsoluteScreenPosition(x, y).ToRelativeScreenPosition() + spacing.OnlyX * 2 + spacing.OnlyY * spacingNeeded);
            Move(aItem.RelativePositionOnScreen - spacingFromItem - RelativeSize);
        }

        void ResetDescriptor()
        {
            Visible = false;
            descriptedName.Value = null;
            description.Value = null;
            statSheet.Value = null;
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
            CalculateMaxX(new RelativeScreenPosition(0.15f));
        }

        public override void Draw(SpriteBatch aBatch)
        {
            base.Draw(aBatch);
            DrawText(aBatch);
        }

        void DrawText(SpriteBatch aBatch)
        {
            AbsoluteScreenPosition pos = ((AbsoluteScreenPosition)AbsolutePos.Location + spacing.ToAbsoluteScreenPos());
            descriptedName.TopLeftDraw(aBatch, pos);
            pos = pos + spacing.ToAbsoluteScreenPos().OnlyY + new AbsoluteScreenPosition(0, (int)descriptedName.Offset.Y);
            description.TopLeftDraw(aBatch, pos);
            pos = pos + spacing.ToAbsoluteScreenPos().OnlyY + new AbsoluteScreenPosition(0, (int)description.Offset.Y);
            statSheet.TopLeftDraw(aBatch, pos);

        }
    }
}
