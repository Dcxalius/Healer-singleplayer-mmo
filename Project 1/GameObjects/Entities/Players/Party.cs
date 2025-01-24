using Project_1.Camera;
using Project_1.GameObjects.Unit;
using Project_1.Input;
using Project_1.UI.HUD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Entities.Players
{
    internal class Party
    {
        Entity owner;
        List<GuildMember> commands = new List<GuildMember>();

        public int PartyCount => party.Count;
        List<GuildMember> party = new List<GuildMember>();
        const float lengthOfLeash = 500;

        public bool IsInCommand(GuildMember aWalker) { return commands.IndexOf(aWalker) >= 0; }
        public bool IsInParty(GuildMember aWalker) { return party.IndexOf(aWalker) >= 0; }

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

        public void AddToCommand(GuildMember aWalker)
        {
            if (commands.Contains(aWalker)) { return; }

            HUDManager.AddWalkerToControl(aWalker);
            commands.Add(aWalker);
        }

        public void NeedyAddToCommand(GuildMember aWalker)
        {
            commands.Clear();
            AddToCommand(aWalker);

        }

        public void RemoveFromCommand(GuildMember aWalker)
        {
            if (!commands.Contains(aWalker)) { return; }

            HUDManager.RemoveWalkersFromControl(new GuildMember[] { aWalker });
            commands.Remove(aWalker);
        }

        public bool AddToParty(GuildMember aWalker)
        {
            if (PartyCount >= 5) return false;

            party.Add(aWalker);
            HUDManager.AddWalkerToParty(party[party.Count - 1]);
            return true;
        }

        public void IssueMoveOrder(ClickEvent aClick)
        {
            WorldSpace worldPosDestination = WorldSpace.FromRelaticeScreenSpace(aClick.RelativePos);
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
