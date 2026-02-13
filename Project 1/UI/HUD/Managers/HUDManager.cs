using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects.Spells;
using Project_1.Input;
using Project_1.Items;
using Project_1.Managers;
using Project_1.Managers.States;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.Textures;
using Project_1.UI;
using Project_1.UI.HUD;
using Project_1.UI.HUD.Inventory;
using Project_1.UI.HUD.SpellBook;
using Project_1.UI.HUD.Windows.Logic;
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
        static SaveStatusIndicator saveStatusIndicator;


        static HeldItem heldItem;
        static HeldSpell heldSpell;

        static List<DialogueBox> dialogueBoxes;

        static SizeChanger sizeChanger;

        static List<(string, RelativeScreenPosition, RelativeScreenPosition)> LoadedSettings;

        public static bool HudMoving => hudMoving;
        public static Action UiInvalidated;
        public static Action PlatesInvalidated;
        static readonly UiDrawList uiDrawListA = new UiDrawList();
        static readonly UiDrawList uiDrawListB = new UiDrawList();
        static volatile UiDrawList uiDrawList = uiDrawListA;
        static readonly PlateDrawList plateDrawListA = new PlateDrawList();
        static readonly PlateDrawList plateDrawListB = new PlateDrawList();
        static volatile PlateDrawList plateDrawList = plateDrawListA;
        static readonly HudMoveDrawList hudMoveDrawListA = new HudMoveDrawList();
        static readonly HudMoveDrawList hudMoveDrawListB = new HudMoveDrawList();
        static volatile HudMoveDrawList hudMoveDrawList = hudMoveDrawListA;
        static NamePlate[] plateNameScratch = Array.Empty<NamePlate>();
        static UIElement[] plateBoxScratch = Array.Empty<UIElement>();
        static bool uiDrawListDirty = true;
        static bool plateDrawListDirty = true;
        static bool hudMoveDrawListDirty = true;
        static bool hudMoving;
        static bool initialized;

        static void AssertUiThreadOrMainFallback()
        {
            if (UiThread.IsRunning)
            {
                ThreadAffinity.AssertUiThread();
                return;
            }
            ThreadAffinity.AssertMainThread();
        }

        static void AssertUiOrMainThread()
        {
            if (ThreadAffinity.IsMainThread) return;
            ThreadAffinity.AssertUiThread();
        }

        public static void Init()
        {
            ThreadAffinity.AssertMainThread();
            if (initialized) return;
            initialized = true;

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

            RelativeScreenPosition saveSize = RelativeScreenPosition.GetSquareFromX(0.03f);
            RelativeScreenPosition savePos = RelativeScreenPosition.One - saveSize - new RelativeScreenPosition(0.02f, 0.02f);
            saveStatusIndicator = new SaveStatusIndicator(savePos, saveSize);
            hudElements.Add(saveStatusIndicator);

            Mailboxes.Ui.Subscribe<LootOpened>(HandleLootOpened);
            Mailboxes.Ui.Subscribe<LootSlotChanged>(e =>
            {
                lootBox.RefreshSlot(e.Slot, e.ItemSnapshot);
                InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<LootSlotRemoved>(e =>
            {
                lootBox.RefreshSlot(e.Slot, ItemUiSnapshot.Empty);
                InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<LootClosed>(e =>
            {
                lootBox.CloseIfContext(e.ContextId);
                InvalidateUi();
            });

            Mailboxes.Ui.Subscribe<InventorySlotChanged>(e => RefreshInventorySlot(e.BagIndex, e.SlotIndex, e.Snapshot));
            Mailboxes.Ui.Subscribe<CastChannelStarted>(e => ChannelSpell(e.SpellGfxPath, e.DurationMs));
            Mailboxes.Ui.Subscribe<CastChannelProgress>(e => UpdateChannelSpell(e.Progress01));
            Mailboxes.Ui.Subscribe<CastChannelCancelled>(_ => CancelChannel());
            Mailboxes.Ui.Subscribe<CastChannelFinished>(_ => FinishChannel());
            Mailboxes.Ui.Subscribe<EquipmentSlotChanged>(e =>
            {
                windowHandler.RefreshCharacterWindowSlot(e.OwnerRenderId, e.OwnerRelation, e.Slot, e.ItemSnapshot);
                InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<EquipmentSlotsRefreshed>(e =>
            {
                windowHandler.RefreshAllCharacterWindowSlots(e.OwnerRenderId, e.OwnerRelation, e.ItemSnapshots);
                InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<StatsRefreshed>(e =>
            {
                windowHandler.RefreshCharacterWindowStats(e.OwnerRenderId, e.OwnerRelation, e.PrimaryStats, e.SecondaryStats);
                InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<ExperienceRefreshed>(e =>
            {
                windowHandler.RefreshCharacterWindowExpBar(e.OwnerRenderId, e.OwnerRelation, e.CurrentLevel, e.CurrentExperience);
                InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<TargetChanged>(e =>
            {
                plateBoxHandler.SetNewTarget(e.OwnerRelation, e.TargetSnapshot);
                InvalidateUi();
                InvalidatePlates();
            });
            Mailboxes.Ui.Subscribe<PlateRefreshRequested>(e =>
            {
                plateBoxHandler.RefreshPlates(e.Snapshot);
                namePlateHandler.RefreshNamePlate(e.Snapshot);
                InvalidateUi();
                InvalidatePlates();
            });
            Mailboxes.Ui.Subscribe<NamePlateAdded>(e =>
            {
                namePlateHandler.AddNamePlate(e.Snapshot);
                InvalidateUi();
                InvalidatePlates();
            });
            Mailboxes.Ui.Subscribe<NamePlateRemoved>(e =>
            {
                namePlateHandler.RemoveNamePlate(e.RenderId);
                InvalidateUi();
                InvalidatePlates();
            });
            Mailboxes.Ui.Subscribe<InventoryAssigned>(e => SetInventory(e.Snapshot));
            Mailboxes.Ui.Subscribe<SpellbookRefreshed>(e =>
            {
                windowHandler.RefreshSpellBook(BuildSpellsFromNames(e.SpellNames));
                InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<SpellbarLoaded>(e => LoadSpellBar(BuildSpellsFromNames(e.SpellNames)));
            Mailboxes.Ui.Subscribe<CharacterWindowSet>(e =>
            {
                windowHandler.SetCharacterWindow(e.Snapshot);
                InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<PlayerPlateSet>(e =>
            {
                plateBoxHandler.SetPlayerPlateBox(e.Snapshot);
                InvalidateUi();
                InvalidatePlates();
            });
            Mailboxes.Ui.Subscribe<GoldChanged>(e => RefreshGold(e.Gold));
            Mailboxes.Ui.Subscribe<PlayerUiSnapshot>(e => UiPlayerStateCache.Update(e));
            Mailboxes.Ui.Subscribe<GuildInviteStatusUpdated>(e =>
            {
                windowHandler.SetGuildMemberInviteStatus(e.MemberNames, e.Statuses);
                InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<BuffAdded>(e =>
            {
                plateBoxHandler.AddBuff(e.Buff, e.OwnerRenderId);
                InvalidateUi();
                InvalidatePlates();
            });
            Mailboxes.Ui.Subscribe<GossipOpened>(e =>
            {
                windowHandler.OpenGossipWindow(e.Data);
                InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<GossipClosed>(_ =>
            {
                windowHandler.CloseGossipWindow();
                InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<ShopOpened>(e =>
            {
                windowHandler.OpenShopWindow(e.ItemIds, e.ShopkeeperName);
                InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<ShopClosed>(_ =>
            {
                windowHandler.CloseShopWindow();
                InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<DescriptorBoxSet>(e =>
            {
                if (e.Position.HasValue)
                {
                    SetDescriptorBox(e.Snapshot, e.Position.Value.ToRelativeScreenPosition());
                }
                else
                {
                    SetDescriptorBox(e.Snapshot, UiMouseStateCache.Relative);
                }
            });
            Mailboxes.Ui.Subscribe<DescriptorBoxClear>(_ => ClearDescriptorBox());
            Mailboxes.Ui.Subscribe<PartyControlCleared>(e =>
            {
                for (int i = 0; i < e.MemberCount; i++)
                {
                    plateBoxHandler.RemoveWalkerFromControl(e.GetMemberRenderId(i));
                }
                InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<PartyWalkerAdded>(e =>
            {
                plateBoxHandler.AddGuildMemberToControl(e.MemberRenderId);
                InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<PartyWalkerRemoved>(e =>
            {
                plateBoxHandler.RemoveWalkerFromControl(e.MemberRenderId);
                InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<PartyMemberAdded>(e =>
            {
                plateBoxHandler.AddGuildMemberToParty(e.Member);
                InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<PartyMemberRemoved>(e =>
            {
                plateBoxHandler.RemoveGuildMemberFromParty(e.MemberRenderId);
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
            Mailboxes.Ui.Subscribe<LogicWindowOpened>(e =>
            {
                windowHandler.OpenLogicWindow(e.MemberRenderId);
                Mailboxes.PublishSimCommand(new LogicWindowSnapshotRequested(e.MemberRenderId));
                InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<LogicWindowSnapshotSet>(e =>
            {
                windowHandler.SetLogicWindowSnapshot(e.MemberRenderId, e.Nodes);
                InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<CharacterWindowToggled>(_ =>
            {
                windowHandler.ToggleCharacterWindow();
                InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<HeldItemStart>(e =>
            {
                if (!UIElement.TryResolve(e.SourceUiElementId, out UIElement element)) return;
                HoldItem(element as Project_1.UI.HUD.Inventory.Item, e.GrabOffset);
            });
            Mailboxes.Ui.Subscribe<HeldItemEnd>(_ => ReleaseItem());
            Mailboxes.Ui.Subscribe<HeldSpellStart>(e =>
            {
                if (string.IsNullOrWhiteSpace(e.SpellName)) return;
                HoldSpell(new Spell(e.SpellName), e.GrabOffset);
            });
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
            Mailboxes.Ui.Subscribe<HudSizeChangerSet>(e =>
            {
                if (!UIElement.TryResolve(e.UiElementId, out UIElement element)) return;
                SetSizeChanger(element);
            });
            Mailboxes.Ui.Subscribe<HudSaveRequested>(_ => Save());
            Mailboxes.Ui.Subscribe<DialogueOpened>(AddDialogueBox);
            Mailboxes.Ui.Subscribe<DialogueClosed>(e => RemoveDialogueBox(e.DialogueBoxId));
            Mailboxes.Ui.Subscribe<SaveDataStarted>(_ =>
            {
                saveStatusIndicator.NotifySaveStarted();
                InvalidateUi();
            });
            Mailboxes.Ui.Subscribe<SaveDataFinished>(_ =>
            {
                saveStatusIndicator.NotifySaveFinished();
                InvalidateUi();
            });

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
            AssertUiThreadOrMainFallback();
            long interactionVersionBefore = UIElement.InteractionVersion;
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

            if ((StateManager.CurrentState == StateManager.States.Game || StateManager.CurrentState == StateManager.States.MoveHUD)
                && UIElement.InteractionVersion != interactionVersionBefore)
            {
                InvalidateUi();
            }

            BuildDrawLists();
        }


        public static void Rescale()
        {
            AssertUiOrMainThread();
            namePlateHandler.Rescale();
            plateBoxHandler.Rescale();

            for (int i = 0; i < hudElements.Count; i++)
            {
                hudElements[i].Rescale();
            }

            for (int i = 0; i < dialogueBoxes.Count; i++)
            {
                dialogueBoxes[i].Rescale();
            }
        }

        #region HudChanging

        public static void SetSizeChanger(UIElement aUIElement)
        {
            AssertUiThreadOrMainFallback();
            if (!sizeChanger.Active) return;
            sizeChanger.SetElement(aUIElement);
        }

        public static void DisableHudMoveable() => SetHudMoveable(false);

        public static void SetHudMoveable(bool aSet) //TODO: Should this really be done this way and not by a bool flag in hud manager?
        {
            AssertUiThreadOrMainFallback();
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
            AssertUiThreadOrMainFallback();
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
            ThreadAffinity.AssertMainThread();
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
            AssertUiThreadOrMainFallback();
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

        public static void ChangeSizes()
        {
            AssertUiThreadOrMainFallback();
            sizeChanger.Active = true;
        }

        public static void DisableSizeChanges()
        {
            AssertUiThreadOrMainFallback();
            sizeChanger.Active = false;
        }

        public static void Save()
        {
            AssertUiThreadOrMainFallback();
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
            AssertUiThreadOrMainFallback();
            dialogueBoxes.Add(aDialogueBox);
            InvalidateUi();
        }

        static void AddDialogueBox(DialogueOpened e)
        {
            AssertUiThreadOrMainFallback();
            UITexture background = e.Background == null ? UITexture.Null : new UITexture(e.Background, Color.White);
            DialogueBox box = new DialogueBox(e.Text, e.TextColor, e.Location.ToDialogueBoxLocation(), e.Pauses.ToDialogueBoxPause(), new List<Action>(), background, e.Pos, e.Size, e.CloseText);
            dialogueBoxes.Add(box);
            InvalidateUi();
        }

        public static void RemoveDialogueBox(int dialogueBoxId)
        {
            AssertUiThreadOrMainFallback();
            DialogueBox toRemove = dialogueBoxes.Find(x => x.UiElementId == dialogueBoxId);
            if (toRemove == null) return;
            dialogueBoxes.Remove(toRemove);
            InvalidateUi();
        }
        #endregion





        #region Inventory
        public static void SetInventory(InventoryUiSnapshot snapshot)
        {
            AssertUiThreadOrMainFallback();
            inventoryBox.SetInventory(snapshot);
            InvalidateUi();
        }
        public static void RefreshInventorySlot(int aBag, int aSlot, InventoryUiSnapshot snapshot)
        {
            AssertUiThreadOrMainFallback();
            inventoryBox.RefreshSlot(aBag, aSlot, snapshot);
            InvalidateUi();
        }
        public static void RefreshInventorySlot((int, int) aBagAndSlot, InventoryUiSnapshot snapshot) => RefreshInventorySlot(aBagAndSlot.Item1, aBagAndSlot.Item2, snapshot);

        public static void SetDescriptorBox(in ItemDescriptorSnapshot snapshot, RelativeScreenPosition aPos)
        {
            AssertUiThreadOrMainFallback();
            descriptorBox.SetToSnapshot(snapshot, aPos);
            InvalidateUi();
        }

        public static void ClearDescriptorBox()
        {
            AssertUiThreadOrMainFallback();
            descriptorBox.Clear();
            InvalidateUi();
        }

        public static void RefreshGold(int aGoldAmount)
        {
            AssertUiThreadOrMainFallback();
            inventoryBox.RefreshGold(aGoldAmount);
            InvalidateUi();
        }
        #endregion

        

        #region Spell
        public static void HoldSpell(Spell aSpell, AbsoluteScreenPosition aGrabOffset)
        {
            AssertUiThreadOrMainFallback();
            heldSpell.HoldMe(aSpell, aGrabOffset);
            InvalidateUi();
        }
        public static void ReleaseSpell()
        {
            AssertUiThreadOrMainFallback();
            heldSpell.ReleaseMe();
            InvalidateUi();
        }

        public static void FinishChannel()
        {
            AssertUiThreadOrMainFallback();
            playerCastBar.FinishCast();
            InvalidateUi();
        }
        public static void CancelChannel()
        {
            AssertUiThreadOrMainFallback();
            playerCastBar.CancelCast();
            InvalidateUi();
        }
        public static void UpdateChannelSpell(float aNewVal)
        {
            AssertUiThreadOrMainFallback();
            playerCastBar.Value = aNewVal;
            InvalidateUi();
        }
        public static void ChannelSpell(GfxPath spellGfxPath, double castDurationMs)
        {
            AssertUiThreadOrMainFallback();
            playerCastBar.CastSpell(spellGfxPath, castDurationMs);
            InvalidateUi();
        }

        

        public static void LoadSpellBar(Spell[] aSpells)
        {
            AssertUiThreadOrMainFallback();
            firstSpellBar.LoadBar(aSpells);
            InvalidateUi();
        }
        public static string[] SaveSpellBar
        {
            get
            {
                AssertUiThreadOrMainFallback();
                return firstSpellBar.SaveBar();
            }
        }


        #endregion

        #region Loot
        public static void Loot(ItemUiSnapshot[] snapshot, LootContext context)
        {
            AssertUiThreadOrMainFallback();
            lootBox.Loot(context, snapshot);
            InvalidateUi();
        }
        public static void RefreshLootSlot(int slot, ItemUiSnapshot snapshot)
        {
            AssertUiThreadOrMainFallback();
            lootBox.RefreshSlot(slot, snapshot);
            InvalidateUi();
        }

        static void HandleLootOpened(LootOpened e)
        {
            Loot(e.Snapshot, e.Context);
        }

        static Spell[] BuildSpellsFromNames(string[] spellNames)
        {
            if (spellNames == null) return Array.Empty<Spell>();
            Spell[] spells = new Spell[spellNames.Length];
            for (int i = 0; i < spellNames.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(spellNames[i])) continue;
                spells[i] = new Spell(spellNames[i]);
            }
            return spells;
        }

        public static void HoldItem(Item aItem, AbsoluteScreenPosition aGrabOffset)
        {
            AssertUiThreadOrMainFallback();
            heldItem.HoldItem(aItem, aGrabOffset);
            InvalidateUi();
        }
        public static void ReleaseItem()
        {
            AssertUiThreadOrMainFallback();
            heldItem.ReleaseMe();
            InvalidateUi();
        }
        #endregion

        public static void InvalidateUi()
        {
            AssertUiThreadOrMainFallback();
            uiDrawListDirty = true;
            hudMoveDrawListDirty = true;
            UiInvalidated?.Invoke();
        }
        public static void InvalidatePlates()
        {
            AssertUiThreadOrMainFallback();
            plateDrawListDirty = true;
            hudMoveDrawListDirty = true;
            PlatesInvalidated?.Invoke();
        }

        internal static UiDrawList UiDrawListSnapshot
        {
            get
            {
                ThreadAffinity.AssertMainThread();
                return uiDrawList;
            }
        }
        internal static PlateDrawList PlateDrawListSnapshot
        {
            get
            {
                ThreadAffinity.AssertMainThread();
                return plateDrawList;
            }
        }
        internal static HudMoveDrawList HudMoveDrawListSnapshot
        {
            get
            {
                ThreadAffinity.AssertMainThread();
                return hudMoveDrawList;
            }
        }

        internal static void BuildDrawLists()
        {
            AssertUiOrMainThread();
            int namePlateCount = 0;
            int plateBoxCount = 0;
            if (plateDrawListDirty || hudMoveDrawListDirty)
            {
                namePlateCount = namePlateHandler.DrawListCount;
                EnsureNamePlateScratchCapacity(namePlateCount);
                namePlateCount = namePlateHandler.CopyDrawList(plateNameScratch);

                plateBoxCount = plateBoxHandler.DrawListCount;
                EnsurePlateBoxScratchCapacity(plateBoxCount);
                plateBoxCount = plateBoxHandler.CopyDrawList(plateBoxScratch);
            }

            if (uiDrawListDirty)
            {
                UiDrawList buildTarget = ReferenceEquals(uiDrawList, uiDrawListA) ? uiDrawListB : uiDrawListA;
                buildTarget.Set(hudElements, dialogueBoxes, descriptorBox, heldItem, heldSpell);
                uiDrawList = buildTarget;
                uiDrawListDirty = false;
            }

            if (plateDrawListDirty)
            {
                PlateDrawList buildTarget = ReferenceEquals(plateDrawList, plateDrawListA) ? plateDrawListB : plateDrawListA;
                buildTarget.Set(plateNameScratch, namePlateCount, plateBoxScratch, plateBoxCount);
                plateDrawList = buildTarget;
                plateDrawListDirty = false;
            }

            if (hudMoveDrawListDirty)
            {
                HudMoveDrawList buildTarget = ReferenceEquals(hudMoveDrawList, hudMoveDrawListA) ? hudMoveDrawListB : hudMoveDrawListA;
                buildTarget.Set(plateBoxScratch, plateBoxCount, hudElements, dialogueBoxes, sizeChanger);
                hudMoveDrawList = buildTarget;
                hudMoveDrawListDirty = false;
            }
        }

        static void EnsureNamePlateScratchCapacity(int count)
        {
            if (count <= plateNameScratch.Length) return;
            int capacity = Math.Max(count, Math.Max(8, plateNameScratch.Length * 2));
            plateNameScratch = new NamePlate[capacity];
        }

        static void EnsurePlateBoxScratchCapacity(int count)
        {
            if (count <= plateBoxScratch.Length) return;
            int capacity = Math.Max(count, Math.Max(8, plateBoxScratch.Length * 2));
            plateBoxScratch = new UIElement[capacity];
        }


        #region Mouse
        public static bool Click(ClickEvent aClickEvent)
        {
            AssertUiThreadOrMainFallback();
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
            AssertUiThreadOrMainFallback();
            for (int i = hudElements.Count - 1; i >= 0; i--)
            {
                if (hudElements[i].ReleasedOn(aReleaseEvent)) return true;
            }
            InvalidateUi();
            return false;
        }

        internal static bool Scroll(ScrollEvent aScrollEvent)
        {
            AssertUiThreadOrMainFallback();
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
            AssertUiThreadOrMainFallback();
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
