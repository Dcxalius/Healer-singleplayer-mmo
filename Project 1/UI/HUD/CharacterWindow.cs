using Microsoft.Xna.Framework;
using Newtonsoft.Json.Bson;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.GameObjects.Unit;
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
    internal class CharacterWindow : Window
    {
        Item[] equiped;

        static RelativeScreenPosition size = RelativeScreenPosition.GetSquareFromY(0.05f);
        static RelativeScreenPosition spacing = RelativeScreenPosition.GetSquareFromY(0.003f);
        static RelativeScreenPosition leftSideTop = spacing;
        static RelativeScreenPosition bottomSideLeft = new RelativeScreenPosition(WindowSize.X / 2 - size.X * 1.5f - spacing.X, (size.Y + spacing.Y) * (int)Equipment.Slot.Hands + spacing.Y);
        static RelativeScreenPosition rightSideTop = new RelativeScreenPosition(WindowSize.X - spacing.X - size.X, spacing.Y);

        public CharacterWindow() : base(new UITexture("WhiteBackground", Color.Turquoise))
        {
            visibleKey = Input.KeyBindManager.KeyListner.Character;
            equiped = new Item[(int)Equipment.Slot.Count];

            RelativeScreenPosition yChange = new RelativeScreenPosition(0, spacing.Y) + new RelativeScreenPosition(0, size.Y);
            CreateItems(Equipment.Slot.Head, Equipment.Slot.Hands, leftSideTop, yChange);
            CreateItems(Equipment.Slot.Belt, Equipment.Slot.Finger2, rightSideTop, yChange);
            RelativeScreenPosition xChange = new RelativeScreenPosition(spacing.X, 0) + new RelativeScreenPosition(size.X, 0);
            CreateItems(Equipment.Slot.MainHand, Equipment.Slot.Ranged, bottomSideLeft, xChange);

            children.AddRange(equiped);
            ToggleVisibilty();
        }

        void CreateItems(Equipment.Slot aStart, Equipment.Slot aEnd, RelativeScreenPosition aStartPos, RelativeScreenPosition aChangeInPos)
        {
            for (int i = (int)aStart ; i <= (int)aEnd; i++)
            {
                Items.Item item = ObjectManager.Player.Equipment.EquipedInSlot((Equipment.Slot)i);
                if (item != null)
                {
                    equiped[i] = new Item(-3, i, true, item.GfxPath, aStartPos + aChangeInPos * (i - (int)aStart) , size);

                }
                else
                {
                    equiped[i] = new Item(-3, i, true, new GfxPath(GfxType.Item, null), aStartPos + aChangeInPos * (i - (int)aStart), size);

                }

            }
        }

        public void SetSlot(Equipment.Slot aSlot)
        {
            equiped[(int)aSlot].AssignItem(ObjectManager.Player.Equipment.EquipedInSlot(aSlot));
        }
    }
}
