using Newtonsoft.Json.Bson;
using Project_1.Camera;
using Project_1.GameObjects.Unit;
using Project_1.GameObjects.Entities.GuildMembers;
using Project_1.UI.HUD.Managers;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Entities.Players
{
    internal class Party
    {
        public const int maxPartySize = 4;

        Player owner;
        List<GuildMembers.GuildMember> commands = new List<GuildMembers.GuildMember>();

        public int PartyCount => party.Count;
        List<GuildMembers.GuildMember> party = new List<GuildMembers.GuildMember>();
        const float lengthOfLeash = 500;

        public bool IsInCommand(GuildMembers.GuildMember aGuildMember) => commands.IndexOf(aGuildMember) >= 0;
        public bool IsInParty(GuildMembers.GuildMember aGuildMember) => party.IndexOf(aGuildMember) >= 0;

        public bool IsInCombat => party.Any(x => x.InCombat);

        public WorldSpace[] GetPositions
        {
            get
            {
                WorldSpace[] pos = new WorldSpace[5];
                pos[0] = owner.FeetPosition;
                for (int i = 0; i < party.Count; i++)
                {
                    pos[i+1] = party[i].FeetPosition;
                }
                return pos;
            }
        }

        public Party(Player aOwner)
        {
            owner = aOwner;
        }

        public void Update()
        {
            SummonPartyIfTooFarAway();
        }

        void SummonPartyIfTooFarAway()
        {
            for (int i = 0; i < party.Count; i++)
            {
                if (party[i].HasDestination == false && (owner.FeetPosition - party[i].FeetPosition).ToVector2().Length() > lengthOfLeash)
                {
                    party[i].SetTarget(ObjectManager.Player);
                }
            }
        }

        public void ClearCommand()
        {
            Mailboxes.Ui.Publish(new PartyControlCleared(commands.ToArray()));
            commands.Clear();
        }

        public void AddToCommand(GuildMembers.GuildMember aGuildMember)
        {
            if (commands.Contains(aGuildMember)) { return; }

            Mailboxes.Ui.Publish(new PartyWalkerAdded(aGuildMember));
            commands.Add(aGuildMember);
        }

        public void NeedyAddToCommand(GuildMembers.GuildMember aGuildMember)
        {
            commands.Clear();
            AddToCommand(aGuildMember);

        }

        public void RemoveFromCommand(GuildMembers.GuildMember aGuildMember)
        {
            if (!commands.Contains(aGuildMember)) { return; }

            Mailboxes.Ui.Publish(new PartyWalkerRemoved(aGuildMember));
            commands.Remove(aGuildMember);
        }

        public bool AddToParty(GuildMembers.GuildMember aGuildMember)
        {
            if (PartyCount >= maxPartySize) return false;

            party.Add(aGuildMember);
            aGuildMember.AddedToParty();

            Mailboxes.Ui.Publish(new PartyMemberAdded(party[party.Count - 1]));
            return true;
        }

        public bool RemoveFromParty(GuildMembers.GuildMember aGuildMember)
        {
            Debug.Assert(PartyCount > 0);
            Debug.Assert(aGuildMember != null);
            Debug.Assert(IsInParty(aGuildMember));

            Mailboxes.Ui.Publish(new PartyMemberRemoved(aGuildMember));
            party.Remove(aGuildMember);
            aGuildMember.RemovedFromParty();
            return true;
        }

        public void IssueMoveOrder(WorldSpace destination, bool append)
        {
            foreach (var walker in commands)
            {
                if (append)
                {
                    walker.AddWalkingOrder(destination);
                }
                else
                {
                    walker.RecieveDirectWalkingOrder(destination);

                }
            }
        }


        public void IssueTargetOrder(Entity aEntity)
        {
            for (int i = 0; i < commands.Count; i++)
            {
                commands[i].SetTarget(aEntity);
            }
        }

        public void ExpToParty(int aExpAmount)
        {
            if (PartyCount == 0)
            {
                owner.GainExperience(aExpAmount);
                return;
            }

            DivideExpAmongParty(aExpAmount);
        }

        public void DivideExpAmongParty(int aExpAmount)
        {
            int dividedExp = aExpAmount / (PartyCount + 1);
            int bonusExp = 0;//TODO: Check what bonus exp should be
            dividedExp += bonusExp;
            for (int i = 0; i < party.Count; i++)
            {
                party[i].GainExperience(dividedExp);
            }

            owner.GainExperience(dividedExp);
        }
        public void GoldToParty(int aGoldAmount)
        {
            if (PartyCount == 0)
            {
                owner.ChangeGold(aGoldAmount);
                return;
            }

            owner.ChangeGold(aGoldAmount / (PartyCount + 1));
        }
    }
}
