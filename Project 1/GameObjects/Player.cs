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

        List<Walker> commands = new List<Walker>();
        List<Walker> party = new List<Walker>();

        const float lengthOfLeash = 500;

        public Player(Vector2 aStartPos) : base(new AnimatedTexture(new GfxPath(GfxType.Object, "Player"), new Point(32), AnimatedTexture.AnimationType.Random, 0, TimeSpan.FromMilliseconds(500)), aStartPos)
        {
        }

        public bool IsInCommand(Walker aWalker) { return commands.IndexOf(aWalker) >= 0; }
        public bool IsInParty(Walker aWalker) { return party.IndexOf(aWalker) >= 0; }

        void KeyboardWalk()
        {
            if (HasDestination) { return; }
            if (InputManager.GetHold(Keys.Left))
            {
                velocity.X -= 1;
            }
            if (InputManager.GetHold(Keys.Right))
            {
                velocity.X += 1;
            }
            if (InputManager.GetHold(Keys.Up))
            {
                velocity.Y -= 1;
            }
            if (InputManager.GetHold(Keys.Down))
            {
                velocity.Y += 1;
            }

            if (velocity == Vector2.Zero) return;
            velocity.Normalize();
            velocity *= (float)(Data.Speed * TimeManager.SecondsSinceLastFrame);
        }

        public override void Update()
        {
            KeyboardWalk();
            base.Update();
            SummonPartyIfTooFarAway();
        }

        void SummonPartyIfTooFarAway()
        {
            for (int i = 0; i < party.Count; i++)
            {
                if (party[i].HasDestination == false && (FeetPos - party[i].FeetPos).Length() > lengthOfLeash)
                {
                    party[i].Target = ObjectManager.Player;
                }
            }
        }

        public void ClearCommand()
        {
            HUDManager.RemoveWalkersFromControl(commands.ToArray());
            commands.Clear();
        }

        public void AddToCommand(Walker aWalker)
        {
            if (commands.Contains(aWalker)) { return; }

            HUDManager.AddWalkerToControl(aWalker);
            commands.Add(aWalker);
        }

        public void NeedyAddToCommand(Walker aWalker)
        {
            commands.Clear();
            AddToCommand(aWalker);

        }

        public void RemoveFromCommand(Walker aWalker)
        {
            if (!commands.Contains(aWalker)) { return; }

            HUDManager.RemoveWalkersFromControl(new Walker[] { aWalker });
            commands.Remove(aWalker);
        }

        public void AddToParty(Walker aWalker)
        {
            party.Add(aWalker);
            HUDManager.AddWalkerToParty(party[party.Count - 1]);
        }

        public void IssueMoveOrder(ClickEvent aClick)
        {
            Vector2 worldPosDestination = Camera.CameraSpaceToWorldPos(aClick.RelativePos);
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
                commands[i].Target = aEntity;
            }
        }
    }
}
