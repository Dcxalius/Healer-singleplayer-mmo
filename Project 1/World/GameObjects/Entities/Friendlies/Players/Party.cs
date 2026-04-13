using Newtonsoft.Json.Bson;
using Project_1.Camera;
using Project_1.GameObjects.Unit;
using Project_1.UI.HUD.Managers;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_1.GameObjects.Entities.Friendlies.GuildMembers;
using Project_1.GameObjects.Entities.Friendlies.Players;

namespace Project_1.GameObjects.Entities.Friendlies.Players
{
    internal class Party
    {
        public const int maxPartySize = 4;

        Player owner;
        List<GuildMember> commands = new List<GuildMember>();

        public int PartyCount => party.Count;
        List<GuildMember> party = new List<GuildMember>();
        const float lengthOfLeash = 500;

        public bool IsInCommand(GuildMember aGuildMember) => commands.IndexOf(aGuildMember) >= 0;
        public bool IsInParty(GuildMember aGuildMember) => party.IndexOf(aGuildMember) >= 0;

        public bool IsInCombat => party.Any(x => x.InCombat);

        public int CopyPositions(WorldSpace[] destination)
        {
            ThreadAffinity.AssertSimThread();
            Debug.Assert(destination != null);
            if (destination == null || destination.Length == 0)
            {
                return 0;
            }

            int count = Math.Min(destination.Length, party.Count + 1);
            destination[0] = owner.FeetPosition;
            for (int i = 1; i < count; i++)
            {
                destination[i] = party[i - 1].FeetPosition;
            }

            return count;
        }

        public Party(Player aOwner)
        {
            ThreadAffinity.AssertSimThread();
            owner = aOwner;
        }

        public void Update()
        {
            ThreadAffinity.AssertSimThread();
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
            ThreadAffinity.AssertSimThread();
            if (commands.Count > PartyControlCleared.MaxMembers)
            {
                Debug.Assert(false, $"Expected at most {PartyControlCleared.MaxMembers} command members but found {commands.Count}.");
            }

            int count = Math.Min(commands.Count, PartyControlCleared.MaxMembers);
            int renderId0 = count > 0 ? commands[0].RenderId : 0;
            int renderId1 = count > 1 ? commands[1].RenderId : 0;
            int renderId2 = count > 2 ? commands[2].RenderId : 0;
            int renderId3 = count > 3 ? commands[3].RenderId : 0;
            MailboxManager.PublishUiEvent(new PartyControlCleared(count, renderId0, renderId1, renderId2, renderId3));
            commands.Clear();
        }

        public void AddToCommand(GuildMember aGuildMember)
        {
            ThreadAffinity.AssertSimThread();
            if (commands.Contains(aGuildMember)) { return; }

            MailboxManager.PublishUiEvent(new PartyWalkerAdded(aGuildMember.RenderId));
            commands.Add(aGuildMember);
        }

        public void NeedyAddToCommand(GuildMember aGuildMember)
        {
            ThreadAffinity.AssertSimThread();
            commands.Clear();
            AddToCommand(aGuildMember);

        }

        public void RemoveFromCommand(GuildMember aGuildMember)
        {
            ThreadAffinity.AssertSimThread();
            if (!commands.Contains(aGuildMember)) { return; }

            MailboxManager.PublishUiEvent(new PartyWalkerRemoved(aGuildMember.RenderId));
            commands.Remove(aGuildMember);
        }

        public bool AddToParty(GuildMember aGuildMember)
        {
            ThreadAffinity.AssertSimThread();
            if (PartyCount >= maxPartySize) return false;

            party.Add(aGuildMember);
            aGuildMember.AddedToParty();

            MailboxManager.PublishUiEvent(new PartyMemberAdded(party[party.Count - 1].BuildUiSnapshot()));
            return true;
        }

        public bool RemoveFromParty(GuildMember aGuildMember)
        {
            ThreadAffinity.AssertSimThread();
            Debug.Assert(PartyCount > 0);
            Debug.Assert(aGuildMember != null);
            Debug.Assert(IsInParty(aGuildMember));

            MailboxManager.PublishUiEvent(new PartyMemberRemoved(aGuildMember.RenderId));
            party.Remove(aGuildMember);
            aGuildMember.RemovedFromParty();
            return true;
        }

        public void IssueMoveOrder(WorldSpace destination, bool append)
        {
            ThreadAffinity.AssertSimThread();
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
            ThreadAffinity.AssertSimThread();
            for (int i = 0; i < commands.Count; i++)
            {
                commands[i].SetTarget(aEntity);
            }
        }

        public void ExpToParty(int aExpAmount)
        {
            ThreadAffinity.AssertSimThread();
            if (aExpAmount == 0) return;
            if (PartyCount == 0)
            {
                owner.GainExperience(aExpAmount);
                return;
            }

            DivideExpAmongParty(aExpAmount);
        }

        public void DivideExpAmongParty(int aExpAmount)
        {
            ThreadAffinity.AssertSimThread();
            //TODO: Calculate level penalty multiplier as well, currently the party exp is based on the average level, is this correct?
            double multiplier = party.Count switch
            {
                1 => 1,
                2 => 1.166,
                3 => 1.3,
                4 => 1.4,
                _ => throw new Exception("How did you get here?")
            };
            //TODO: Ponder if exp should be double or float
            int groupExp = (int)(aExpAmount * multiplier);
            int dividedExp = groupExp / (PartyCount + 1);
            for (int i = 0; i < party.Count; i++)
            {
                party[i].GainExperience(dividedExp);
            }

            owner.GainExperience(dividedExp);
        }
        public void GoldToParty(int aGoldAmount)
        {
            ThreadAffinity.AssertSimThread();
            if (PartyCount == 0)
            {
                owner.ChangeGold(aGoldAmount);
                return;
            }

            owner.ChangeGold(aGoldAmount / (PartyCount + 1));
        }
    }
}
