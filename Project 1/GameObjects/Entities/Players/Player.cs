using Microsoft.Xna.Framework;
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
using Project_1.UI.HUD;
using Project_1.Textures.AnimatedTextures;
using Project_1.Items;
using Project_1.GameObjects.Spells;
using Project_1.Camera;
using Project_1.UI.UIElements.Buttons;

namespace Project_1.GameObjects.Entities.Players
{
    internal class Player : Friendly
    {
        public PlayerData PlayerData => UnitData as PlayerData;
        public Inventory Inventory => PlayerData.Inventory;

        public SpellBook SpellBook => spellBook;
        SpellBook spellBook;

        public Party Party => party;
        Party party;

        public Guild Guild => guild;
        Guild guild;
        public bool LockedMovement => lockedMovement;
        bool lockedMovement = false;

        public Player() : base(ObjectFactory.GetPlayerData())
        {
            HUDManager.SetInventory(Inventory);
            spellBook = new SpellBook(this);
            party = new Party(this);
            guild = new Guild(this);

            HUDManager.SetCharacterWindow(this);
            HUDManager.SetPlayerPlateBox(this);
        }

        public override void Update()
        {
            KeyboardWalk();
            Party.Update();
            base.Update();
        }

        public void GetPartyMembersFromGuild()
        {
            string[] partyMembers = PlayerData.Party;
            for (int i = 0; i < partyMembers.Length; i++)
            {
                GuildMember guildMember = guild.GetGuildMemberByName(partyMembers[i]);
                ObjectManager.SpawnGuildMemberToParty(guildMember, guildMember.FeetPosition);
            }


            HUDManager.SetGuildMemberInviteStatus(partyMembers.ToList(), Enumerable.Repeat(TwoStateGFXButton.State.Second, partyMembers.Length).ToList());
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
