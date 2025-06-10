using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Entities.Npcs;
using Project_1.GameObjects.Entities.Players;
using Project_1.GameObjects.Spells;
using Project_1.GameObjects.Unit;
using Project_1.Input;
using Project_1.Items;
using Project_1.Managers;
using Project_1.Textures;
using Project_1.UI.HUD.Inventory;
using Project_1.UI.HUD.PlateBoxes;
using Project_1.UI.HUD.SpellBook;
using Project_1.UI.HUD.Windows;
using Project_1.UI.HUD.Windows.Gossip;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Bars;
using Project_1.UI.UIElements.Boxes;
using Project_1.UI.UIElements.Buttons;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Item = Project_1.UI.HUD.Inventory.Item;

namespace Project_1.UI.HUD.Managers
{
    internal static class HUDManager
    {
        public static PlateBoxHandler plateBoxHandler;
        public static NamePlateHandler namePlateHandler;
        public static WindowHandler windowHandler;
        static List<UIElement> hudElements;



        static InventoryBox inventoryBox;
        static LootBox lootBox;
        static DescriptorBox descriptorBox;

        static CastBar playerCastBar;
        static FirstSpellBar firstSpellBar;



        static HeldItem heldItem;
        static HeldSpell heldSpell;

        static List<DialogueBox> dialogueBoxes;

        static SizeChanger sizeChanger;

        static List<(string, RelativeScreenPosition, RelativeScreenPosition)> LoadedSettings;

        public static bool HudMoving => hudMoving;
        static bool hudMoving;
        public static void Init()
        {
            plateBoxHandler = new PlateBoxHandler();
            namePlateHandler = new NamePlateHandler();
            windowHandler = new WindowHandler();

            hudMoving = false;
            ImportSettings();

            hudElements = new List<UIElement>();
            plateBoxHandler.InitPlateBoxes(LoadedSettings);

            lootBox = new LootBox(new RelativeScreenPosition(0.1f, 0.5f), new RelativeScreenPosition(0.4f, 0.4f));
            hudElements.Add(lootBox);
            inventoryBox = new InventoryBox(new RelativeScreenPosition(0.59f, 0.60f), new RelativeScreenPosition(0.4f), 16);
            hudElements.Add(inventoryBox);

            descriptorBox = new DescriptorBox(); //Shouldnt be in elements for now

            sizeChanger = new SizeChanger();

            windowHandler.InitWindows(ref hudElements);

            var loaded = LoadedSettings.Find(x => x.Item1 == typeof(FirstSpellBar).Name);
            firstSpellBar = new FirstSpellBar(10, loaded.Item2, loaded.Item3.X);
            hudElements.Add(firstSpellBar);
            loaded = LoadedSettings.Find(x => x.Item1 == typeof(CastBar).Name);
            playerCastBar = new CastBar(loaded.Item2, loaded.Item3);
            hudElements.Add(playerCastBar);

            heldItem = new HeldItem();
            heldSpell = new HeldSpell();

            dialogueBoxes = new List<DialogueBox>();

        }

        static void ImportSettings()
        {
            if (File.Exists(SaveManager.HudSettings))
            {
                string json = File.ReadAllText(SaveManager.HudSettings);
                LoadedSettings = SaveManager.ImportData<List<(string, RelativeScreenPosition, RelativeScreenPosition)>>(json);
            }
            else
            {
                string json = File.ReadAllText(SaveManager.DefaultHudSettings);
                LoadedSettings = SaveManager.ImportData<List<(string, RelativeScreenPosition, RelativeScreenPosition)>>(json);
            }
        }

        public static void Update()
        {
            namePlateHandler.Update();
            plateBoxHandler.Update();

            for (int i = 0; i < hudElements.Count; i++)
            {
                hudElements[i].Update();
            }

            for (int i = 0; i < dialogueBoxes.Count; i++)
            {
                dialogueBoxes[i].Update();
            }
        }


        public static void Rescale()
        {
            namePlateHandler.Rescale();
            plateBoxHandler.Rescale();

            for (int i = 0; i < hudElements.Count; i++)
            {
                hudElements[i].Rescale();
            }

            for (int i = 0; i < dialogueBoxes.Count; i++)
            {
                hudElements[i].Rescale();
            }
        }

        #region HudChanging

        public static void SetSizeChanger(UIElement aUIElement)
        {
            if (!sizeChanger.Active) return;
            sizeChanger.SetElement(aUIElement);
        }

        public static void DisableHudMoveable() => SetHudMoveable(false);

        public static void SetHudMoveable(bool aSet) //TODO: Should this really be done this way and not by a bool flag in hud manager?
        {
            plateBoxHandler.SetHudMovable(aSet);

            for (int i = 0; i < hudElements.Count; i++)
            {
                hudElements[i].SetHudMoveable(aSet);
            }

            for (int i = 0; i < dialogueBoxes.Count; i++)
            {
                dialogueBoxes[i].SetHudMoveable(aSet);
            }
        }

        public static void ResetHudMoveable()
        {
            plateBoxHandler.ResetHudMovable();

            for (int i = 0; i < hudElements.Count; i++)
            {
                hudElements[i].ResetHudMoveable();
            }

            for (int i = 0; i < dialogueBoxes.Count; i++)
            {
                dialogueBoxes[i].ResetHudMoveable();
            }
        }

        public static void HudMoveableDraw(SpriteBatch aBatch)
        {
            plateBoxHandler.HudMovableDraw(aBatch);

            for (int i = 0; i < hudElements.Count; i++)
            {
                hudElements[i].HudMovableDraw(aBatch);
            }

            for (int i = 0; i < dialogueBoxes.Count; i++)
            {
                dialogueBoxes[i].HudMovableDraw(aBatch);
            }

            sizeChanger.Draw(aBatch);
        }

