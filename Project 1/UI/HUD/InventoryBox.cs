using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Bson;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.Input;
using Project_1.Items;
using Project_1.Managers;
using Project_1.Textures;
using Project_1.UI.HUD.Inventory;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Boxes;
using System;

namespace Project_1.UI.HUD
{
    internal class InventoryBox : Box
    {

        //Inventory inventory;

        BagHolderBox bagHolderBox;
        BagBox[] bagBox;
        Label gold;
        Image goldImage;

        const float itemSizeX = 0.06f;
        const float spacingX = 0.01f;

        
        public static RelativeScreenPosition ItemSize => itemSizeInBagSpace;
        static RelativeScreenPosition itemSizeInBagSpace;

        public static AbsoluteScreenPosition AbsItemSize => absItemSize;
        static AbsoluteScreenPosition absItemSize;

        public static RelativeScreenPosition BagBoxSize => bagBoxSize;
        static RelativeScreenPosition bagBoxSize;

        public static RelativeScreenPosition BagBoxSpacing => bagBoxSpacing;
        static RelativeScreenPosition bagBoxSpacing;

        public static AbsoluteScreenPosition AbsBagBoxSpacing => absBagBoxSpacing;
        static AbsoluteScreenPosition absBagBoxSpacing;


        static RelativeScreenPosition spacing = RelativeScreenPosition.GetSquareFromX(spacingX); //Change name

        int columnCount;

        static InventoryBox()
        {
        }

        public InventoryBox(RelativeScreenPosition aPos, RelativeScreenPosition aSize, int aColumnCount) : base(new UITexture("WhiteBackground",new Color(80, 80, 80, 80)), aPos, new RelativeScreenPosition(0.3f, 0.4f))
        {
            InitUIElement(aColumnCount);
            InitStatics();
            InitChildren();
        }

        void InitUIElement(int aColumnCount)
        {
            columnCount = aColumnCount;
            visibleKey = KeyBindManager.KeyListner.Inventory;
            Visible = false;
            Dragable = true;
            alwaysOnScreen = true;
            hudMoveable = false;
        }

        void InitStatics() //TODO: Make this indepentet and called in the static constructor
        {

            bagBoxSize = new RelativeScreenPosition(1 - spacing.X * 2, 0.3f);
            itemSizeInBagSpace = RelativeScreenPosition.GetSquareFromX((1 - (columnCount + 1) * spacing.X) / (float)columnCount, bagBoxSize.ToAbsoluteScreenPos(Size));
            absItemSize = itemSizeInBagSpace.ToAbsoluteScreenPos(bagBoxSize.ToAbsoluteScreenPos(Size));
            bagBoxSpacing = RelativeScreenPosition.GetSquareFromX(spacingX, bagBoxSize.ToAbsoluteScreenPos(Size));
            absBagBoxSpacing = bagBoxSpacing.ToAbsoluteScreenPos(bagBoxSize.ToAbsoluteScreenPos(Size));
        }

