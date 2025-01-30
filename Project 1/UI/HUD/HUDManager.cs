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
using Project_1.UI.HUD.PlateBoxes;
using Project_1.UI.HUD.Windows;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Bars;
using Project_1.UI.UIElements.Guild;
using Project_1.UI.UIElements.Inventory;
using Project_1.UI.UIElements.SpellBook;
using SharpDX.MediaFoundation.DirectX;
using SharpDX.XAudio2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Item = Project_1.UI.UIElements.Inventory.Item;

namespace Project_1.UI.HUD
{
    internal static class HUDManager
    {
        static Dictionary<Entity, NamePlate> namePlates = new Dictionary<Entity, NamePlate>();


        static List<UIElement> hudElements = new List<UIElement>();

        static PlayerPlateBox playerPlateBox;
        static TargetPlateBox targetPlateBox;
        static PartyPlateBox[] partyPlateBoxes;
        static BuffBox playerBuffBox;
        static BuffBox targetBuffBox;
        static BuffBox[] partyBuffBoxes = new BuffBox[4];

        static InventoryBox inventoryBox;
        static LootBox lootBox;
        static DescriptorBox descriptorBox;

        static CharacterWindow characterWindow;
        static SpellBookWindow spellBookWindow;
        static GuildWindow guildWindow;

        static CastBar playerCastBar;
        static FirstSpellBar firstSpellBar;



        static HeldItem heldItem;
        static HeldSpell heldSpell;



        public static void Init()
        {
            playerPlateBox = new PlayerPlateBox(new RelativeScreenPosition(0.1f, 0.1f), new RelativeScreenPosition(0.2f, 0.1f));
            hudElements.Add(playerPlateBox);
            targetPlateBox = new TargetPlateBox(new RelativeScreenPosition(0.33f, 0.1f), new RelativeScreenPosition(0.2f, 0.1f));
            hudElements.Add(targetPlateBox);
            partyPlateBoxes = new PartyPlateBox[4];
            for (int i = 0; i < partyPlateBoxes.Length; i++)
            {
                partyPlateBoxes[i] = new PartyPlateBox(i);
            }
            hudElements.AddRange(partyPlateBoxes);


            playerBuffBox = new BuffBox(ObjectManager.Player, BuffBox.FillDirection.TopRightToDown, new RelativeScreenPosition(0.01f, 0.1f), new RelativeScreenPosition(0.08f, 0.1f));
            hudElements.Add(playerBuffBox);
            targetBuffBox = new BuffBox(null, BuffBox.FillDirection.TopRightToDown, new RelativeScreenPosition(0.33f, 0.21f), new RelativeScreenPosition(0.2f, 0.1f));
            hudElements.Add(targetBuffBox);
            partyBuffBoxes = new BuffBox[4];
            RelativeScreenPosition pBuffBoxSize = new RelativeScreenPosition(0.08f, 0.1f);
            for (int i = 0; i < partyBuffBoxes.Length; i++)
            {
                partyBuffBoxes[i] = new BuffBox(null, BuffBox.FillDirection.TopRightToDown, partyPlateBoxes[i].RelativePos - pBuffBoxSize.OnlyX, pBuffBoxSize);
            }
            hudElements.AddRange(partyBuffBoxes);

            lootBox = new LootBox(new RelativeScreenPosition(0.1f, 0.5f), new RelativeScreenPosition(0.4f, 0.4f));
            hudElements.Add(lootBox);
            inventoryBox = new InventoryBox(new RelativeScreenPosition(0.59f, 0.80f), new RelativeScreenPosition(0.4f));
            hudElements.Add(inventoryBox);
            descriptorBox = new DescriptorBox();
            hudElements.Add(descriptorBox);

            Window.Init(new RelativeScreenPosition(0.05f, 0.2f), new RelativeScreenPosition(0.1f, 0f), new RelativeScreenPosition(0.2f, 0.6f));
            characterWindow = new CharacterWindow(ObjectManager.Player);
            hudElements.Add(characterWindow);
            spellBookWindow = new SpellBookWindow();
            hudElements.Add(spellBookWindow);
            guildWindow = new GuildWindow();
            hudElements.Add(guildWindow);

            firstSpellBar = new FirstSpellBar(10, new RelativeScreenPosition(0.2f, 0.86f), 0.6f);
            hudElements.Add(firstSpellBar);
            playerCastBar = new CastBar(new RelativeScreenPosition(0.1f, 0.203f), new RelativeScreenPosition(0.2f, 0.015f));
            hudElements.Add(playerCastBar);

            heldItem = new HeldItem();
            heldSpell = new HeldSpell();
        }
        public static void Update()
        {
            foreach (KeyValuePair<Entity, NamePlate> namePlate in namePlates)
            {
                namePlate.Value.Update(null);
            }

            for (int i = 0; i < hudElements.Count; i++)
            {
                hudElements[i].Update(null);
            }
        }

