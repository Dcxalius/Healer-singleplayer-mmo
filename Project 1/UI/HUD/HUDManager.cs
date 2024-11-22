using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Spells;
using Project_1.Input;
using Project_1.Managers;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Inventory;
using SharpDX.MediaFoundation.DirectX;
using SharpDX.XAudio2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.HUD
{
    internal static class HUDManager
    {
        static PlayerPlateBox playerPlateBox;
        static TargetPlateBox targetPlateBox;
        static FirstSpellBar firstSpellBar;
        static InventoryBox inventoryBox;
        static PartyPlateBox[] partyPlateBoxes = new PartyPlateBox[4];
        static LootBox lootBox;
        static DescriptorBox descriptorBox;
        static SpellBookBox spellBookBox;

        static List<UIElement> hudElements = new List<UIElement>();

        static HeldItem heldItem;

        public static void SetNewTarget(Entity aEntity)
        {
            targetPlateBox.SetEntity(aEntity);
        }


        public static void Init()
        {
            playerPlateBox = new PlayerPlateBox(new Vector2(0.1f, 0.1f), new Vector2(0.2f, 0.1f));
            targetPlateBox = new TargetPlateBox(new Vector2(0.33f, 0.1f), new Vector2(0.2f, 0.1f));
            firstSpellBar = new FirstSpellBar(10, new Vector2(0.2f, 0.86f), 0.6f);
            inventoryBox = new InventoryBox(new Vector2(0.59f, 0.59f), new Vector2(0.4f));
            lootBox = new LootBox(new Vector2(0.1f, 0.5f), new Vector2(0.4f, 0.4f));
            spellBookBox = new SpellBookBox(new Vector2(0.2f, 0.4f), new Vector2(0.4f, 0.4f));
            descriptorBox = new DescriptorBox();

            hudElements.Add(playerPlateBox);
            hudElements.Add(targetPlateBox);
            hudElements.Add(firstSpellBar);
            hudElements.Add(inventoryBox);
            hudElements.Add(lootBox);
            hudElements.Add(descriptorBox);
            hudElements.Add(spellBookBox);

            heldItem = new HeldItem();
        }

        public static void AddWalkerToParty(Walker aWalker)
        {
            int openIndex = -1;
            for (int i = 0; i < partyPlateBoxes.Length; i++)
            {
                if (partyPlateBoxes[i] == null)
                {
                    openIndex = i;
                    break;
                }
            }

            if (openIndex == -1)
            {
                DebugManager.Print(typeof(HUDManager), "Tried to add to full party.");
                return;
            }

            partyPlateBoxes[openIndex] = new PartyPlateBox(aWalker, new Vector2(0.1f, 0.24f), new Vector2(0.2f, 0.1f));
            hudElements.Add(partyPlateBoxes[openIndex]);
        }

        public static void AddSpellToSpellBook(Spell aSpell)
        {
            spellBookBox.AssignSpell(aSpell);
        }

        public static void RefreshInventorySlot(int aBag, int aSlot)
        {
            inventoryBox.RefreshSlot(aBag, aSlot);
        }

        public static void RefreshInventorySlot((int, int) aBagAndSlot)
        {
            RefreshInventorySlot(aBagAndSlot.Item1, aBagAndSlot.Item2);
        }

        public static void SetDescriptorBox(Item aItem)
        {
            descriptorBox.SetToItem(aItem);
        }


        //public static void RefreshInventory()
        //{
        //    inventoryBox.RefreshAllBags();
        //}

        //public static void RefeshSlot(int aBag, int aSlot)
        //{
        //    inventoryBox.RefreshSlot(aBag, aSlot);
        //}

        public static Items.Item LootItem(int aSlotInLoot)
        {
            return lootBox.LootItem(aSlotInLoot);
        }

        public static void ReduceLootItem(int aSlotInLoot, int aCount)
        {
            lootBox.ReduceItem(aSlotInLoot, aCount);
        }

        public static void HoldItem(Item aItem, Vector2 aGrabOffset)
        {
            heldItem.HoldItem(aItem, aGrabOffset);
        }

        public static void ReleaseItem()
        {
            heldItem.ReleaseMe();
        }

        public static void Loot(Corpse aCorpse)
        {
            lootBox.Loot(aCorpse);
        }
        

        public static void AddWalkerToControl(Walker aWalker)
        {
            for (int i = 0; i < partyPlateBoxes.Length; i++)
            {
                if (partyPlateBoxes[i].BelongsTo(aWalker))
                {
                    partyPlateBoxes[i].VisibleBorder = true;
                    break;
                }
            
            }
        }

        public static void RemoveWalkersFromControl(Walker[] aWalkers)
        {
            for (int i = 0; i < aWalkers.Length; i++)
            {
                for (int j = 0; j < partyPlateBoxes.Length; j++)
                {

                    if (partyPlateBoxes[j].BelongsTo(aWalkers[i]))
                    {
                        partyPlateBoxes[j].VisibleBorder = false;
                        break;
                    }
                }
            }
        }

        public static bool Click(ClickEvent aClickEvent)
        {
            for (int i = hudElements.Count - 1; i >= 0; i--)
            {
                if (hudElements[i].ClickedOn(aClickEvent))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool Release(ReleaseEvent aReleaseEvent)
        {
            for (int i = hudElements.Count - 1; i >= 0; i--)
            {
                if (hudElements[i].ReleasedOn(aReleaseEvent))
                {
                    return true;
                }
            }
            return false;
        }

        public static void Update()
        {
            for (int i = 0; i < hudElements.Count; i++)
            {
                hudElements[i].Update(null);
            }

            heldItem.Update();
        }

        public static void Draw(SpriteBatch aBatch)
        {
            for (int i = 0; i < hudElements.Count; i++)
            {
                hudElements[i].Draw(aBatch);
            }

            heldItem.Draw(aBatch);
        }

        public static void Rescale()
        {
            for (int i = 0; i < hudElements.Count; i++)
            {
                hudElements[i].Rescale();
            }
        }

    }
}
