using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Entities.Players;
using Project_1.GameObjects.Spells;
using Project_1.GameObjects.Unit;
using Project_1.Input;
using Project_1.Items;
using Project_1.Managers;
using Project_1.Textures;
using Project_1.UI.HUD.PlateBoxes;
using Project_1.UI.HUD.Windows;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Bars;
using Project_1.UI.UIElements.Boxes;
using Project_1.UI.UIElements.Buttons;
using Project_1.UI.UIElements.Guild;
using Project_1.UI.UIElements.Inventory;
using Project_1.UI.UIElements.PlateBoxes;
using Project_1.UI.UIElements.SpellBook;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Item = Project_1.UI.UIElements.Inventory.Item;

namespace Project_1.UI.HUD
{
    internal static class HUDManager
    {
        static Dictionary<Entity, NamePlate> namePlates = new Dictionary<Entity, NamePlate>();

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

        public static void Init()
        {
            SetSettings();

            hudElements = new List<UIElement>();
            InitPlateBoxes();

            lootBox = new LootBox(new RelativeScreenPosition(0.1f, 0.5f), new RelativeScreenPosition(0.4f, 0.4f));
            hudElements.Add(lootBox);
            inventoryBox = new InventoryBox(new RelativeScreenPosition(0.59f, 0.60f), new RelativeScreenPosition(0.4f));
            hudElements.Add(inventoryBox);

            descriptorBox = new DescriptorBox(); //Shouldnt be in elements for now

            sizeChanger = new SizeChanger();

            InitWindows();

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

        static void SetSettings()
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
            UpdateNamePlates();
            for (int i = 0; i < plateBoxes.Count; i++)
            {
                plateBoxes[i].Update();
            }

            for (int i = 0; i < hudElements.Count; i++)
            {
                hudElements[i].Update();
            }

            for (int i = 0; i < dialogueBoxes.Count; i++)
            {
                dialogueBoxes[i].Update();
            }
        }

        static void UpdateNamePlates()
        {

            foreach (KeyValuePair<Entity, NamePlate> namePlate in namePlates)
            {
                namePlate.Value.Update();
            }

            int maxPasses = 50;
            int passes = 0;
            List<Rectangle> collisonRects = new List<Rectangle>();
            do
            {
                passes++;
                List<NamePlate> namePlates = new List<NamePlate>();
                foreach (KeyValuePair<Entity, NamePlate> namePlate in HUDManager.namePlates)
                {
                    namePlates.Add(namePlate.Value);
                }


                List<(int, int)> collisionIndexes = new List<(int, int)>();
                collisonRects.Clear();

                for (int i = 0; i < namePlates.Count; i++)
                {
                    for (int j = 0; j < namePlates.Count; j++)
                    {
                        if (i == j) continue;
                        if (collisionIndexes.Contains((j, i))) continue;

                        Rectangle r = Rectangle.Intersect(namePlates[i].AbsolutePos, namePlates[j].AbsolutePos);

                        if (r.Size.X != 0 && r.Size.Y != 0)
                        {
                            collisionIndexes.Add((i, j));
                            collisonRects.Add(r);
                        }
                    }
                }

                for (int i = 0; i < collisonRects.Count; i++)
                {
                    UpdateSingleNamePlate(namePlates[collisionIndexes[i].Item1], collisonRects[i].Center, collisonRects[i].Size);
                    UpdateSingleNamePlate(namePlates[collisionIndexes[i].Item2], collisonRects[i].Center, collisonRects[i].Size);
                }
            }
            while (collisonRects.Count > 0 && passes < maxPasses);

            //DebugManager.Print(typeof(HUDManager), "Passes of nameplatedupdate was: " + passes);
        }

        static void UpdateSingleNamePlate(NamePlate aNamePlate, Point aCentre, Point aSize)
        {
            Vector2 dir = (aNamePlate.AbsolutePos.Center - aCentre).ToVector2();
            dir.Normalize();

            if (float.IsNaN(dir.X) || float.IsNaN(dir.Y)) dir = Vector2.Zero;
            
            //DebugManager.Print(typeof(HUDManager), "Nameplate of " + aNamePlate.Name + " was at " + aNamePlate.RelativePos);

            aNamePlate.Move(aNamePlate.RelativePos + (new AbsoluteScreenPosition((aSize.ToVector2() * (dir / 2)).ToPoint()) + new AbsoluteScreenPosition(1 * Math.Sign(dir.X), 1 * Math.Sign(dir.Y))).ToRelativeScreenPosition());
            
            //DebugManager.Print(typeof(HUDManager), "Nameplate of " + aNamePlate.Name + " is now at " + aNamePlate.RelativePos);

            if (aNamePlate.RelativePos.X == -1431655) throw new Exception();

        }

