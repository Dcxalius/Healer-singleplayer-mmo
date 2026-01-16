using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects.Entities.GuildMembers;
using Project_1.GameObjects.Entities.Players;
using Project_1.GameObjects.Spells;
using Project_1.Input;
using Project_1.Items;
using Project_1.Managers;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.UI;
using Project_1.UI.HUD.Inventory;
using Project_1.UI.HUD.SpellBook;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Bars;
using Project_1.UI.UIElements.Boxes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Item = Project_1.UI.HUD.Inventory.Item;

namespace Project_1.UI.HUD.Managers
{
    internal static class HUDManager
    {
        internal static readonly object UiLock = new object();
        public static PlateBoxHandler plateBoxHandler;
        public static NamePlateHandler namePlateHandler;
        public static WindowHandler windowHandler;
        //public static HudChanger hudChanger;

        static List<UIElement> hudElements;

        

        static InventoryBox inventoryBox;
        static LootBox lootBox;
        static DescriptorBox descriptorBox;

        static CastBar playerCastBar;
        static SpellBar firstSpellBar;

        static Minimap minimap;


        static HeldItem heldItem;
        static HeldSpell heldSpell;

        static List<DialogueBox> dialogueBoxes;

        static SizeChanger sizeChanger;

        static List<(string, RelativeScreenPosition, RelativeScreenPosition)> LoadedSettings;

        public static bool HudMoving => hudMoving;
        public static Action UiInvalidated;
        public static Action PlatesInvalidated;
        static volatile UiDrawList uiDrawList;
        static volatile PlateDrawList plateDrawList;
        static bool uiDrawListDirty = true;
        static bool plateDrawListDirty = true;
        static bool hudMoving;
        static bool initialized;
        static HUDManager()
        {
            ThreadAffinity.AssertMainThread();
            plateBoxHandler = new PlateBoxHandler();
            namePlateHandler = new NamePlateHandler();
            windowHandler = new WindowHandler();
            //hudChanger = new HudChanger();

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

            var loaded = LoadedSettings.Find(x => x.Item1 == typeof(SpellBar).Name);
            firstSpellBar = new SpellBar(Color.White, 10, loaded.Item2, loaded.Item3.X);
            hudElements.Add(firstSpellBar);
            loaded = LoadedSettings.Find(x => x.Item1 == typeof(CastBar).Name);
            playerCastBar = new CastBar(loaded.Item2, loaded.Item3);
            hudElements.Add(playerCastBar);

            heldItem = new HeldItem();
            heldSpell = new HeldSpell();

            dialogueBoxes = new List<DialogueBox>();

            RelativeScreenPosition mmSize = RelativeScreenPosition.GetSquareFromX(0.2f);
            minimap = new Minimap(new RelativeScreenPosition(0.75f, 0.05f), mmSize);
            hudElements.Add(minimap);
        }

