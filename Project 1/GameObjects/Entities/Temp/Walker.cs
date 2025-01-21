using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects.Entities.Players;
using Project_1.Input;
using Project_1.Textures;
using Project_1.Textures.AnimatedTextures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Entities.Temp
{
    internal class Walker : Entity
    {
        public Walker(WorldSpace aStartingPos) : base(ObjectFactory.GetData("Walker"), aStartingPos)
        {
        }

        protected override void ClickedOn(ClickEvent aClickEvent)
        {
            base.ClickedOn(aClickEvent);
            Command(aClickEvent);


        }

        public void Command(ClickEvent aClickEvent)
        {
            Party party = ObjectManager.Player.Party;
            if (!party.IsInParty(this)) return;
            if (aClickEvent.Modifier(InputManager.HoldModifier.Shift))
            {
                party.AddToCommand(this);
            }
            else if (aClickEvent.Modifier(InputManager.HoldModifier.Ctrl))
            {
                party.NeedyAddToCommand(this);
            }

        }

        public void RecieveDirectWalkingOrder(WorldSpace aPos)
        {
            target = null;
            destination.OverwriteDestination(aPos);
        }


        public void AddWalkingOrder(WorldSpace aPos) => destination.AddDestination(aPos);

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
            Party party = ObjectManager.Player.Party;
            if (!party.IsInParty(this))
            {
                GainExperience(aExpAmount);
                return;
            }

            party.DivideExpAmongParty(aExpAmount);

        }
    }
}
