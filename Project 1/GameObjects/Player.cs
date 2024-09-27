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
using Project_1.GameObjects;
using Project_1.Input;
using System.Runtime.CompilerServices;
using Project_1.Tiles;
using Project_1.UI.HUD;

namespace Project_1.GameObjects
{
    internal class Player : Entity
    {
        public List<Walker> commands = new List<Walker>();
        public List<Walker> party = new List<Walker>();

        public Player() : base(new AnimatedTexture(new GfxPath(GfxType.Object, "Player"), new Point(32), AnimatedTexture.AnimationType.Random, 0, TimeSpan.FromMilliseconds(500)), new Vector2(100,100), 100)
        {
        }


        void MouseWalk()
        {
            if (HasDestination) { return; }
            if (InputManager.GetHold(Keys.Left))
            {
                velocity.X -= (float)(Data.Speed * TimeManager.SecondsSinceLastFrame);
            }
            if (InputManager.GetHold(Keys.Right))
            {
                velocity.X += (float)(Data.Speed * TimeManager.SecondsSinceLastFrame);
            }
            if (InputManager.GetHold(Keys.Up))
            {
                velocity.Y -= (float)(Data.Speed * TimeManager.SecondsSinceLastFrame);
            }
            if (InputManager.GetHold(Keys.Down))
            {
                velocity.Y += (float)(Data.Speed * TimeManager.SecondsSinceLastFrame);
            }
        }

        public override void Update()
        {
            MouseWalk();

            base.Update();
        }

        public void ClearCommand()
        {
            commands.Clear();
        }

        public void AddToCommand(Walker aWalker)
        {
            if (commands.Contains(aWalker)) { return; }

            commands.Add(aWalker);
        }

        public void RemoveFromCommand(Walker aWalker)
        {
            if (!commands.Contains(aWalker)) { return; }

            commands.Remove(aWalker);
        }

        public void AddToParty(Walker aWalker)
        {
            party.Add(aWalker);
            HUDManager.AddWalkerToParty(party[party.Count - 1]);
        }

        public void IssueMoveOrder(ClickEvent aClick)
        {
            Vector2 worldPosDestination = Camera.CameraSpaceToWorldPos(aClick.ClickPos);
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
    }
}
