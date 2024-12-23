﻿using Microsoft.Xna.Framework;
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
using Project_1.Input;
using Project_1.Tiles;
using Project_1.UI.HUD;
using Project_1.Textures.AnimatedTextures;
using Project_1.Items;
using Project_1.GameObjects.Spells;
using Project_1.Camera;
using Project_1.GameObjects.Entities.Temp;

namespace Project_1.GameObjects.Entities.Players
{
    internal class Player : Entity
    {
        public Inventory Inventory { get => inventory; }
        Inventory inventory;

        public SpellBook SpellBook { get => spellBook; }
        SpellBook spellBook;
       
        List<Walker> commands = new List<Walker>();
        List<Walker> party = new List<Walker>();

        const float lengthOfLeash = 500;

        public override Entity Target { get => playerClickTarget; set => playerClickTarget = value; }
        Entity playerClickTarget;

        public bool LockedMovement => lockedMovement;
        bool lockedMovement = false;

        public Player(WorldSpace aStartPos) : base(new RandomAnimatedTexture(new GfxPath(GfxType.Object, "Player"), new Point(32), 0, TimeSpan.FromMilliseconds(500)), aStartPos)
        {
            inventory = new Inventory(this);
            spellBook = new SpellBook(this);
        }


        public bool IsInCommand(Walker aWalker) { return commands.IndexOf(aWalker) >= 0; }
        public bool IsInParty(Walker aWalker) { return party.IndexOf(aWalker) >= 0; }

        void KeyboardWalk()
        {
            if (HasDestination) { return; }
            if (KeyBindManager.GetHold(KeyBindManager.KeyListner.MoveCharacterLeft))
            {
                velocity.X -= 1;
            }
            if (KeyBindManager.GetHold(KeyBindManager.KeyListner.MoveCharacterRight))
            {
                velocity.X += 1;
            }
            if (KeyBindManager.GetHold(KeyBindManager.KeyListner.MoveCharacterUp))
            {
                velocity.Y -= 1;
            }
            if (KeyBindManager.GetHold(KeyBindManager.KeyListner.MoveCharacterDown))
            {
                velocity.Y += 1;
            }

            if (velocity == WorldSpace.Zero) return;
            velocity.Normalize();
            velocity *= (float)(UnitData.MovementData.Speed * TimeManager.SecondsSinceLastFrame);
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
                if (party[i].HasDestination == false && (FeetPosition - party[i].FeetPosition).ToVector2().Length() > lengthOfLeash)
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
                commands[i].Target = aEntity;
            }
        }
    }
}