        public static void Rescale()
        {
            foreach (KeyValuePair<Entity, NamePlate> namePlate in namePlates)
            {
                namePlate.Value.Rescale();
            }

            for (int i = 0; i < plateBoxes.Count; i++)
            {
                plateBoxes[i].Rescale();
            }

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

        public static void SetHudMoveable(bool aSet)
        {
            for (int i = 0; i < plateBoxes.Count; i++)
            {
                plateBoxes[i].SetHudMoveable(aSet);
            }

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
            for (int i = 0; i < plateBoxes.Count; i++)
            {
                plateBoxes[i].ResetHudMoveable();
            }

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
            for (int i = 0; i < plateBoxes.Count; i++)
            {
                plateBoxes[i].HudMovableDraw(aBatch);
            }

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
            for (int i = 0; i < plateBoxes.Count; i++)
            {
                plateBoxes[i].Update();
            }

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
            for (int i = 0; i < plateBoxes.Count; i++)
            {
                if (!plateBoxes[i].HudMoveable) continue;
                saveables.Add(plateBoxes[i].Save);
            }

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

        #region PlateBoxes 
        static PlayerPlateBox playerPlateBox;
        static TargetPlateBox targetPlateBox;
        static PartyPlateBox[] partyPlateBoxes;
        static BuffBox playerBuffBox;
        static BuffBox targetBuffBox;
        static BuffBox[] partyBuffBoxes;

        static List<UIElement> plateBoxes;
        static void InitPlateBoxes()
        {
            plateBoxes = new List<UIElement>();

            var loaded = LoadedSettings.Find(x => x.Item1 == typeof(PlayerPlateBox).Name);
            playerPlateBox = new PlayerPlateBox(loaded.Item2, loaded.Item3);
            plateBoxes.Add(playerPlateBox);
            loaded = LoadedSettings.Find(x => x.Item1 == typeof(TargetPlateBox).Name);
            targetPlateBox = new TargetPlateBox(loaded.Item2, loaded.Item3);
            plateBoxes.Add(targetPlateBox);
            partyPlateBoxes = new PartyPlateBox[4];
            var loadedPos = LoadedSettings.Where(x => x.Item1 == typeof(PartyPlateBox).Name).ToArray();
            for (int i = 0; i < partyPlateBoxes.Length; i++)
            {

                partyPlateBoxes[i] = new PartyPlateBox(loadedPos[i].Item2, loadedPos[i].Item3, i);
            }
            plateBoxes.AddRange(partyPlateBoxes);

            loadedPos = LoadedSettings.Where(x => x.Item1 == typeof(BuffBox).Name).ToArray();
            playerBuffBox = new BuffBox(ObjectManager.Player, BuffBox.FillDirection.TopRightToDown, loadedPos[0].Item2, loadedPos[0].Item3);
            plateBoxes.Add(playerBuffBox);
            targetBuffBox = new BuffBox(null, BuffBox.FillDirection.TopRightToDown, loadedPos[1].Item2, loadedPos[1].Item3);
            plateBoxes.Add(targetBuffBox);
            partyBuffBoxes = new BuffBox[4];
            for (int i = 0; i < partyBuffBoxes.Length; i++)
            {

                partyBuffBoxes[i] = new BuffBox(null, BuffBox.FillDirection.TopRightToDown, loadedPos[2 + i].Item2, loadedPos[2 + i].Item3);
            }
            plateBoxes.AddRange(partyBuffBoxes);
        }
        #endregion

        #region Windows

        static CharacterWindow characterWindow;
        static SpellBookWindow spellBookWindow;
        static GuildWindow guildWindow;
        static InspectWindow inspectWindow;
        static void InitWindows()
        {
            Window.Init(new RelativeScreenPosition(0.05f, 0.2f), new RelativeScreenPosition(0.1f, 0f), new RelativeScreenPosition(0.2f, 0.6f));
            characterWindow = new CharacterWindow();
            hudElements.Add(characterWindow);
            spellBookWindow = new SpellBookWindow();
            hudElements.Add(spellBookWindow);
            inspectWindow = new InspectWindow();
            hudElements.Add(inspectWindow);
            guildWindow = new GuildWindow();
            hudElements.Add(guildWindow);

        }
        #endregion

        #region Dialogue
        public static void AddDialogueBox(DialogueBox aDialogueBox) => dialogueBoxes.Add(aDialogueBox);

        public static void RemoveDialogueBox(DialogueBox aDialogueBox) => dialogueBoxes.Remove(aDialogueBox);
        #endregion

        #region Plate
        public static void AddNamePlate(Entity aEntity, NamePlate aNamePlate) => namePlates.Add(aEntity, aNamePlate);

        public static void RemoveNamePlate(Entity aEntity) => namePlates.Remove(aEntity);

        public static void SetPlayerPlateBox(Player aPlayer) => playerPlateBox.SetData(aPlayer);

        public static void RefreshPlates(Entity aEntity)
        {
            switch (aEntity.RelationToPlayer)
            {
                case Relation.RelationToPlayer.Self:
                    playerPlateBox.Refresh(aEntity);
                    if (!targetPlateBox.BelongsTo(aEntity)) break;
                    targetPlateBox.Refresh(aEntity);
                    break;
                case Relation.RelationToPlayer.Friendly:
                    if (targetPlateBox.BelongsTo(aEntity)) targetPlateBox.Refresh(aEntity); 
                    for (int i = 0; i < partyPlateBoxes.Length; i++)
                    {
                        if (partyPlateBoxes[i].BelongsTo(null)) break;
                        if (!partyPlateBoxes[i].BelongsTo(aEntity as GuildMember)) continue;
                        partyPlateBoxes[i].Refresh(aEntity);
                        break;
                    }
                    break;
                case Relation.RelationToPlayer.Neutral:
                case Relation.RelationToPlayer.Hostile:
                    if (!targetPlateBox.BelongsTo(aEntity)) break;
                    targetPlateBox.Refresh(aEntity);
                    break;
                default:
                    break;
            }

            namePlates[aEntity].Refresh(aEntity);
        }
        public static void SetNewTarget(Entity aTargeter, Entity aTarget)
        {
            switch (aTargeter.RelationToPlayer)
            {
                case Relation.RelationToPlayer.Self:
                    targetPlateBox.SetTarget(aTarget);
                    targetBuffBox.AssignBox(aTarget);
                    break;
                case Relation.RelationToPlayer.Friendly:
                case Relation.RelationToPlayer.Neutral:
                case Relation.RelationToPlayer.Hostile:
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
        #endregion

        #region Control
        public static void AddGuildMemberToParty(GuildMember aGuildMember)
        {
            if (PartyPlateBox.PartyBoxesActive >= Party.maxPartySize)
            {
                DebugManager.Print(typeof(HUDManager), "Tried to add to full party.");
                return;
            }

            partyPlateBoxes[PartyPlateBox.PartyBoxesActive].SetTarget(aGuildMember);
            partyBuffBoxes[PartyPlateBox.PartyBoxesActive - 1].AssignBox(aGuildMember);
        }

        public static void RemoveGuildMemberFromParty(GuildMember aGuildMember)
        {
            int index = FindGuildMemberPartyIndex(aGuildMember);

            Debug.Assert(index >= 0);

            if (PartyPlateBox.PartyBoxesActive - 1 == index)
            {
                partyPlateBoxes[index].RemoveTarget();
                return;
            }

            for (int i = index; i < PartyPlateBox.PartyBoxesActive - index - 1; i++)
            {
                partyPlateBoxes[i].SetTarget(partyPlateBoxes[i + 1].GuildMember);
            }

            partyPlateBoxes[PartyPlateBox.PartyBoxesActive - 1].RemoveTarget();
        }

        public static void ClearParty()
        {
            for (int i = 0; i < partyPlateBoxes.Length; i++)
            {
                partyPlateBoxes[i].RemoveTarget();
            }
            PartyPlateBox.ClearPartyBoxes();
        }

        public static int FindGuildMemberPartyIndex(GuildMember aGuildMember)
        {
            for (int i = 0; i < partyPlateBoxes.Length; i++)
            {
                if (partyPlateBoxes[i].BelongsTo(aGuildMember))
                {
                    return i;
                }

            }

            return -1;
        }

        public static void AddWalkersToControl(GuildMember[] aGuildMembers)
        {
            for (int i = 0; i < aGuildMembers.Length; i++)
            {
                AddWalkerToControl(aGuildMembers[i]);
            }
        }

        public static void AddWalkerToControl(GuildMember aGuildMember)
        {
            int index = FindGuildMemberPartyIndex(aGuildMember);

            if (index == -1) return;

            partyPlateBoxes[index].VisibleBorder = false;
        }
        public static void RemoveWalkersFromControl(GuildMember[] aGuildMembers)
        {
            for (int i = 0; i < aGuildMembers.Length; i++)
            {
                RemoveWalkerFromControl(aGuildMembers[i]);
            }
        }

        public static void RemoveWalkerFromControl(GuildMember aGuildmember)
        {
            int index = FindGuildMemberPartyIndex(aGuildmember);

            if (index == -1) return;

            partyPlateBoxes[index].VisibleBorder = false;
        }

        public static void AddGuildMember(Friendly aData)
        {
            //guildWindow.
        }

        public static void SetGuildMembers(Friendly[] aData)
        {
            guildWindow.SetRoster(aData);
        }

        public static void SetGuildMemberInviteStatus(List<string> aName, List<TwoStateGFXButton.State> aState)
        {
            guildWindow.SetGuildMemberInviteStatus(aName, aState);
        }



        #endregion

        #region Inventory
        public static void SetInventory(Inventory aInventory) => inventoryBox.SetInventory(aInventory);
        public static void RefreshInventorySlot(int aBag, int aSlot, Inventory aInventory) => inventoryBox.RefreshSlot(aBag, aSlot, aInventory);
        public static void RefreshInventorySlot((int, int) aBagAndSlot, Inventory aInventory) => RefreshInventorySlot(aBagAndSlot.Item1, aBagAndSlot.Item2, aInventory);

        public static void SetDescriptorBox(Item aItem) => descriptorBox.SetToItem(aItem);

        #endregion

        #region CharacterAndInspectWindow
        public static void SetCharacterWindow(Player aPlayer) => characterWindow.SetData(aPlayer);
        public static void ToggleCharacterWindow() => characterWindow.ToggleVisibilty();

        public static void RefreshAllCharacterWindowSlots(Equipment aEquipment, Friendly aFriendly)
        {
            for (int i = 0; i < (int)Equipment.Slot.Count; i++)
            {
                RefreshCharacterWindowSlot((Equipment.Slot)i, aEquipment, aFriendly);
            }
        }

        public static void RefreshCharacterWindowSlot(Equipment.Slot aSlot, Equipment aEquipment, Friendly aFriendly)
        {
            if (aFriendly == null) return;
            if (aFriendly.RelationToPlayer == Relation.RelationToPlayer.Self)
            {
                characterWindow.SetSlot(aSlot, aEquipment);
                return;
            }

            if (!inspectWindow.BelongsTo(aFriendly as GuildMember)) return;

            inspectWindow.SetSlot(aSlot, aEquipment);
        }

        public static void RefreshCharacterWindowStats(PairReport aReport, Friendly aFriendly)
        {
            if (aFriendly == null) return;

            if (aFriendly.RelationToPlayer == Relation.RelationToPlayer.Self)
            {
                characterWindow.SetReportBox(aReport);
                return;
            }
            if (!inspectWindow.BelongsTo(aFriendly as GuildMember)) return;

            inspectWindow.SetReportBox(aReport);
        }

        public static void RefreshCharacterWindowExpBar(Friendly aFriendly)
        {
            if (aFriendly.RelationToPlayer == Relation.RelationToPlayer.Self)
            {
                characterWindow.RefreshExp(aFriendly.Level);
                return;
            }

            if (!inspectWindow.BelongsTo(aFriendly as GuildMember)) return;

            inspectWindow.RefreshExp(aFriendly.Level);

        }

        public static GuildMember GetGuildMemberInspectWindowTarget() => inspectWindow.GuildMember;

        public static bool PlayerCharacterPaneOpen => characterWindow.Visible;

        public static void ToggleInspectWindow(GuildMember aGuildMember)
        {
            if (inspectWindow.Visible == true && inspectWindow.BelongsTo(aGuildMember))
            {
                inspectWindow.ToggleVisibilty();
                inspectWindow.RemoveData();
                return;
            }
            inspectWindow.SetData(aGuildMember);
            if (inspectWindow.Visible == false)
            {
                inspectWindow.ToggleVisibilty();
            }
        }

        public static void CloseGuildWindow()
        {
            if (inspectWindow.Visible == false) return;

            inspectWindow.ToggleVisibilty();
        }
        #endregion

        #region Spell
        public static void AddSpellToSpellBook(Spell aSpell) => spellBookWindow.AssignSpell(aSpell);
        public static void RefreshSpellBook(Spell[] aSpells) => spellBookWindow.RefreshSpells(aSpells);
        public static void HoldSpell(Spell aSpell, AbsoluteScreenPosition aGrabOffset) => heldSpell.HoldMe(aSpell, aGrabOffset);
        public static void ReleaseSpell() => heldSpell.ReleaseMe();

        public static void FinishChannel() => playerCastBar.FinishCast();
        public static void CancelChannel() => playerCastBar.CancelCast();
        public static void UpdateChannelSpell(float aNewVal) => playerCastBar.Value = aNewVal;
        public static void ChannelSpell(Spell aSpell) => playerCastBar.CastSpell(aSpell);

        public static void AddBuff(GameObjects.Spells.Buff.Buff aBuff, Entity aEntity)
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

            for (int i = plateBoxes.Count - 1; i >= 0; i--)
            {
                if (plateBoxes[i].ClickedOn(aClickEvent)) return true;
            }

            
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
            foreach (KeyValuePair<Entity, NamePlate> namePlate in namePlates)
            {
                namePlate.Value.Draw(aBatch);
            }

            for (int i = 0; i < plateBoxes.Count; i++)
            {
                plateBoxes[i].Draw(aBatch);
            }

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
