using Project_1.Camera;
using Project_1.GameObjects.Unit;
using Project_1.Input;
using Project_1.UI.HUD;
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

        Entity owner;
        List<GuildMember> commands = new List<GuildMember>();

        public int PartyCount => party.Count;
        List<GuildMember> party = new List<GuildMember>();
        const float lengthOfLeash = 500;

        public bool IsInCommand(GuildMember aGuildMember) => commands.IndexOf(aGuildMember) >= 0;
        public bool IsInParty(GuildMember aGuildMember) => party.IndexOf(aGuildMember) >= 0;

        public bool IsInCombat => party.Any(x => x.InCombat);

        public Party(Entity aOwner)
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
            HUDManager.RemoveWalkersFromControl(commands.ToArray());
            commands.Clear();
        }

        public void AddToCommand(GuildMember aGuildMember)
        {
            if (commands.Contains(aGuildMember)) { return; }

            HUDManager.AddWalkerToControl(aGuildMember);
            commands.Add(aGuildMember);
        }

        public void NeedyAddToCommand(GuildMember aGuildMember)
        {
            commands.Clear();
            AddToCommand(aGuildMember);

        }

        public void RemoveFromCommand(GuildMember aGuildMember)
        {
            if (!commands.Contains(aGuildMember)) { return; }

            HUDManager.RemoveWalkersFromControl(new GuildMember[] { aGuildMember });
            commands.Remove(aGuildMember);
        }

        public bool AddToParty(GuildMember aGuildMember)
        {
            if (PartyCount >= maxPartySize) return false;

            party.Add(aGuildMember);
            aGuildMember.AddedToParty();

            HUDManager.AddGuildMemberToParty(party[party.Count - 1]);
            return true;
        }

        public bool RemoveFromParty(GuildMember aGuildMember)
        {
            Debug.Assert(PartyCount > 0);
            Debug.Assert(aGuildMember != null);
            Debug.Assert(IsInParty(aGuildMember));

            HUDManager.RemoveGuildMemberFromParty(aGuildMember);
            party.Remove(aGuildMember);
            aGuildMember.RemovedFromParty();
            return true;
        }

        public void IssueMoveOrder(ClickEvent aClick)
        {
            WorldSpace worldPosDestination = WorldSpace.FromRelativeScreenSpace(aClick.RelativePos);
            foreach (var walker in commands)
            {
                if (aClick.Modifier(InputManager.HoldModifier.Shift))
                {
                    walker.AddWalkingOrder(worldPosDestination);
                }
                else
                {
                    walker.RecieveDirectWalkingOrder(worldPosDestination);

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
    }

}