        void InitChildren()
        {
            RelativeScreenPosition itemSizeInInventoryScope = AbsItemSize.ToRelativeScreenPosition(Size);
            RelativeScreenPosition bagBoxSpacingInInventoryScope = AbsBagBoxSpacing.ToRelativeScreenPosition(Size);
            RelativeScreenPosition bhPos = new RelativeScreenPosition(bagBoxSpacingInInventoryScope.X, 1f - (itemSizeInInventoryScope.Y + bagBoxSpacingInInventoryScope.Y * 3));
            RelativeScreenPosition bhSize = new RelativeScreenPosition(itemSizeInInventoryScope.X * (Items.Inventory.bagSlots) + bagBoxSpacingInInventoryScope.X * (Items.Inventory.bagSlots + 1), itemSizeInInventoryScope.Y + bagBoxSpacingInInventoryScope.Y * 2);

            bagHolderBox = new BagHolderBox(bhPos, bhSize);
            AddChild(bagHolderBox);


            bagBox = new BagBox[Items.Inventory.bagSlots];
            for (int i = 0; i < bagBox.Length; i++)
            {
                bagBox[i] = new BagBox(i);
            }

            AddChildren(bagBox);

            RelativeScreenPosition imgSize = RelativeScreenPosition.GetSquareFromY(bagHolderBox.RelativeSize.Y / 2, Size);
            goldImage = new Image(new UITexture("Gold", Color.White), RelativeScreenPosition.One - imgSize - bagBoxSpacingInInventoryScope.OnlyX, imgSize);

            RelativeScreenPosition goldSize = new RelativeScreenPosition(1 - imgSize.X - bagBoxSpacingInInventoryScope.X * 2, bhSize.Y);
            gold = new Label("0", RelativeScreenPosition.Zero + RelativeScreenPosition.One.OnlyY - goldSize.OnlyY - bagBoxSpacingInInventoryScope.OnlyY, goldSize, Label.TextAllignment.CentreRight);

            AddChild(gold);
            AddChild(goldImage);
        }

        public override void Resize(RelativeScreenPosition aSize)
        {
            base.Resize(aSize);
        }

        public void SetInventory(Items.Inventory aInventory)
        {
            bagHolderBox.SetBags(aInventory.GetBags(), itemSizeInBagSpace.ToAbsoluteScreenPos(bagBoxSize.ToAbsoluteScreenPos(Size)), bagBoxSpacing.ToAbsoluteScreenPos(bagBoxSize.ToAbsoluteScreenPos(Size)));

            CreateBagBoxes(RelativeSize, aInventory);
            CalculateSize(RelativePos);
        }

        void CreateBagBoxes(RelativeScreenPosition aSize, Items.Inventory aInventory)
        {
            RelativeScreenPosition newBagPos = absBagBoxSpacing.ToRelativeScreenPosition(Size);

            bagBox[0].Move(newBagPos);
            bagBox[0].RefreshBag(aInventory, Items.Inventory.defaultSlots, columnCount);

            newBagPos.Y += bagBox[0].RelativeSize.Y;
            newBagPos.Y += absBagBoxSpacing.ToRelativeScreenPosition().Y;

            for (int i = 1; i < bagBox.Length; i++)
            {
                if (aInventory.Bags[i] == null)
                {
                    bagBox[i].Empty();
                    continue;
                }

                bagBox[i].Move(newBagPos);
                bagBox[i].RefreshBag(aInventory, aInventory.GetBag(i).SlotCount, columnCount);

                newBagPos.Y += bagBox[i].RelativeSize.Y + absBagBoxSpacing.ToRelativeScreenPosition().Y;
            }
        }

        public void RefreshGold(int aGoldAmount) => gold.Text = aGoldAmount.ToString();

        public void RefreshSlot(int aBag, int aSlot, Items.Inventory aInventory)
        {
            if (aBag >= 0)
            {
                bagBox[aBag].RefreshSlot(aSlot);
                return;
            }

            bagHolderBox.RefreshSlot(aSlot);
            if (aInventory.Bags[aSlot] == null)
            {
                bagBox[aSlot].Empty();
                MoveBags(aSlot, aInventory);
                CalculateSize(RelativePos);
                return;
            }

            bagBox[aSlot].RefreshBag(aInventory, aInventory.Bags[aSlot].SlotCount, columnCount);
            MoveBags(aSlot, aInventory);
            CalculateSize(RelativePos);
            return;
        }

        void MoveBags(int aSlot, Items.Inventory aInventory)
        {
            RelativeScreenPosition pos = absBagBoxSpacing.ToRelativeScreenPosition(Size) + bagBox[0].RelativeSize.OnlyY + absBagBoxSpacing.ToRelativeScreenPosition(Size).OnlyY;
            for (int i = 1; i < Items.Inventory.bagSlots; i++)
            {
                if (aInventory.Bags[i] == null) continue;

                bagBox[i].Move(pos);
                pos.Y += bagBox[i].RelativeSize.Y + absBagBoxSpacing.ToRelativeScreenPosition(Size).Y;
            }
        }

