using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Spells;
using Project_1.Textures;
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


        Vector2 StartPosition
        {
            get
            {
                switch (fillDirection)
                {
                    case FillDirection.TopRightToDown:
                        return new Vector2(RelativeSize.X - buffSize.X - spacing.X, spacing.Y);
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        Vector2 buffSize = Camera.Camera.GetRelativeXSquare(0.015f);
        Vector2 spacing = Camera.Camera.GetRelativeXSquare(0.005f);
        Vector2 textSpacing = new Vector2(0, 0.007f);

        public BuffBox(Entity aOwner, FillDirection aDir, Vector2 aPos, Vector2 aSize) : base(UITexture.Null, aPos, aSize)
        {
            owner = aOwner;
            buffs = new List<Buff>();
            children.AddRange(buffs);
        }
        public bool IsThisMine(Entity aOwner) => aOwner == owner;

        public void AssignBox(Entity aOwner)
        {
            owner = aOwner;
            if (aOwner == null)
            {
                ClearBuffs();
                return;
            }
            SetAllBuffs(aOwner.GetAllBuffs());
        }

        void ClearBuffs()
        {
            buffs.Clear();
            children.Clear();

        }

        void SetAllBuffs(List<GameObjects.Spells.Buff> aBuff)
        {
            ClearBuffs();
            for (int i = 0; i < aBuff.Count; i++)
            {
                buffs.Add(new Buff(aBuff[i], Vector2.Zero, buffSize));
            }
            children.AddRange(buffs);
            SortBuffs();
        }

        public void AddBuff(GameObjects.Spells.Buff aBuff)
        {
            buffs.Add(new Buff(aBuff, Vector2.Zero, buffSize));
            children.Add(buffs.Last());
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
                children.Remove(buffs.Last());
                buffs.RemoveAt(buffs.Count - 1);
                CheckLast();
            }
        }

        void SortBuffs()
        {
            buffs.Sort();
            Vector2 pos = StartPosition;
            int maxInX = (int)Math.Floor(RelativeSize.X / (buffSize.X + spacing.X + textSpacing.X) + spacing.X);
            int maxInY = (int)Math.Floor(RelativeSize.Y / (buffSize.Y + spacing.Y + textSpacing.Y) + spacing.Y);

            if (maxInX * maxInY < buffs.Count) throw new NotImplementedException(); //TODO: You probably should have fixed this by now xdd

            for (int i = 0; i < buffs.Count; i++)
            {
                buffs[i].Move(pos + StepPosition(i, maxInX, maxInY));
            }
        }

        Vector2 StepPosition(int i, int aMaxX, int aMaxY)
        {
            switch (fillDirection)
            {
                case FillDirection.TopRightToDown:
                    Vector2 xdd = Vector2.Zero;
                    xdd.X = (float)((buffSize.X + spacing.X + textSpacing.X) * -Math.Floor((double)i / aMaxY));
                    xdd.Y = ((buffSize.Y + spacing.Y + textSpacing.Y) * (i % aMaxY)); 
                    return xdd;
                default:
                    throw new NotImplementedException();
            }
        }

        public override void Draw(SpriteBatch aBatch)
        {
            base.Draw(aBatch);
        }
    }
}
