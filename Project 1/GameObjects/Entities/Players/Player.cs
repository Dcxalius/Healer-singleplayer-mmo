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
using Project_1.Textures.AnimatedTextures;
using Project_1.Items;
using Project_1.GameObjects.Spells;
using Project_1.Camera;
using System.Diagnostics;
using Project_1.UI.HUD.Managers;
using Project_1.GameObjects.Unit;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.GameObjects.Entities.GuildMembers;

namespace Project_1.GameObjects.Entities.Players
{
    internal class Player : Friendly
    {
        public override Color MinimapColor => Color.White;
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
            Mailboxes.Ui.Publish(new InventoryAssigned(Inventory));
            party = new Party(this);
            guild = new Guild(this);
            SpellBook.Init(this);

            LoadSpellBar(PlayerData.SavedSpellsOnBar);

            Mailboxes.Ui.Publish(new SpellbookRefreshed(this, SpellBook.Spells));
            Mailboxes.Ui.Publish(new CharacterWindowSet(this));
            Mailboxes.Ui.Publish(new PlayerPlateSet(this));
            Mailboxes.Ui.Publish(new GoldChanged(this, Gold));
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
            Mailboxes.Ui.Publish(new SpellbarLoaded(this, spellsToAddToBar));
        }

        public void GetPartyMembersFromGuild()
        {
            string[] partyMembers = PlayerData.Party;
            for (int i = 0; i < partyMembers.Length; i++)
            {
                GuildMember guildMember = guild.GetGuildMemberByName(partyMembers[i]);
                ObjectManager.SpawnGuildMemberToParty(guildMember, guildMember.FeetPosition);
            }


            Mailboxes.Ui.Publish(new GuildInviteStatusUpdated(partyMembers.ToList(), Enumerable.Repeat(InviteStatus.Accepted, partyMembers.Length).ToList()));
        }

        void KeyboardWalk()
        {
            if (HasDestination && LockedMovement) { return; }
            if (KeyBindStateCache.GetHold(KeyBindManager.KeyListner.MoveCharacterLeft))
            {
                velocity.X -= 1;
            }
            if (KeyBindStateCache.GetHold(KeyBindManager.KeyListner.MoveCharacterRight))
            {
                velocity.X += 1;
            }
            if (KeyBindStateCache.GetHold(KeyBindManager.KeyListner.MoveCharacterUp))
            {
                velocity.Y -= 1;
            }
            if (KeyBindStateCache.GetHold(KeyBindManager.KeyListner.MoveCharacterDown))
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
            Mailboxes.Ui.Publish(new GoldChanged(this, Gold));
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