        public static void Rescale()
        {
            for (int i = 0; i < hudElements.Count; i++)
            {
                hudElements[i].Rescale();
            }
        }

        #region Plate
        public static void AddNamePlate(Entity aEntity, NamePlate aNamePlate)
        {
            namePlates.Add(aEntity, aNamePlate);
        }

        public static void RemoveNamePlate(Entity aEntity)
        {
            namePlates.Remove(aEntity);
        }

        public static void SetPlayerPlateBox(Player aPlayer)
        {

            playerPlateBox.SetData(aPlayer);
        }

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
                        return;
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

        #endregion

        #region Guild
        public static void AddGuildMember(Friendly aData)
        {
            //guildWindow.
        }

        public static void SetGuildMembers(Friendly[] aData)
        {
            guildWindow.SetRoster(aData);
        }


        #endregion



        #region Inventory
        public static void SetInventory(Inventory aInventory) => inventoryBox.SetInventory(aInventory);
        public static void RefreshInventorySlot(int aBag, int aSlot, Inventory aInventory) => inventoryBox.RefreshSlot(aBag, aSlot, aInventory);
        public static void RefreshInventorySlot((int, int) aBagAndSlot, Inventory aInventory) => RefreshInventorySlot(aBagAndSlot.Item1, aBagAndSlot.Item2, aInventory);

        public static void SetDescriptorBox(Item aItem) => descriptorBox.SetToItem(aItem);

        #endregion

        #region CharacterWindow
        public static void SetCharacterWindow(Friendly aFriendly, PairReport aRaport) => characterWindow.SetData(aFriendly, aRaport);

        public static void RefreshCharacterWindowSlot(Equipment.Slot aSlot, Equipment aEquipment) => characterWindow.SetSlot(aSlot, aEquipment);
        public static void RefreshCharacterWindowStats(PairReport aReport) => characterWindow.SetReportBox(aReport);
        public static void RefreshCharacterWindowExpBar(Entity aEntity)
        {
            if (aEntity.GetType() == typeof(Player))
            {
                characterWindow.RefreshExp(aEntity.Level);
            }
        }
        #endregion

        #region Spell
        public static void AddSpellToSpellBook(Spell aSpell) => spellBookWindow.AssignSpell(aSpell);
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
        #endregion

        #region Loot
        public static Items.Item GetLootItem(int aSlotInLoot) => lootBox.GetItem(aSlotInLoot);
        public static void ReduceLootItem(int aSlotInLoot, int aCount) => lootBox.ReduceItem(aSlotInLoot, aCount);
        public static void Loot(Corpse aCorpse) => lootBox.Loot(aCorpse);

        public static void HoldItem(Item aItem, AbsoluteScreenPosition aGrabOffset) => heldItem.HoldItem(aItem, aGrabOffset);
        public static void ReleaseItem() => heldItem.ReleaseMe();
        #endregion


        #region Mouse
        public static bool Click(ClickEvent aClickEvent)
        {
            for (int i = hudElements.Count - 1; i >= 0; i--)
            {
                if (hudElements[i].ClickedOn(aClickEvent)) return true;
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
            for (int i = 0; i < hudElements.Count; i++)
            {
                if (hudElements[i].ScrolledOn(aScrollEvent)) return true;

            }
            return false;
        }

        public static void PauseMenuActivated() //TODO: Find better name
        {
            for (int i = 0; i < hudElements.Count; i++)
            {
                hudElements[i].PauseMenuActivated();
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

            for (int i = 0; i < hudElements.Count; i++)
            {
                hudElements[i].Draw(aBatch);
            }

            heldItem.Draw(aBatch);
            heldSpell.Draw(aBatch);
        }
    }
}