        public static void Init()
        {
            ThreadAffinity.AssertMainThread();
            if (initialized) return;
            initialized = true;

            Mailboxes.Ui.Subscribe<LootOpened>(HandleLootOpened);
            Mailboxes.Ui.Subscribe<LootSlotChanged>(e =>
            {
                lootBox.RefreshSlot(e.Slot);
                InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<LootSlotRemoved>(e =>
            {
                lootBox.RefreshSlot(e.Slot);
                InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<LootClosed>(e =>
            {
                LootState.Close(e.ContextId);
                lootBox.CloseIfContext(e.ContextId);
                InvalidateUi();
            });

            Mailboxes.Ui.Subscribe<InventorySlotChanged>(e => RefreshInventorySlot(e.BagIndex, e.SlotIndex, e.Inventory));
            Mailboxes.Ui.Subscribe<CastChannelStarted>(e => ChannelSpell(e.Spell));
            Mailboxes.Ui.Subscribe<CastChannelProgress>(e => UpdateChannelSpell(e.Progress01));
            Mailboxes.Ui.Subscribe<CastChannelCancelled>(_ => CancelChannel());
            Mailboxes.Ui.Subscribe<CastChannelFinished>(_ => FinishChannel());
            Mailboxes.Ui.Subscribe<EquipmentSlotChanged>(e =>
            {
                windowHandler.RefreshCharacterWindowSlot(e.Slot, e.Equipment, e.Friendly);
                InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<EquipmentSlotsRefreshed>(e =>
            {
                windowHandler.RefreshAllCharacterWindowSlots(e.Equipment, e.Friendly);
                InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<StatsRefreshed>(e =>
            {
                windowHandler.RefreshCharacterWindowStats(e.Stats, e.Friendly);
                InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<ExperienceRefreshed>(e =>
            {
                windowHandler.RefreshCharacterWindowExpBar(e.Friendly);
                InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<TargetChanged>(e =>
            {
                plateBoxHandler.SetNewTarget(e.Owner, e.Target);
                InvalidateUi();
                InvalidatePlates();
            });
            Mailboxes.Ui.Subscribe<PlateRefreshRequested>(e =>
            {
                plateBoxHandler.RefreshPlates(e.Entity);
                InvalidateUi();
                InvalidatePlates();
            });
            Mailboxes.Ui.Subscribe<NamePlateAdded>(e =>
            {
                namePlateHandler.AddNamePlate(e.Entity, e.Plate);
                InvalidateUi();
                InvalidatePlates();
            });
            Mailboxes.Ui.Subscribe<NamePlateRemoved>(e =>
            {
                namePlateHandler.RemoveNamePlate(e.Entity);
                InvalidateUi();
                InvalidatePlates();
            });
            Mailboxes.Ui.Subscribe<InventoryAssigned>(e => SetInventory(e.Inventory));
            Mailboxes.Ui.Subscribe<SpellbookRefreshed>(e =>
            {
                windowHandler.RefreshSpellBook(e.Spells);
                InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<SpellbarLoaded>(e => LoadSpellBar(e.Spells));
            Mailboxes.Ui.Subscribe<CharacterWindowSet>(e =>
            {
                if (e.Owner is Player p)
                {
                    windowHandler.SetCharacterWindow(p);
                }
                InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<PlayerPlateSet>(e =>
            {
                if (e.Owner is Player p)
                {
                    plateBoxHandler.SetPlayerPlateBox(p);
                    InvalidateUi();
                    InvalidatePlates();
                }
            });
            Mailboxes.Ui.Subscribe<GoldChanged>(e => RefreshGold(e.Gold));
            Mailboxes.Ui.Subscribe<GuildInviteStatusUpdated>(e =>
            {
                windowHandler.SetGuildMemberInviteStatus(
                    e.MemberNames as System.Collections.Generic.List<string>
                        ?? new System.Collections.Generic.List<string>(e.MemberNames),
                    e.Statuses as System.Collections.Generic.List<Project_1.UI.UIElements.Buttons.TwoStateGFXButton.State>
                        ?? new System.Collections.Generic.List<Project_1.UI.UIElements.Buttons.TwoStateGFXButton.State>(e.Statuses));
                InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<BuffAdded>(e =>
            {
                plateBoxHandler.AddBuff(e.Buff, e.Owner);
                InvalidateUi();
                InvalidatePlates();
            });
            Mailboxes.Ui.Subscribe<GossipOpened>(e =>
            {
                windowHandler.OpenGossipWindow(e.Start, e.Npc);
                InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<ShopOpened>(e =>
            {
                windowHandler.OpenShopWindow(e.Shop, e.Npc);
                InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<DescriptorBoxSet>(e =>
            {
                if (e.Position.HasValue)
                {
                    SetDescriptorBox(e.Item, e.Position.Value.ToRelativeScreenPosition());
                }
                else
                {
                    SetDescriptorBox(e.Item, UiMouseStateCache.Relative);
                }
            });
            Mailboxes.Ui.Subscribe<DescriptorBoxClear>(_ => SetDescriptorBox(null));
            Mailboxes.Ui.Subscribe<PartyControlCleared>(e =>
            {
                plateBoxHandler.RemoveWalkerFromControl(e.Members.ToArray());
                InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<PartyWalkerAdded>(e =>
            {
                plateBoxHandler.AddGuildMemberToControl(e.Member);
                InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<PartyWalkerRemoved>(e =>
            {
                plateBoxHandler.RemoveWalkerFromControl(new GuildMember[] { e.Member });
                InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<PartyMemberAdded>(e =>
            {
                plateBoxHandler.AddGuildMemberToParty(e.Member);
                InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<PartyMemberRemoved>(e =>
            {
                plateBoxHandler.RemoveGuildMemberFromParty(e.Member);
                InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<GuildMembersSet>(e =>
            {
                windowHandler.SetGuildMembers(e.Members);
                InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<GuildMemberAdded>(e =>
            {
                windowHandler.AddGuildMember(e.Member);
                InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<PartyCleared>(_ =>
            {
                plateBoxHandler.ClearParty();
                InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<InspectWindowToggled>(e =>
            {
                windowHandler.ToggleInspectWindow(e.Member);
                InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<CharacterWindowToggled>(_ =>
            {
                windowHandler.ToggleCharacterWindow();
                InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<HeldItemStart>(e => HoldItem(e.Source as Project_1.UI.HUD.Inventory.Item, e.GrabOffset));
            Mailboxes.Ui.Subscribe<HeldItemEnd>(_ => ReleaseItem());
            Mailboxes.Ui.Subscribe<HeldSpellStart>(e => HoldSpell(e.Spell, e.GrabOffset));
            Mailboxes.Ui.Subscribe<HeldSpellEnd>(_ => ReleaseSpell());
            Mailboxes.Ui.Subscribe<HudMovableChanged>(e =>
            {
                if (e.Enabled) SetHudMoveable(true);
                else ResetHudMoveable();
                InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<HudSizeChangeRequested>(e =>
            {
                if (e.Enabled) ChangeSizes();
                else DisableSizeChanges();
                InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<HudSizeChangerSet>(e => SetSizeChanger(e.Element));
            Mailboxes.Ui.Subscribe<HudSaveRequested>(_ => Save());
            Mailboxes.Ui.Subscribe<DialogueOpened>(e => AddDialogueBox(e.Box));
            Mailboxes.Ui.Subscribe<DialogueClosed>(e => RemoveDialogueBox(e.Box));

            uiDrawListDirty = true;
            plateDrawListDirty = true;
            BuildDrawLists();
            InvalidateUi();
            InvalidatePlates();
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

            BuildDrawLists();
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
        public static void AddDialogueBox(DialogueBox aDialogueBox)
        {
            dialogueBoxes.Add(aDialogueBox);
            InvalidateUi();
        }

        public static void RemoveDialogueBox(DialogueBox aDialogueBox)
        {
            dialogueBoxes.Remove(aDialogueBox);
            InvalidateUi();
        }
        #endregion





        #region Inventory
        public static void SetInventory(Items.Inventory aInventory)
        {
            inventoryBox.SetInventory(aInventory);
            InvalidateUi();
        }
        public static void RefreshInventorySlot(int aBag, int aSlot, Items.Inventory aInventory)
        {
            inventoryBox.RefreshSlot(aBag, aSlot, aInventory);
            InvalidateUi();
        }
        public static void RefreshInventorySlot((int, int) aBagAndSlot, Items.Inventory aInventory) => RefreshInventorySlot(aBagAndSlot.Item1, aBagAndSlot.Item2, aInventory);

        public static void SetDescriptorBox(Item aItem)
        {
            descriptorBox.SetToItem(aItem);
            InvalidateUi();
        }
        public static void SetDescriptorBox(Items.Item aItem, RelativeScreenPosition aPos)
        {
            descriptorBox.SetToItem(aItem, aPos);
            InvalidateUi();
        }

        public static void RefreshGold(int aGoldAmount)
        {
            inventoryBox.RefreshGold(aGoldAmount);
            InvalidateUi();
        }
        #endregion

        

        #region Spell
        public static void HoldSpell(Spell aSpell, AbsoluteScreenPosition aGrabOffset)
        {
            heldSpell.HoldMe(aSpell, aGrabOffset);
            InvalidateUi();
        }
        public static void ReleaseSpell()
        {
            heldSpell.ReleaseMe();
            InvalidateUi();
        }

        public static void FinishChannel()
        {
            playerCastBar.FinishCast();
            InvalidateUi();
        }
        public static void CancelChannel()
        {
            playerCastBar.CancelCast();
            InvalidateUi();
        }
        public static void UpdateChannelSpell(float aNewVal)
        {
            playerCastBar.Value = aNewVal;
            InvalidateUi();
        }
        public static void ChannelSpell(Spell aSpell)
        {
            playerCastBar.CastSpell(aSpell);
            InvalidateUi();
        }

        

        public static void LoadSpellBar(Spell[] aSpells)
        {
            firstSpellBar.LoadBar(aSpells);
            InvalidateUi();
        }
        public static string[] SaveSpellBar => firstSpellBar.SaveBar();


        #endregion

        #region Loot
        public static void Loot(Items.Item[] snapshot, LootContext context)
        {
            lootBox.Loot(context, snapshot);
            InvalidateUi();
        }
        public static void RefreshLootSlot(int slot)
        {
            lootBox.RefreshSlot(slot);
            InvalidateUi();
        }

        static void HandleLootOpened(LootOpened e)
        {
            Loot(e.Snapshot, e.Context);
        }

        public static void HoldItem(Item aItem, AbsoluteScreenPosition aGrabOffset)
        {
            heldItem.HoldItem(aItem, aGrabOffset);
            InvalidateUi();
        }
        public static void ReleaseItem()
        {
            heldItem.ReleaseMe();
            InvalidateUi();
        }
        #endregion

        public static void InvalidateUi()
        {
            uiDrawListDirty = true;
            UiInvalidated?.Invoke();
        }
        public static void InvalidatePlates()
        {
            plateDrawListDirty = true;
            PlatesInvalidated?.Invoke();
        }

        internal static UiDrawList UiDrawListSnapshot => uiDrawList;
        internal static PlateDrawList PlateDrawListSnapshot => plateDrawList;

        internal static void BuildDrawLists()
        {
            if (uiDrawListDirty)
            {
                uiDrawList = new UiDrawList(hudElements.ToArray(), dialogueBoxes.ToArray(), descriptorBox, heldItem, heldSpell);
                uiDrawListDirty = false;
            }

            if (plateDrawListDirty)
            {
                plateDrawList = new PlateDrawList(namePlateHandler.GetDrawList(), plateBoxHandler.GetDrawList());
                plateDrawListDirty = false;
            }
        }


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
                    InvalidateUi();
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
            InvalidateUi();
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
            InvalidateUi();
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

    }
}
