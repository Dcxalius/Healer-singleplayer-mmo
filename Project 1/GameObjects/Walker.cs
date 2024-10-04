using Microsoft.Xna.Framework;
using Project_1.Input;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects
{
    internal class Walker : Entity
    {
        public Walker(Vector2 aStartingPos) : base(new Textures.AnimatedTexture(new GfxPath(GfxType.Object, "Walker"), new Point(32), Textures.AnimatedTexture.AnimationType.Random, 0, TimeSpan.FromMilliseconds(500)), aStartingPos, 100)
        {
        }

        public override void Update()
        {
            base.Update();


        }

        public void AddToControl(Player aPlayer)
        {
            aPlayer.AddToCommand(this);
        }

        public void NeedyControl(Player aPlayer)
        {
            aPlayer.NeedyAddToCommand(this);
        }

        public override bool Click(ClickEvent aClickEvent)
        {
            return base.Click(aClickEvent);
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

        protected override void AddToAggroTable(Entity aAttacker, float aDamageTaken)
        {
            
        }
    }
}
