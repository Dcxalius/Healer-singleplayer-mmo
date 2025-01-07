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

        public void AddToControl(Player aPlayer)
        {
            aPlayer.AddToCommand(this);
        }

        public void NeedyControl(Player aPlayer)
        {
            aPlayer.NeedyAddToCommand(this);
        }


        protected override void ClickedOn(ClickEvent aClickEvent)
        {
            base.ClickedOn(aClickEvent);

            if (aClickEvent.Modifier(InputManager.HoldModifier.Shift))
            {
                AddToControl(ObjectManager.Player);
            }
            else if (aClickEvent.Modifier(InputManager.HoldModifier.Ctrl))
            {
                NeedyControl(ObjectManager.Player);
            }
        }

        public void RecieveDirectWalkingOrder(WorldSpace aPos)
        {
            target = null;
            destination.OverwriteDestination(aPos);
        }


        public void AddWalkingOrder(WorldSpace aPos)
        {
            destination.AddDestination(aPos);
        }

    }
}