        void CalculateSize(RelativeScreenPosition aPos) //TODO: Find better name and make this accept a enum that dictates wheter it grows up and down
        {
            RelativeScreenPosition resize = RelativeScreenPosition.Zero;

            RelativeScreenPosition outerSpacingInScreenSpace = absBagBoxSpacing.ToRelativeScreenPosition(); //I'll be honest, I dont know why this isn't in the context of the inv box
            float s = (absItemSize.ToRelativeScreenPosition() * columnCount + absBagBoxSpacing.ToRelativeScreenPosition() * (columnCount + 3)).X;
            resize.X = s;

            float bagY = outerSpacingInScreenSpace.Y;
            (AbsoluteScreenPosition, AbsoluteScreenPosition)[] oldPosAndSize = new (AbsoluteScreenPosition, AbsoluteScreenPosition)[Items.Inventory.bagSlots]; 
            for (int i = 0; i < bagBox.Length; i++)
            {
                if (bagBox[i].RelativeSize.Y == 0)
                {
                    continue;
                }
                oldPosAndSize[i] = (bagBox[i].RelativePos.ToAbsoluteScreenPos(Size), bagBox[i].Size);
                bagY += bagBox[i].Size.ToRelativeScreenPosition().Y + outerSpacingInScreenSpace.Y;
            }

            resize.Y = bagHolderBox.Size.ToRelativeScreenPosition().Y + outerSpacingInScreenSpace.Y + bagY;

            AbsoluteScreenPosition bagHolderAbsSize = bagHolderBox.Size;
            Resize(resize);

            bagHolderBox.Move(new RelativeScreenPosition(AbsBagBoxSpacing.ToRelativeScreenPosition(Size).X, 1f - (bagHolderAbsSize.ToRelativeScreenPosition(Size).Y + AbsBagBoxSpacing.ToRelativeScreenPosition(Size).Y)));
            bagHolderBox.Resize(bagHolderAbsSize.ToRelativeScreenPosition(Size));
            Move(new RelativeScreenPosition(RelativePos.X, aPos.Y));

            for (int i = 0; i < bagBox.Length; i++)
            {
                bagBox[i].Move(AbsBagBoxSpacing.ToRelativeScreenPosition(Size).OnlyX + oldPosAndSize[i].Item1.ToRelativeScreenPosition(Size).OnlyY);
                bagBox[i].Resize(oldPosAndSize[i].Item2.ToRelativeScreenPosition(Size));

            }

            goldImage.Resize(RelativeScreenPosition.GetSquareFromY(bagHolderBox.RelativeSize.Y / 2, Size));
            //goldImage.Move(RelativeScreenPosition.One - goldImage.RelativeSize - outerSpacingInScreenSpace.OnlyX);
            goldImage.Move(RelativeScreenPosition.One - bagHolderBox.RelativeSize.OnlyY / 2 - goldImage.RelativeSize.OnlyY / 2 - goldImage.RelativeSize.OnlyX - outerSpacingInScreenSpace.OnlyX);
            //


            gold.Resize(new RelativeScreenPosition(1 - goldImage.RelativeSize.X - outerSpacingInScreenSpace.X * 2, bagHolderBox.RelativeSize.Y));
            gold.Move(new RelativeScreenPosition(RelativeScreenPosition.Zero + RelativeScreenPosition.One.OnlyY - gold.RelativeSize.OnlyY - outerSpacingInScreenSpace.OnlyY));
        }

        public override void Rescale()
        {
            //TODO
            spacing = RelativeScreenPosition.GetSquareFromX(spacingX);
            //itemSize = RelativeScreenPosition.GetSquareFromX(itemSizeX).ToAbsoluteScreenPos();
            base.Rescale();
        }
    }
}
