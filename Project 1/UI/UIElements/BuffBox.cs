using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects.Entities;
using Project_1.Textures;
using Project_1.UI.UIElements.Boxes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements
{
    internal class BuffBox : Box
    {
        public enum FillDirection
        {
            TopRightToDown
        }

        FillDirection fillDirection;
        List<Buff> buffs;

        Entity owner;


        RelativeScreenPosition StartPosition
        {
            get
            {
                switch (fillDirection)
                {
                    case FillDirection.TopRightToDown:
                        return new RelativeScreenPosition(RelativeSize.X - buffSize.X - spacing.X, spacing.Y);
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        RelativeScreenPosition buffSize = RelativeScreenPosition.GetSquareFromX(0.015f);
        RelativeScreenPosition spacing = RelativeScreenPosition.GetSquareFromX(0.005f);
        RelativeScreenPosition textSpacing = new RelativeScreenPosition(0, 0.007f);

        public BuffBox(Entity aOwner, FillDirection aDir, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(UITexture.Null, aPos, aSize)
        {
            owner = aOwner;
            buffs = new List<Buff>();
            AddChildren(buffs);
            capturesClick = false;
        }
        public bool IsThisMine(Entity aOwner) => aOwner == owner;

        public void AssignBox(Entity aOwner)
        {
            owner = aOwner;
            if (aOwner == null)
            {
                ClearBuffs();
                Visible = false;
                return;
            }
            SetAllBuffs(aOwner.GetAllBuffs());
            Visible = true;
        }

        void ClearBuffs()
        {
            buffs.Clear();
            KillAllChildren();
        }

        void SetAllBuffs(List<GameObjects.Spells.Buff.Buff> aBuff)
        {
            ClearBuffs();
            for (int i = 0; i < aBuff.Count; i++)
            {
                buffs.Add(new Buff(aBuff[i], RelativeScreenPosition.Zero, buffSize));
            }

            AddChildren(buffs);
            SortBuffs();
        }

        public void AddBuff(GameObjects.Spells.Buff.Buff aBuff)
        {
            buffs.Add(new Buff(aBuff, RelativeScreenPosition.Zero, buffSize));
            AddChild(buffs.Last());
            SortBuffs();
        }

        public override void Update(in UIElement aParent)
        {
            base.Update(aParent);


            CheckLast();
        }

        void CheckLast()
        {
            if (buffs.Count == 0) return;
            if (buffs.Last().Duration <= 0)
            {
                KillChild(buffs.Count - 1);
                buffs.RemoveAt(buffs.Count - 1);
                CheckLast();
            }
        }

        void SortBuffs()
        {
            buffs.Sort();
            RelativeScreenPosition pos = StartPosition;
            int maxInX = (int)Math.Floor(RelativeSize.X / (buffSize.X + spacing.X + textSpacing.X) + spacing.X);
            int maxInY = (int)Math.Floor(RelativeSize.Y / (buffSize.Y + spacing.Y + textSpacing.Y) + spacing.Y);

            if (maxInX * maxInY < buffs.Count) throw new NotImplementedException(); //TODO: You probably should have fixed this by now xdd

            for (int i = 0; i < buffs.Count; i++)
            {
                buffs[i].Move(pos + StepPosition(i, maxInX, maxInY));
            }
        }

        RelativeScreenPosition StepPosition(int i, int aMaxX, int aMaxY)
        {
            switch (fillDirection)
            {
                case FillDirection.TopRightToDown:
                    RelativeScreenPosition step = RelativeScreenPosition.Zero;
                    step.X = (float)((buffSize.X + spacing.X + textSpacing.X) * -Math.Floor((double)i / aMaxY));
                    step.Y = ((buffSize.Y + spacing.Y + textSpacing.Y) * (i % aMaxY)); 
                    return step;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
