using Microsoft.Xna.Framework;
using Project_1.Input;
using Project_1.Textures;
using Project_1.Textures.AnimatedTextures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Entities
{
    internal class Walker : Entity
    {
        public Walker(Vector2 aStartingPos) : base(new RandomAnimatedTexture(new GfxPath(GfxType.Object, "Walker"), new Point(32), 0, TimeSpan.FromMilliseconds(500)), aStartingPos)
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

        public void RecieveDirectWalkingOrder(Vector2 aPos)
        {
            target = null;
            OverwriteDestination(aPos);
        }


        public void AddWalkingOrder(Vector2 aPos)
        {
            AddDestination(aPos);
        }

    }
}
