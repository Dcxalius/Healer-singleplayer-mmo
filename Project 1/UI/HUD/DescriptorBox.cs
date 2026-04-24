using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Input;
using Project_1.Managers;
using Project_1.Messaging.Events;
using Project_1.Textures;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Boxes;
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
        Label itemBonusStats;
        Label itemSellPrice;

        SquareImage goldImage;

        float xMax;
        readonly RelativeScreenPosition spacingFromItem = RelativeScreenPosition.GetSquareFromX(0.0005f);
        RelativeScreenPosition spacingInWorld;


        public DescriptorBox(UIElement aParent) : base(aParent, new UITexture("GrayBackground", Color.White), RelativeScreenPosition.Zero, RelativeScreenPosition.Zero)
        {
            xMax = 0.15f;

            itemName = new Label(this, RelativeScreenPosition.Zero, RelativeScreenPosition.Zero, Label.TextAllignment.TopLeft);
            itemDescription = new Label(this, RelativeScreenPosition.Zero, RelativeScreenPosition.Zero, Label.TextAllignment.TopLeft);
            itemStats = new Label(this, RelativeScreenPosition.Zero, RelativeScreenPosition.Zero, Label.TextAllignment.TopLeft);
            itemBonusStats = new Label(this, RelativeScreenPosition.Zero, RelativeScreenPosition.Zero, Label.TextAllignment.TopLeft, Color.LimeGreen);
            itemSellPrice = new Label(this, RelativeScreenPosition.Zero, RelativeScreenPosition.Zero, Label.TextAllignment.CentreRight);

            goldImage = new SquareImage(this, new UITexture("Gold", Color.White), RelativeScreenPosition.Zero, RelativeScreenPosition.Zero);

            Visible = false;
            AlwaysFullyOnScreen = true;
        }

        public void SetToSnapshot(in ItemDescriptorSnapshot snapshot, RelativeScreenPosition aPos)
        {
            Visible = true;
            spacingInWorld = RelativeScreenPosition.GetSquareFromX(0.005f);

            SetText(snapshot, out float ySize, out int spacingNeeded);
            
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

            if (itemBonusStats.Text != null)
            {
                itemBonusStats.Resize(new AbsoluteScreenPosition(Size.X, (int)itemBonusStats.UnderlyingTextOffset.Y).ToRelativeScreenPosition(Size));
                itemBonusStats.Move(pos);
                pos += itemBonusStats.RelativeSize.OnlyY + spacingInBox.OnlyY;
            }

            if (itemSellPrice.Text != null)
            {
                goldImage.Resize(new RelativeScreenPosition(0, itemSellPrice.UnderlyingTextOffset.Y / Size.Y));
                itemSellPrice.Resize(new RelativeScreenPosition(1 - goldImage.RelativeSize.X - spacingInBox.X * 2, itemSellPrice.UnderlyingTextOffset.Y / Size.Y));
                itemSellPrice.Move(pos);
                //pos += itemSellPrice.RelativeSize.OnlyY + spacing.OnlyY;

                goldImage.Move(RelativeScreenPosition.One - goldImage.RelativeSize - spacingInBox); //TODO: Allign image better
            }

        }

        public void SetToSnapshot(in SpellDescriptorSnapshot snapshot, RelativeScreenPosition aPos)
        {
            Visible = true;
            spacingInWorld = RelativeScreenPosition.GetSquareFromX(0.005f);

            SetText(snapshot, out float ySize, out int spacingNeeded);

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
            }
        }

        public void Clear()
        {
            ResetDescriptor();
        }

        void SetText(in ItemDescriptorSnapshot snapshot, out float ySize, out int spacingNeeded)
        {
            ySize = 0;
            spacingNeeded = 1;
            itemName.Text = snapshot.Name;
            ySize += itemName.UnderlyingTextOffset.Y;

            spacingNeeded += 1;
            itemDescription.Text = snapshot.Description;
            ySize += itemDescription.UnderlyingTextOffset.Y;

            spacingNeeded += 1;

            if (snapshot.HasStatReport)
            {
                itemStats.Text = snapshot.StatReport;
                spacingNeeded += 1;
                ySize += itemStats.UnderlyingTextOffset.Y;

            }
            else { itemStats.Text = null; }

            if (snapshot.HasBonusStatReport)
            {
                itemBonusStats.Text = snapshot.BonusStatReport;
                spacingNeeded += 1;
                ySize += itemBonusStats.UnderlyingTextOffset.Y;
            }
            else { itemBonusStats.Text = null; }

            goldImage.Visible = false;
            if (snapshot.HasSellPrice)
            {
                goldImage.Visible = true;
                itemSellPrice.Text = snapshot.SellPrice.ToString();
                spacingNeeded += 1;
                ySize += itemSellPrice.UnderlyingTextOffset.Y;

            }
            else { itemSellPrice.Text = null; }
        }

        void SetText(in SpellDescriptorSnapshot snapshot, out float ySize, out int spacingNeeded)
        {
            ySize = 0;
            spacingNeeded = 1;
            itemName.Text = snapshot.Name;
            ySize += itemName.UnderlyingTextOffset.Y;

            spacingNeeded += 1;
            itemDescription.Text = snapshot.Description;
            ySize += itemDescription.UnderlyingTextOffset.Y;

            if (snapshot.HasStatReport)
            {
                spacingNeeded += 1;
                itemStats.Text = snapshot.StatReport;
                ySize += itemStats.UnderlyingTextOffset.Y;
            }
            else
            {
                itemStats.Text = null;
            }

            itemBonusStats.Text = null;
            itemSellPrice.Text = null;
            goldImage.Visible = false;
        }

        

        void ResetDescriptor()
        {
            Visible = false;
            itemName.Text = null;
            itemDescription.Text = null;
            itemStats.Text = null;
            itemBonusStats.Text = null;
            itemSellPrice.Text = null;
            Resize(RelativeScreenPosition.Zero);
            return;
        }
    }
}
