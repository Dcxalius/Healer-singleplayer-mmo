﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_1.Managers;
using Project_1.Textures;
using Project_1.Input;
using Project_1.Tiles;
using Project_1.Textures.AnimatedTextures;
using Project_1.Items;
using Project_1.GameObjects.Spells;
using Project_1.Camera;
using Project_1.UI.UIElements.Buttons;
using System.Diagnostics;
using Project_1.UI.HUD.Managers;
using Project_1.GameObjects.Unit;

namespace Project_1.GameObjects.Entities.Players
{
    internal class Player : Friendly
    {
        public PlayerData PlayerData => UnitData as PlayerData;
        public Inventory Inventory => PlayerData.Inventory;

        public SpellBook SpellBook => PlayerData.SpellBook;

        public Party Party => party;
        Party party;

        public Guild Guild => guild;
        Guild guild;
        public bool LockedMovement => lockedMovement;
        bool lockedMovement = false;

        public int Gold => PlayerData.Gold;


        public bool InCombatOrPartyInCombat => party.IsInCombat || InCombat;

        public Player(string aName, string aClassName) /*Change class to be a class*/ : this(new PlayerData(aName, aClassName))
        {

        }

        public Player(PlayerData aPlayerData) : base(aPlayerData)
        {
            HUDManager.SetInventory(Inventory);
            party = new Party(this);
            guild = new Guild(this);
            SpellBook.Init(this);

            LoadSpellBar(PlayerData.SavedSpellsOnBar);

            HUDManager.windowHandler.RefreshSpellBook(SpellBook.Spells);
            HUDManager.windowHandler.SetCharacterWindow(this);
            HUDManager.plateBoxHandler.SetPlayerPlateBox(this);
            HUDManager.RefreshGold(Gold);
        }

        public override void Update()
        {
            KeyboardWalk();
            Party.Update();
            base.Update();
        }

        void LoadSpellBar(string[] aSpellOnBar)
        {
            if (aSpellOnBar == null) return;
            Spell[] spells = SpellBook.Spells;
            int?[] indexOfSpellsToAdd = new int?[aSpellOnBar.Length];
            for (int i = 0; i < aSpellOnBar.Length; i++)
            {
                if (aSpellOnBar[i] == null) continue;

                int indexOfSpell = Array.FindIndex(spells, x => x.Name == aSpellOnBar[i]);
                Debug.Assert(indexOfSpell >= 0);

                indexOfSpellsToAdd[i] = indexOfSpell;
            }
            Spell[] spellsToAddToBar = new Spell[indexOfSpellsToAdd.Length];
            for (int i = 0; i < indexOfSpellsToAdd.Length; i++)
            {
                if (!indexOfSpellsToAdd[i].HasValue) continue;
                spellsToAddToBar[i] = spells[indexOfSpellsToAdd[i].Value];
            }
            HUDManager.LoadSpellBar(spellsToAddToBar);
        }

        public void GetPartyMembersFromGuild()
        {
            string[] partyMembers = PlayerData.Party;
            for (int i = 0; i < partyMembers.Length; i++)
            {
                GuildMember guildMember = guild.GetGuildMemberByName(partyMembers[i]);
                ObjectManager.SpawnGuildMemberToParty(guildMember, guildMember.FeetPosition);
            }


            HUDManager.windowHandler.SetGuildMemberInviteStatus(partyMembers.ToList(), Enumerable.Repeat(TwoStateGFXButton.State.Second, partyMembers.Length).ToList());
        }

        void KeyboardWalk()
        {
            if (HasDestination) { return; }
            if (KeyBindManager.GetHold(KeyBindManager.KeyListner.MoveCharacterLeft))
            {
                velocity.X -= 1;
            }
            if (KeyBindManager.GetHold(KeyBindManager.KeyListner.MoveCharacterRight))
            {
                velocity.X += 1;
            }
            if (KeyBindManager.GetHold(KeyBindManager.KeyListner.MoveCharacterUp))
            {
                velocity.Y -= 1;
            }
            if (KeyBindManager.GetHold(KeyBindManager.KeyListner.MoveCharacterDown))
            {
                velocity.Y += 1;
            }

            if (velocity == WorldSpace.Zero) return;
            velocity.Normalize();
            velocity *= (float)(UnitData.MovementData.Speed * TimeManager.SecondsSinceLastFrame);
        }

        public void ChangeGold(int aAmount)
        {
            PlayerData.Gold += aAmount;
            HUDManager.RefreshGold(Gold);
        }

        protected override bool CheckForRelation()
        {
            if (target.RelationToPlayer == Unit.Relation.RelationToPlayer.Self || target.RelationToPlayer == Unit.Relation.RelationToPlayer.Friendly)
            {
                return false;
            }
            if (target.RelationToPlayer != RelationToPlayer)
            {
                return true;
            }

            return false;
        }

        public override void ExpToParty(int aExpAmount)
        {
            party.ExpToParty(aExpAmount);
        }
    }
}
