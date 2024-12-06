using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Spells;
using Project_1.Input;
using Project_1.Managers;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Bars;
using Project_1.UI.UIElements.Inventory;
using Project_1.UI.UIElements.SpellBook;
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
        static PartyPlateBox[] partyPlateBoxes = new PartyPlateBox[4];
        static BuffBox playerBuffBox;
        static BuffBox targetBuffBox;
        static BuffBox[] partyBuffBoxes = new BuffBox[4];

        static InventoryBox inventoryBox;
        static LootBox lootBox;
        static DescriptorBox descriptorBox;

        static SpellBookBox spellBookBox;
        static CastBar playerCastBar;
        static FirstSpellBar firstSpellBar;

        static List<UIElement> hudElements = new List<UIElement>();

        static HeldItem heldItem;
        static HeldSpell heldSpell;



        public static void Init()
        {

            playerPlateBox = new PlayerPlateBox(new RelativeScreenPosition(0.1f, 0.1f), new RelativeScreenPosition(0.2f, 0.1f));
            targetPlateBox = new TargetPlateBox(new RelativeScreenPosition(0.33f, 0.1f), new RelativeScreenPosition(0.2f, 0.1f));
            playerBuffBox = new BuffBox(ObjectManager.Player, BuffBox.FillDirection.TopRightToDown, new RelativeScreenPosition(0.01f, 0.1f), new RelativeScreenPosition(0.08f, 0.1f));
            targetBuffBox = new BuffBox(null, BuffBox.FillDirection.TopRightToDown, new RelativeScreenPosition(0.33f, 0.21f), new RelativeScreenPosition(0.2f, 0.1f));

            lootBox = new LootBox(new RelativeScreenPosition(0.1f, 0.5f), new RelativeScreenPosition(0.4f, 0.4f));
            inventoryBox = new InventoryBox(new RelativeScreenPosition(0.59f, 0.59f), new RelativeScreenPosition(0.4f));
            descriptorBox = new DescriptorBox();

            spellBookBox = new SpellBookBox(new RelativeScreenPosition(0.2f, 0.4f), new RelativeScreenPosition(0.4f, 0.4f));
            firstSpellBar = new FirstSpellBar(10, new RelativeScreenPosition(0.2f, 0.86f), 0.6f);
            playerCastBar = new CastBar(new RelativeScreenPosition(0.1f, 0.203f), new RelativeScreenPosition(0.2f, 0.015f));

            hudElements.Add(playerPlateBox);
            hudElements.Add(targetPlateBox);
            hudElements.Add(playerBuffBox);
            hudElements.Add(targetBuffBox);

            hudElements.Add(lootBox);
            hudElements.Add(inventoryBox);
            hudElements.Add(descriptorBox);

            hudElements.Add(firstSpellBar);
            hudElements.Add(spellBookBox);
            hudElements.Add(playerCastBar);


            heldItem = new HeldItem();
            heldSpell = new HeldSpell();
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

            partyPlateBoxes[openIndex] = new PartyPlateBox(aWalker, new RelativeScreenPosition(0.1f, 0.24f), new RelativeScreenPosition(0.2f, 0.1f)); //TODO: Fix temp values
            partyBuffBoxes[openIndex] = new BuffBox(aWalker, BuffBox.FillDirection.TopRightToDown, new RelativeScreenPosition(0.01f, 0.24f), new RelativeScreenPosition(0.08f, 0.1f));
            partyBuffBoxes[openIndex].AssignBox(aWalker);
            hudElements.Add(partyPlateBoxes[openIndex]);
            hudElements.Add(partyBuffBoxes[openIndex]);
        }


        public static void SetNewTarget() //TODO: Make this use new target as arg
        {
            targetPlateBox.SetEntity();
            targetBuffBox.AssignBox(ObjectManager.Player.Target);
        }

        public static void AddBuff(GameObjects.Spells.Buff aBuff, Entity aEntity)
        {
            if (targetBuffBox.IsThisMine(aEntity))
            {
                targetBuffBox.AddBuff(aBuff);
            }
            if (playerBuffBox.IsThisMine(aEntity))
            {
                playerBuffBox.AddBuff(aBuff);
                return;
            }
            for (int i = 0; i < partyBuffBoxes.Length; i++)
            {
                if (partyBuffBoxes[i] == null) continue;

                if (partyBuffBoxes[i].IsThisMine(aEntity))
                {
                    partyBuffBoxes[i].AddBuff(aBuff);
                    return;
                }
            }
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

        public static void FinishChannel()
        {
            playerCastBar.FinishCast();
        }

        public static void CancelChannel()
        {
            playerCastBar.CancelCast();
        }

        public static void UpdateChannelSpell(float aNewVal)
        {
            playerCastBar.Value = aNewVal;
        }

        public static void ChannelSpell(Spell aSpell)
        {
            playerCastBar.CastSpell(aSpell);
        }

        public static Items.Item GetLootItem(int aSlotInLoot)
        {
            return lootBox.GetItem(aSlotInLoot);
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

        public static void HoldSpell(Spell aSpell, Vector2 aGrabOffset)
        {
            heldSpell.HoldMe(aSpell, aGrabOffset);
        }

        public static void ReleaseSpell()
        {
            heldSpell.ReleaseMe();
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
        }

        public static void Draw(SpriteBatch aBatch)
        {
            for (int i = 0; i < hudElements.Count; i++)
            {
                hudElements[i].Draw(aBatch);
            }

            heldItem.Draw(aBatch);
            heldSpell.Draw(aBatch);
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
