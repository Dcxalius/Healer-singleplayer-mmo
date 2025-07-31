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
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.HUD
{
    internal class DescriptorBox : Box
    {
        Label itemName;
        Label itemDescription;
        Label itemStats;
        Label itemSellPrice;

        Image goldImage;

        float xMax;
        readonly RelativeScreenPosition spacingFromItem = RelativeScreenPosition.GetSquareFromX(0.0005f);
        RelativeScreenPosition spacingInWorld;


        public DescriptorBox() : base(new UITexture("GrayBackground", Color.White), RelativeScreenPosition.Zero, RelativeScreenPosition.Zero)
        {
            xMax = 0.15f;

            itemName = new Label(null, RelativeScreenPosition.Zero, RelativeScreenPosition.Zero, Label.TextAllignment.TopLeft);
            AddChild(itemName);
            itemDescription = new Label(null, RelativeScreenPosition.Zero, RelativeScreenPosition.Zero, Label.TextAllignment.TopLeft);
            AddChild(itemDescription);
            itemStats = new Label(null, RelativeScreenPosition.Zero, RelativeScreenPosition.Zero, Label.TextAllignment.TopLeft);
            AddChild(itemStats);
            itemSellPrice = new Label(null, RelativeScreenPosition.Zero, RelativeScreenPosition.Zero, Label.TextAllignment.CentreRight);
            AddChild(itemSellPrice);

            goldImage = new Image(new UITexture("Gold", Color.White), RelativeScreenPosition.Zero, RelativeScreenPosition.Zero);

            AddChild(goldImage);
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

            SetToItem(item, aItem.RelativePositionOnScreen);
        }

        public void SetToItem(Items.Item aItem, RelativeScreenPosition aPos) //TODO: Should this be public?
        {
            Visible = true;
            spacingInWorld = RelativeScreenPosition.GetSquareFromX(0.005f);

            SetText(aItem, out float ySize, out int spacingNeeded);
            
            Resize(new RelativeScreenPosition(xMax, ySize / Camera.Camera.ScreenRectangle.Height) + spacingInWorld.OnlyX * 2 + spacingInWorld.OnlyY * spacingNeeded);
            Move(aPos - spacingFromItem - RelativeSize);
            RelativeScreenPosition spacingInBox = spacingInWorld.ToAbsoluteScreenPos().ToRelativeScreenPosition(Size);

            RelativeScreenPosition pos = spacingInBox;
            itemName.Resize(new AbsoluteScreenPosition(Size.X, (int)itemName.UnderlyingTextOffset.Y).ToRelativeScreenPosition(Size));
            itemName.Move(pos);
            pos += itemName.RelativeSize.OnlyY + spacingInBox.OnlyY;

            itemDescription.Resize(new AbsoluteScreenPosition(Size.X, (int)itemDescription.UnderlyingTextOffset.Y).ToRelativeScreenPosition(Size));
            itemDescription.Move(pos);
            pos += itemDescription.RelativeSize.OnlyY + spacingInBox.OnlyY;

            if (itemStats.Text != null)
            {
                itemStats.Resize(new AbsoluteScreenPosition(Size.X, (int)itemStats.UnderlyingTextOffset.Y).ToRelativeScreenPosition(Size));
                itemStats.Move(pos);
                pos += itemStats.RelativeSize.OnlyY + spacingInBox.OnlyY;
            }

            if (itemSellPrice.Text != null)
            {
                itemSellPrice.Resize(new RelativeScreenPosition(1 - RelativeScreenPosition.GetSquareFromY(itemSellPrice.UnderlyingTextOffset.Y / Size.Y, Size).X - spacingInBox.X * 2, itemSellPrice.UnderlyingTextOffset.Y / Size.Y));
                itemSellPrice.Move(pos);
                //pos += itemSellPrice.RelativeSize.OnlyY + spacing.OnlyY;

                goldImage.Resize(RelativeScreenPosition.GetSquareFromY(itemSellPrice.RelativeSize.Y, Size));
                goldImage.Move(RelativeScreenPosition.One - goldImage.RelativeSize - spacingInBox); //TODO: Allign image better
            }

        }

        void SetText(Items.Item aItem, out float ySize, out int spacingNeeded)
        {
            ySize = 0;
            spacingNeeded = 1;
            itemName.Text = aItem.Name;
            ySize += itemName.UnderlyingTextOffset.Y;

            spacingNeeded += 1;
            itemDescription.Text = aItem.Description;
            ySize += itemDescription.UnderlyingTextOffset.Y;

            spacingNeeded += 1;

            if (aItem.ItemType == Items.ItemData.ItemType.Equipment || aItem.ItemType == Items.ItemData.ItemType.Weapon)
            {
                itemStats.Text = (aItem as Equipment).StatReport.Value;
                spacingNeeded += 1;
                ySize += itemStats.UnderlyingTextOffset.Y;

            }
            else { itemStats.Text = null; }

            goldImage.Visible = false;
            if (aItem.Cost > 0)
            {
                goldImage.Visible = true;
                itemSellPrice.Text = aItem.SellPrice.ToString();
                spacingNeeded += 1;
                ySize += itemSellPrice.UnderlyingTextOffset.Y;

            }
            else { itemSellPrice.Text = null; }
        }

        

        void ResetDescriptor()
        {
            Visible = false;
            itemName.Text = null;
            itemDescription.Text = null;
            itemStats.Text = null;
            itemSellPrice.Text = null;
            Resize(RelativeScreenPosition.Zero);
            return;
        }
    }
}