        public static void HudMovableUpdate()
        {
            plateBoxHandler.HudMovableUpdate();

            for (int i = 0; i < hudElements.Count; i++)
            {
                hudElements[i].Update();
            }

            for (int i = 0; i < dialogueBoxes.Count; i++)
            {
                dialogueBoxes[i].Update();
            }

            sizeChanger.Update();
        }

        public static void ChangeSizes() => sizeChanger.Active = true;

        public static void DisableSizeChanges() => sizeChanger.Active = false;

        public static void Save()
        {
            List<(string, RelativeScreenPosition, RelativeScreenPosition)> saveables = new List<(string, RelativeScreenPosition, RelativeScreenPosition)>();
            plateBoxHandler.Save(ref saveables);

            for (int i = 0; i < hudElements.Count; i++)
            {
                if (!hudElements[i].HudMoveable) continue;
                saveables.Add(hudElements[i].Save);
            }

            for (int i = 0; i < dialogueBoxes.Count; i++)
            {
                if (!dialogueBoxes[i].HudMoveable) continue;
                saveables.Add(dialogueBoxes[i].Save);
            }

            SaveManager.ExportData(SaveManager.HudSettings, saveables);
        }
        #endregion



        #region Dialogue
        public static void AddDialogueBox(DialogueBox aDialogueBox) => dialogueBoxes.Add(aDialogueBox);

        public static void RemoveDialogueBox(DialogueBox aDialogueBox) => dialogueBoxes.Remove(aDialogueBox);
        #endregion





        #region Inventory
        public static void SetInventory(Items.Inventory aInventory) => inventoryBox.SetInventory(aInventory);
        public static void RefreshInventorySlot(int aBag, int aSlot, Items.Inventory aInventory) => inventoryBox.RefreshSlot(aBag, aSlot, aInventory);
        public static void RefreshInventorySlot((int, int) aBagAndSlot, Items.Inventory aInventory) => RefreshInventorySlot(aBagAndSlot.Item1, aBagAndSlot.Item2, aInventory);

        public static void SetDescriptorBox(Item aItem) => descriptorBox.SetToItem(aItem);

        #endregion

        

        #region Spell
        public static void HoldSpell(Spell aSpell, AbsoluteScreenPosition aGrabOffset) => heldSpell.HoldMe(aSpell, aGrabOffset);
        public static void ReleaseSpell() => heldSpell.ReleaseMe();

        public static void FinishChannel() => playerCastBar.FinishCast();
        public static void CancelChannel() => playerCastBar.CancelCast();
        public static void UpdateChannelSpell(float aNewVal) => playerCastBar.Value = aNewVal;
        public static void ChannelSpell(Spell aSpell) => playerCastBar.CastSpell(aSpell);

        

        public static void LoadSpellBar(Spell[] aSpells) => firstSpellBar.LoadBar(aSpells);
        public static string[] SaveSpellBar => firstSpellBar.SaveBar();


        #endregion

        #region Loot
        public static Items.Item GetLootItem(int aSlotInLoot) => lootBox.GetItem(aSlotInLoot);
        public static void ReduceLootItem(int aSlotInLoot, int aCount) => lootBox.ReduceItem(aSlotInLoot, aCount);
        public static void Loot(LootDrop aDrop) => lootBox.Loot(aDrop);

        public static void HoldItem(Item aItem, AbsoluteScreenPosition aGrabOffset) => heldItem.HoldItem(aItem, aGrabOffset);
        public static void ReleaseItem() => heldItem.ReleaseMe();
        #endregion


        #region Mouse
        public static bool Click(ClickEvent aClickEvent)
        {
            if (sizeChanger.ClickedOn(aClickEvent)) return true;

            for (int i = 0; i < dialogueBoxes.Count; i++)
            {
                if (dialogueBoxes[i].ClickedOn(aClickEvent)) return true;
            }

            for (int i = hudElements.Count - 1; i >= 0; i--)
            {
                if (hudElements[i].ClickedOn(aClickEvent))
                {
                    UIElement temp = hudElements[i];
                    hudElements.RemoveAt(i);
                    hudElements.Add(temp);
                    return true;
                }
            }

            if (plateBoxHandler.Click(aClickEvent)) return true;


            return false;
        }

        public static bool Release(ReleaseEvent aReleaseEvent)
        {
            for (int i = hudElements.Count - 1; i >= 0; i--)
            {
                if (hudElements[i].ReleasedOn(aReleaseEvent)) return true;
            }
            return false;
        }

        internal static bool Scroll(ScrollEvent aScrollEvent)
        {
            for (int i = 0; i < dialogueBoxes.Count; i++)
            {
                if (dialogueBoxes[i].ScrolledOn(aScrollEvent)) return true;
            }
            for (int i = 0; i < hudElements.Count; i++)
            {
                if (hudElements[i].ScrolledOn(aScrollEvent)) return true;

            }
            return false;
        }

        public static void LeavingGameState()
        {
            for (int i = 0; i < hudElements.Count; i++)
            {
                hudElements[i].LeavingGameState();
            }
            heldItem.ReleaseMe();
            heldSpell.ReleaseMe();
        }
        #endregion

        public static void Draw(SpriteBatch aBatch)
        {
            namePlateHandler.Draw(aBatch);
            plateBoxHandler.Draw(aBatch);

            for (int i = 0; i < hudElements.Count; i++)
            {
                hudElements[i].Draw(aBatch);
            }

            for (int i = 0; i < dialogueBoxes.Count; i++)
            {
                dialogueBoxes[i].Draw(aBatch);
            }

            descriptorBox.Draw(aBatch);
            heldItem.Draw(aBatch);
            heldSpell.Draw(aBatch);
        }
    }
}
